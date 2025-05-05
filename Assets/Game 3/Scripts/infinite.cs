using UnityEngine;
using System.Collections.Generic;

public class EndlessRoadManager : MonoBehaviour
{
    public GameObject roadPrefab;
    public GameObject[] obstacles;
    public Transform player;
    public float fixedXPosition = 0f; // Sabit X pozisyonu
    public float spawnInterval = 20.0f; // Inspector'dan ayarlanabilir spawn aral���
    public float obstacleLifetime = 50.0f; // Inspector'dan ayarlanabilir obstacle yok olma s�resi
    public float obstacleSpawnDistance = 5.0f; // Obstacle'�n her 5 metrede bir do�mas�
    public float obstacleSpawnY = 1.0f; // Obstacle'�n Y eksenindeki pozisyonu, Inspector'dan ayarlanabilir

    [Header("Obstacle Spawn Mesafesi")]
    [Tooltip("Karakterden ne kadar ileriye kadar obstacle spawn edilecek")]
    public float obstacleSpawnDistanceAhead = 150.0f; // Yeni eklenen de�i�ken

    private float spawnZ = 5.0f;
    private float roadLength = 70.0f;
    private int numRoadsOnScreen = 3;
    private float safeZone = 100.0f;

    private List<GameObject> activeRoads;
    private List<GameObject> activeObstacles;
    private float nextObstacleSpawnZ = 0f; // Bir sonraki obstacle'�n spawn olaca�� Z pozisyonu

    void Start()
    {
        activeRoads = new List<GameObject>();
        activeObstacles = new List<GameObject>();

        for (int i = 0; i < numRoadsOnScreen; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        if (player.position.z - safeZone > (spawnZ - numRoadsOnScreen * roadLength))
        {
            RecycleRoad();
            SpawnRoad();
        }

        SpawnObstacles();
        DestroyPassedObstacles();
    }

    private void SpawnRoad()
    {
        GameObject road = Instantiate(roadPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
        activeRoads.Add(road);
        spawnZ += roadLength;
    }

    private void SpawnObstacles()
    {
        // Obstacle'lar�, karakterin �n�nde belirledi�imiz mesafeye kadar spawn ediyoruz.
        while (nextObstacleSpawnZ < player.position.z + obstacleSpawnDistanceAhead)
        {
            if (obstacles.Length > 0)
            {
                int randomIndex = Random.Range(0, obstacles.Length);
                GameObject selectedObstacle = obstacles[randomIndex];

                Vector3 obstaclePosition = new Vector3(fixedXPosition, obstacleSpawnY, nextObstacleSpawnZ);
                GameObject spawnedObstacle = Instantiate(selectedObstacle, obstaclePosition, Quaternion.identity);
                activeObstacles.Add(spawnedObstacle);

                // Obstacle yok olma s�resi Inspector panelinden ayarlanabilir
                Destroy(spawnedObstacle, obstacleLifetime);
            }

            // Bir sonraki obstacle spawn noktas�
            nextObstacleSpawnZ += obstacleSpawnDistance;
        }
    }

    private void RecycleRoad()
    {
        GameObject road = activeRoads[0];
        activeRoads.RemoveAt(0);
        road.transform.position = new Vector3(0, 0, spawnZ);
        activeRoads.Add(road);
        spawnZ += roadLength;
    }

    private void DestroyPassedObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] != null && activeObstacles[i].transform.position.z < player.position.z - spawnInterval)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}
