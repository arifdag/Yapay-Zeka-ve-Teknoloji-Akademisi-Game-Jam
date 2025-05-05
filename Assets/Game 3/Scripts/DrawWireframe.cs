using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class WireframeOutline : MonoBehaviour
{
    [Tooltip("Kenarlara atayaca��n LineRenderer materyali (Unlit/Color, ye�il olsun)")]
    public Material lineMaterial;

    [Tooltip("�izgi kal�nl��� (�rne�in 0.05�0.2 aras� deneyebilirsin)")]
    public float lineWidth = 0.1f;

    void Start()
    {
        // 1) Mesh ve vertex/triangle verilerini al
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] tris = mesh.triangles;

        // 2) Kenarlar� (edges) say; her ��gen i�in 3 kenar
        var edgeCount = new Dictionary<(int, int), int>();
        for (int i = 0; i < tris.Length; i += 3)
        {
            var tri = new[] { tris[i], tris[i + 1], tris[i + 2] };
            // 3 kenar� ele al
            AddEdge(edgeCount, tri[0], tri[1]);
            AddEdge(edgeCount, tri[1], tri[2]);
            AddEdge(edgeCount, tri[2], tri[0]);
        }

        // 3) Sadece bir kere g�r�nen (d��) kenarlar i�in LineRenderer yarat
        foreach (var kv in edgeCount)
        {
            if (kv.Value != 1) continue;           // i� diagonal kenarlar 2 kere say�ld�, onlar� atla

            int vA = kv.Key.Item1;
            int vB = kv.Key.Item2;

            // Yeni bo� GameObject, k�p�n child�� olsun
            GameObject edgeGo = new GameObject("Edge");
            edgeGo.transform.SetParent(transform, false);

            // LineRenderer ekle ve ayarlar� yap
            var lr = edgeGo.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, vertices[vA]);
            lr.SetPosition(1, vertices[vB]);
            lr.material = lineMaterial;
            lr.startWidth = lr.endWidth = lineWidth;
            lr.useWorldSpace = false;
        }
    }

    // Kenar key�ini a<b olacak �ekilde tutarak (1,2) ile (2,1)�i ayn� kenar sayar
    void AddEdge(Dictionary<(int, int), int> dict, int a, int b)
    {
        var key = a < b ? (a, b) : (b, a);
        if (dict.ContainsKey(key)) dict[key]++;
        else dict[key] = 1;
    }
}
