using UnityEngine;
using System.Collections.Generic;

public class ObstaclePlacer : MonoBehaviour
{
    [Header("Player & Lane Setup")]
    public Transform playerTransform;
    public float[] laneXPositions = { -2.5f, 0f, 2.5f }; // X positions for lanes 0, 1, 2

    [Header("Obstacle Prefabs")]
    // --- Single Lane ---
    public List<GameObject> prefabs_Jump_1Lane;
    public List<GameObject> prefabs_Slide_1Lane;
    // --- Double Lane (Adjacent) ---
    public List<GameObject> prefabs_Jump_2Lane; 
    public List<GameObject> prefabs_Slide_2Lane; 
    // --- Triple Lane ---
    public List<GameObject> prefabs_Jump_3Lane;
    public List<GameObject> prefabs_Slide_3Lane;

    [Header("Generation Control")]
    public float spawnDistanceAhead = 60f; // How far ahead of the player to spawn
    public float despawnDistanceBehind = 20f; // How far behind to despawn
    public float minSpacingZ = 15f; // Minimum distance between consecutive obstacle sets
    public float maxSpacingZ = 30f; // Maximum distance

    [Header("Placement Pattern Weights (Sum should ideally be > 0)")]
    [Range(0, 10)] public int weight_SingleObstacle = 5; // How often to place just one obstacle
    [Range(0, 10)] public int weight_DoubleAdjacent = 3; // How often to place a 2-lane obstacle
    [Range(0, 10)] public int weight_TripleObstacle = 1;  // How often to place a 3-lane obstacle
    [Range(0, 10)] public int weight_DoubleSeparate = 2; // How often to place obstacles in lanes 0 and 2
    [Range(0, 10)] public int weight_AdjacentMixed = 2; // How often place adjacent Jump & Slide

    // Private variables
    private float currentZGenerated = 0f;
    private Dictionary<float, List<GameObject>> activeObstacles = new Dictionary<float, List<GameObject>>();
    private List<GameObject> objectsToDespawn = new List<GameObject>(); 

    private int totalWeight;

    void Start()
    {
        // Validations
        if (playerTransform == null) {
            Debug.LogError("ObstaclePlacer: Player Transform not assigned!", this);
            this.enabled = false; return;
        }
        if (laneXPositions == null || laneXPositions.Length != 3) {
            Debug.LogError("ObstaclePlacer: Lane X Positions must be an array of size 3!", this);
            this.enabled = false; return;
        }
        CalculateTotalWeight();
        if (totalWeight <= 0) {
            Debug.LogWarning("ObstaclePlacer: All pattern weights are zero. No obstacles will be placed.", this);
        }

        // Initialize generation point slightly ahead of player start
        currentZGenerated = playerTransform.position.z + minSpacingZ;
    }

    void Update()
    {
        // Spawn Check
        if (playerTransform.position.z + spawnDistanceAhead > currentZGenerated)
        {
            SpawnObstacleSet();
        }

        // Despawn Check
        DespawnObstacles();
    }

    void CalculateTotalWeight()
    {
        totalWeight = weight_SingleObstacle + weight_DoubleAdjacent + weight_TripleObstacle +
                      weight_DoubleSeparate + weight_AdjacentMixed;
    }

    void SpawnObstacleSet()
    {
        // Determine Spawn Z
        float nextSpawnZ = currentZGenerated + Random.Range(minSpacingZ, maxSpacingZ);
        activeObstacles.Add(nextSpawnZ, new List<GameObject>()); // Prepare list for this Z

        // Choose Placement Pattern based on Weights
        if (totalWeight <= 0) { // No patterns possible if weights are zero
            currentZGenerated = nextSpawnZ; // Still advance Z to prevent infinite loop
            return;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        // Pattern Logic

        // Single Obstacle
        cumulativeWeight += weight_SingleObstacle;
        if (randomWeight < cumulativeWeight)
        {
            PlaceSingleObstacle(nextSpawnZ);
            currentZGenerated = nextSpawnZ;
            return;
        }

        // Double Adjacent Obstacle
        cumulativeWeight += weight_DoubleAdjacent;
        if (randomWeight < cumulativeWeight)
        {
            PlaceDoubleAdjacentObstacle(nextSpawnZ);
            currentZGenerated = nextSpawnZ;
            return;
        }

        // Triple Lane Obstacle
        cumulativeWeight += weight_TripleObstacle;
        if (randomWeight < cumulativeWeight)
        {
            PlaceTripleObstacle(nextSpawnZ);
            currentZGenerated = nextSpawnZ;
            return;
        }

        // Double Separate (Lanes 0 & 2)
        cumulativeWeight += weight_DoubleSeparate;
        if (randomWeight < cumulativeWeight)
        {
            PlaceDoubleSeparateObstacle(nextSpawnZ);
            currentZGenerated = nextSpawnZ;
            return;
        }

        // Adjacent Mixed (Jump + Slide)
        cumulativeWeight += weight_AdjacentMixed;
        if (randomWeight < cumulativeWeight)
        {
            PlaceAdjacentMixedObstacle(nextSpawnZ);
            currentZGenerated = nextSpawnZ;
            return;
        }

        // Fallback (shouldn't happen if weights are correct, but good practice)
        Debug.LogWarning("ObstaclePlacer: Fell through pattern selection. Check weights.", this);
        currentZGenerated = nextSpawnZ;
    }

    // Placement Pattern Functions

    void PlaceSingleObstacle(float spawnZ)
    {
        int lane = Random.Range(0, 3);
        // 50/50 chance for Jump or Slide
        GameObject prefab = (Random.value < 0.5f) ? GetRandomPrefab(prefabs_Jump_1Lane) : GetRandomPrefab(prefabs_Slide_1Lane);
        InstantiateObstacle(prefab, lane, spawnZ);
    }

    void PlaceDoubleAdjacentObstacle(float spawnZ)
    {
        int startLane = Random.Range(0, 2); // 0 (for lanes 0,1) or 1 (for lanes 1,2)
        // Use the middle X position of the two lanes
        float posX = (laneXPositions[startLane] + laneXPositions[startLane + 1]) / 2f;

        GameObject prefab = (Random.value < 0.5f) ? GetRandomPrefab(prefabs_Jump_2Lane) : GetRandomPrefab(prefabs_Slide_2Lane);
        InstantiateObstacle(prefab, posX, spawnZ); // Use specific X pos instead of lane index
    }

    void PlaceTripleObstacle(float spawnZ)
    {
        // Place at center lane position (index 1)
        GameObject prefab = (Random.value < 0.5f) ? GetRandomPrefab(prefabs_Jump_3Lane) : GetRandomPrefab(prefabs_Slide_3Lane);
        InstantiateObstacle(prefab, 1, spawnZ);
    }

    void PlaceDoubleSeparateObstacle(float spawnZ)
    {
        // Place one in lane 0, one in lane 2. Leaves lane 1 clear.
        // Can be Jump/Jump, Slide/Slide, or Jump/Slide
        float typeRoll = Random.value;

        if (typeRoll < 0.4f) { // Jump/Jump
            InstantiateObstacle(GetRandomPrefab(prefabs_Jump_1Lane), 0, spawnZ);
            InstantiateObstacle(GetRandomPrefab(prefabs_Jump_1Lane), 2, spawnZ);
        } else if (typeRoll < 0.8f) { // Slide/Slide
            InstantiateObstacle(GetRandomPrefab(prefabs_Slide_1Lane), 0, spawnZ);
            InstantiateObstacle(GetRandomPrefab(prefabs_Slide_1Lane), 2, spawnZ);
        } else { // Jump/Slide
            if (Random.value < 0.5f) { // Jump Lane 0, Slide Lane 2
                InstantiateObstacle(GetRandomPrefab(prefabs_Jump_1Lane), 0, spawnZ);
                InstantiateObstacle(GetRandomPrefab(prefabs_Slide_1Lane), 2, spawnZ);
            } else { // Slide Lane 0, Jump Lane 2
                InstantiateObstacle(GetRandomPrefab(prefabs_Slide_1Lane), 0, spawnZ);
                InstantiateObstacle(GetRandomPrefab(prefabs_Jump_1Lane), 2, spawnZ);
            }
        }
    }

     void PlaceAdjacentMixedObstacle(float spawnZ)
    {
        int startLane = Random.Range(0, 2); // 0 (for lanes 0,1) or 1 (for lanes 1,2)
        // Place Jump in one, Slide in the other adjacent lane. Forces player to the 3rd lane.
         if (Random.value < 0.5f) { // Jump first, Slide second
            InstantiateObstacle(GetRandomPrefab(prefabs_Jump_1Lane), startLane, spawnZ);
            InstantiateObstacle(GetRandomPrefab(prefabs_Slide_1Lane), startLane + 1, spawnZ);
         } else { // Slide first, Jump second
            InstantiateObstacle(GetRandomPrefab(prefabs_Slide_1Lane), startLane, spawnZ);
            InstantiateObstacle(GetRandomPrefab(prefabs_Jump_1Lane), startLane + 1, spawnZ);
         }
    }


    // Helper Functions

    GameObject GetRandomPrefab(List<GameObject> prefabList)
    {
        if (prefabList == null || prefabList.Count == 0)
        {
            Debug.LogWarning($"ObstaclePlacer: Prefab list is empty or null. Cannot spawn.", this);
            return null;
        }
        return prefabList[Random.Range(0, prefabList.Count)];
    }

    void InstantiateObstacle(GameObject prefab, int laneIndex, float spawnZ)
    {
        if (prefab == null) return;
        if (laneIndex < 0 || laneIndex >= laneXPositions.Length)
        {
            Debug.LogError($"ObstaclePlacer: Invalid Lane Index {laneIndex}", this);
            return;
        }

        Vector3 spawnPos = new Vector3(laneXPositions[laneIndex], prefab.transform.position.y, spawnZ); // Use prefab's Y
        GameObject instance = Instantiate(prefab, spawnPos, prefab.transform.rotation, transform); // Parent to this GO

        // Add to active list for despawning
        if (activeObstacles.ContainsKey(spawnZ))
        {
            activeObstacles[spawnZ].Add(instance);
        }
    }

    // Overload for specific X position (used for 2-lane obstacles)
    void InstantiateObstacle(GameObject prefab, float spawnX, float spawnZ)
    {
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(spawnX, prefab.transform.position.y, spawnZ); // Use prefab's Y
        GameObject instance = Instantiate(prefab, spawnPos, prefab.transform.rotation, transform); // Parent to this GO

        // Add to active list for despawning
         if (activeObstacles.ContainsKey(spawnZ))
        {
            activeObstacles[spawnZ].Add(instance);
        }
    }

    void DespawnObstacles()
    {
        objectsToDespawn.Clear(); // Use a temporary list to collect objects for safe removal
        List<float> keysToRemove = new List<float>();

        float despawnZ = playerTransform.position.z - despawnDistanceBehind;

        foreach (KeyValuePair<float, List<GameObject>> entry in activeObstacles)
        {
            // If the Z position where this set was spawned is behind the despawn threshold
            if (entry.Key < despawnZ)
            {
                objectsToDespawn.AddRange(entry.Value); // Add all objects from this Z position to the despawn list
                keysToRemove.Add(entry.Key); // Mark this Z key for removal from the dictionary
            }
        }

        // Destroy the collected objects
        foreach (GameObject obj in objectsToDespawn)
        {
            if (obj != null) // Check if it wasn't somehow destroyed already
            {
                Destroy(obj);
            }
        }

        // Remove the entries from the active obstacles dictionary
        foreach (float key in keysToRemove)
        {
            activeObstacles.Remove(key);
        }
    }

     // Visualize spawn and despawn zones in the editor
    void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Vector3 spawnLinePos = playerTransform.position + Vector3.forward * spawnDistanceAhead;
            Vector3 despawnLinePos = playerTransform.position - Vector3.forward * despawnDistanceBehind;
            float lineLength = (laneXPositions.Length > 0) ? Mathf.Abs(laneXPositions[0] - laneXPositions[laneXPositions.Length - 1]) + 5f : 10f;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnLinePos + Vector3.left * lineLength/2, spawnLinePos + Vector3.right * lineLength/2);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(despawnLinePos + Vector3.left * lineLength/2, despawnLinePos + Vector3.right * lineLength/2);

             // Draw line showing how far generated
            Gizmos.color = Color.magenta;
            Vector3 generatedLinePos = new Vector3(0, playerTransform.position.y , currentZGenerated);
            Gizmos.DrawLine(generatedLinePos + Vector3.left * lineLength/2, generatedLinePos + Vector3.right * lineLength/2);
        }
    }
}