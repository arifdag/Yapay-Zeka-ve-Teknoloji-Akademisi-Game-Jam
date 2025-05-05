using UnityEngine;
using System.Collections.Generic;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Prefab Listeleri")]
    public List<GameObject> naturePrefabs; // A�a�lar, bitkiler vb.
    public List<GameObject> buildingPrefabs; // Evler, garajlar vb.

    [Header("Player Referans�")]
    public Transform player;

    [Header("Spawn Ayarlar�")]
    public float spawnDistanceAhead = 50f;
    public float spawnIntervalZ = 10f;

    [Header("X Pozisyonlar�")]
    public float leftBuildingX = -15f;  // Binalar�n sabit X konumu
    public float rightNatureMinX = 10f; // A�a�lar�n minimum X konumu
    public float rightNatureMaxX = 20f; // A�a�lar�n maksimum X konumu

    [Header("Building Spawn Offset")]
    [Tooltip("Binalar�n, player�e g�re spawn Z offset�i (�rne�in -10 ise, player�in arkas�nda spawn olur)")]
    public float buildingSpawnZOffset = -10f;

    private List<GameObject> activeEnvironmentObjects = new List<GameObject>();
    private float nextSpawnZ = 0f;
    private float lastNatureX = 0f; // Son do�an do�a objesinin X konumu

    void Update()
    {
        while (player.position.z + spawnDistanceAhead >= nextSpawnZ)
        {
            SpawnBuilding(leftBuildingX); // Sol tarafa bina ekle
            SpawnNature(); // Sa� tarafa rastgele do�a objesi ekle
            nextSpawnZ += spawnIntervalZ;
        }

        for (int i = activeEnvironmentObjects.Count - 1; i >= 0; i--)
        {
            if (activeEnvironmentObjects[i].transform.position.z < player.position.z - 20f)
            {
                Destroy(activeEnvironmentObjects[i]);
                activeEnvironmentObjects.RemoveAt(i);
            }
        }
    }

    void SpawnBuilding(float xPos)
    {
        if (buildingPrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, buildingPrefabs.Count);
        GameObject prefabToSpawn = buildingPrefabs[randomIndex];

        // Bina spawn pozisyonunu, nextSpawnZ'e buildingSpawnZOffset ekleyerek belirliyoruz.
        Vector3 spawnPos = new Vector3(xPos, 0f, nextSpawnZ + buildingSpawnZOffset);
        ApplyPrefabOffsets(ref spawnPos, prefabToSpawn);

        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        activeEnvironmentObjects.Add(spawnedObject);
    }

    void SpawnNature()
    {
        if (naturePrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, naturePrefabs.Count);
        GameObject prefabToSpawn = naturePrefabs[randomIndex];

        // X pozisyonunu rastgele belirle, �nceki nesneyle �ak��mad���ndan emin ol
        float newNatureX;
        do
        {
            newNatureX = Random.Range(rightNatureMinX, rightNatureMaxX);
        } while (Mathf.Abs(newNatureX - lastNatureX) < 2f); // En az 2 birim aral�k b�rak

        lastNatureX = newNatureX; // Son kullan�lan X pozisyonunu kaydet

        Vector3 spawnPos = new Vector3(newNatureX, 0f, nextSpawnZ);
        ApplyPrefabOffsets(ref spawnPos, prefabToSpawn);

        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        activeEnvironmentObjects.Add(spawnedObject);
    }

    void ApplyPrefabOffsets(ref Vector3 position, GameObject prefab)
    {
        EnvironmentObjectSettings settings = prefab.GetComponent<EnvironmentObjectSettings>();
        if (settings != null)
        {
            position.x += settings.spawnOffsetX;
            position.y += settings.spawnOffsetY;
            position.z += settings.spawnOffsetZ;
        }
    }
}
