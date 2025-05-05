using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class WireframeOutline : MonoBehaviour
{
    [Tooltip("Kenarlara atayacaðýn LineRenderer materyali (Unlit/Color, yeþil olsun)")]
    public Material lineMaterial;

    [Tooltip("Çizgi kalýnlýðý (örneðin 0.05–0.2 arasý deneyebilirsin)")]
    public float lineWidth = 0.1f;

    void Start()
    {
        // 1) Mesh ve vertex/triangle verilerini al
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] tris = mesh.triangles;

        // 2) Kenarlarý (edges) say; her üçgen için 3 kenar
        var edgeCount = new Dictionary<(int, int), int>();
        for (int i = 0; i < tris.Length; i += 3)
        {
            var tri = new[] { tris[i], tris[i + 1], tris[i + 2] };
            // 3 kenarý ele al
            AddEdge(edgeCount, tri[0], tri[1]);
            AddEdge(edgeCount, tri[1], tri[2]);
            AddEdge(edgeCount, tri[2], tri[0]);
        }

        // 3) Sadece bir kere görünen (dýþ) kenarlar için LineRenderer yarat
        foreach (var kv in edgeCount)
        {
            if (kv.Value != 1) continue;           // iç diagonal kenarlar 2 kere sayýldý, onlarý atla

            int vA = kv.Key.Item1;
            int vB = kv.Key.Item2;

            // Yeni boþ GameObject, küpün child’ý olsun
            GameObject edgeGo = new GameObject("Edge");
            edgeGo.transform.SetParent(transform, false);

            // LineRenderer ekle ve ayarlarý yap
            var lr = edgeGo.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, vertices[vA]);
            lr.SetPosition(1, vertices[vB]);
            lr.material = lineMaterial;
            lr.startWidth = lr.endWidth = lineWidth;
            lr.useWorldSpace = false;
        }
    }

    // Kenar key’ini a<b olacak þekilde tutarak (1,2) ile (2,1)’i ayný kenar sayar
    void AddEdge(Dictionary<(int, int), int> dict, int a, int b)
    {
        var key = a < b ? (a, b) : (b, a);
        if (dict.ContainsKey(key)) dict[key]++;
        else dict[key] = 1;
    }
}
