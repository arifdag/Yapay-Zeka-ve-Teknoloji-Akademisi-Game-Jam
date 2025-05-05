using UnityEngine;
using System.Collections.Generic;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Prefab Listeleri")]
    public List<GameObject> naturePrefabs; // Aðaçlar, bitkiler vb.
    public List<GameObject> buildingPrefabs; // Evler, garajlar vb.

    [Header("Player Referansý")]
    public Transform player;

    [Header("Spawn Ayarlarý")]
    public float spawnDistanceAhead = 50f;
    public float spawnIntervalZ = 10f;

    [Header("X Pozisyonlarý")]
    public float leftBuildingX = -15f;  // Binalarýn sabit X konumu
    public float rightNatureMinX = 10f; // Aðaçlarýn minimum X konumu
    public float rightNatureMaxX = 20f; // Aðaçlarýn maksimum X konumu

    [Header("Building Spawn Offset")]
    [Tooltip("Binalarýn, player’e göre spawn Z offset’i (örneðin -10 ise, player’in arkasýnda spawn olur)")]
    public float buildingSpawnZOffset = -10f;

    private List<GameObject> activeEnvironmentObjects = new List<GameObject>();
    private float nextSpawnZ = 0f;
    private float lastNatureX = 0f; // Son doðan doða objesinin X konumu

    void Update()
    {
        while (player.position.z + spawnDistanceAhead >= nextSpawnZ)
        {
            SpawnBuilding(leftBuildingX); // Sol tarafa bina ekle
            SpawnNature(); // Sað tarafa rastgele doða objesi ekle
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

        // X pozisyonunu rastgele belirle, önceki nesneyle çakýþmadýðýndan emin ol
        float newNatureX;
        do
        {
            newNatureX = Random.Range(rightNatureMinX, rightNatureMaxX);
        } while (Mathf.Abs(newNatureX - lastNatureX) < 2f); // En az 2 birim aralýk býrak

        lastNatureX = newNatureX; // Son kullanýlan X pozisyonunu kaydet

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
