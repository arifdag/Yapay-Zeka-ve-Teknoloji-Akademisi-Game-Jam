using UnityEngine;
using System.Collections.Generic;

public class ProceduralWorldGenerator : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform; 

    [Header("Prefabs")]
    public GameObject roadPrefab;       // Prefab for the central road segment
    public GameObject groundPrefab;     // Prefab for the ground segment under the road
    public List<GameObject> buildingPrefabs; // List of prefabs to spawn on the sides

    [Header("Generation Control")]
    public float spawnDistance = 400f; // How far ahead to generate segments
    public float despawnDistance = 100f; // How far behind to destroy segments
    [Tooltip("How many segments to generate initially at Start")]
    public int initialSegments = 10;

    [Header("Road & Ground Settings")]
    [Tooltip("The length of one road/ground prefab segment along the Z-axis")]
    public float segmentLength = 50f;
    public float groundYPosition = -1f;

    [Header("Building Placement Settings")]
    [Tooltip("X position for buildings on the player's left (negative value)")]
    public float buildingXPositionLeft = -15f;
    [Tooltip("X position for buildings on the player's right (positive value)")]
    public float buildingXPositionRight = 15f;
    [Tooltip("Vertical position (Y) for the base of the buildings")]
    public float buildingYPosition = 0f;
    [Tooltip("Minimum distance between buildings along the Z-axis")]
    public float buildingSpacingZ = 10f; // Distance between consecutive buildings
    [Tooltip("Minimum scale multiplier for buildings")]
    public float buildingMinScale = 0.8f;
    [Tooltip("Maximum scale multiplier for buildings")]
    public float buildingMaxScale = 1.2f;


    // Private variables
    private float currentZGenerated = 0f; // Tracks how far we've generated
    private float lastLeftBuildingZ = 0f; // Tracks the Z position of the last placed left building
    private float lastRightBuildingZ = 0f; // Tracks the Z position of the last placed right building

    // Store active segments for despawning
    private Dictionary<float, List<GameObject>> activeSegments = new Dictionary<float, List<GameObject>>();

    void Start()
    {
        // Basic validation
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform not assigned!", this);
            this.enabled = false;
            return;
        }
        if (roadPrefab == null || groundPrefab == null)
        {
            Debug.LogError("Road or Ground Prefab not assigned!", this);
            this.enabled = false;
            return;
        }
        if (buildingPrefabs == null || buildingPrefabs.Count == 0)
        {
            Debug.LogWarning("No Building Prefabs assigned. Only road and ground will be generated.", this);
        }
        if (segmentLength <= 0)
        {
            Debug.LogError("Segment Length must be greater than zero!", this);
            segmentLength = 50f;
        }
        if (buildingSpacingZ <= 0) {
            Debug.LogWarning("Building Spacing Z is zero or negative. Buildings might overlap heavily.", this);
            buildingSpacingZ = 1f; // Prevent division by zero issues potentially
        }


        // Initialize last building positions slightly behind start to ensure first placement
        lastLeftBuildingZ = -buildingSpacingZ;
        lastRightBuildingZ = -buildingSpacingZ;

        // Generate initial segments
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        // Check if we need to generate more segments
        if (playerTransform.position.z + spawnDistance > currentZGenerated)
        {
            SpawnSegment();
        }

        // Check if we need to despawn old segments
        DespawnSegments();
    }

    void SpawnSegment()
    {
        // Instantiate Road and Ground
        Vector3 roadSpawnPos = new Vector3(0, 0, currentZGenerated);
        GameObject roadInstance = Instantiate(roadPrefab, roadSpawnPos, Quaternion.identity, transform); // Parent to this object

        Vector3 groundSpawnPos = new Vector3(0, groundYPosition, currentZGenerated);
        GameObject groundInstance = Instantiate(groundPrefab, groundSpawnPos, Quaternion.identity, transform); // Parent to this object

        // Keep track of spawned objects for this segment Z position
        float segmentStartZ = currentZGenerated;
        if (!activeSegments.ContainsKey(segmentStartZ))
        {
            activeSegments.Add(segmentStartZ, new List<GameObject>());
        }
        activeSegments[segmentStartZ].Add(roadInstance);
        activeSegments[segmentStartZ].Add(groundInstance);


        // Instantiate Buildings
        if (buildingPrefabs != null && buildingPrefabs.Count > 0)
        {
            float segmentEndZ = currentZGenerated + segmentLength;

            // Place Left Buildings within this segment
            while (lastLeftBuildingZ + buildingSpacingZ < segmentEndZ)
            {
                lastLeftBuildingZ += buildingSpacingZ;
                // Ensure building is actually within the current segment's bounds (or starts within it)
                if (lastLeftBuildingZ >= segmentStartZ)
                {
                   PlaceBuilding(buildingXPositionLeft, lastLeftBuildingZ, segmentStartZ);
                }
            }

            // Place Right Buildings within this segment
            while (lastRightBuildingZ + buildingSpacingZ < segmentEndZ)
            {
                 lastRightBuildingZ += buildingSpacingZ;
                 // Ensure building is actually within the current segment's bounds (or starts within it)
                if (lastRightBuildingZ >= segmentStartZ)
                {
                    PlaceBuilding(buildingXPositionRight, lastRightBuildingZ, segmentStartZ);
                }
            }
        }

        // Update the generation tracker
        currentZGenerated += segmentLength;
    }

    void PlaceBuilding(float xPos, float zPos, float segmentKeyZ)
    {
        // Select a random building prefab
        int randomIndex = Random.Range(0, buildingPrefabs.Count);
        GameObject prefabToSpawn = buildingPrefabs[randomIndex];

        if (prefabToSpawn != null)
        {
            Vector3 buildingPos = new Vector3(xPos, buildingYPosition, zPos);
            GameObject buildingInstance = Instantiate(prefabToSpawn, buildingPos, Quaternion.identity, transform);

            // Apply random scale
            float randomScale = Random.Range(buildingMinScale, buildingMaxScale);
            buildingInstance.transform.localScale = Vector3.one * randomScale;

            // Add to the active segments list for despawning later
            if (activeSegments.ContainsKey(segmentKeyZ))
            {
                activeSegments[segmentKeyZ].Add(buildingInstance);
            }
             else
            {
                // This case shouldn't strictly happen with the current logic, but safety first
                Debug.LogWarning($"Trying to add building to non-existent segment key {segmentKeyZ}");
                // Create the list if it somehow doesn't exist yet
                activeSegments.Add(segmentKeyZ, new List<GameObject> { buildingInstance });
            }
        }
    }


    void DespawnSegments()
    {
        // Use a temporary list to avoid modifying the dictionary while iterating
        List<float> segmentsToRemove = new List<float>();

        foreach (KeyValuePair<float, List<GameObject>> entry in activeSegments)
        {
            // The key (entry.Key) is the starting Z position of the segment
            float segmentStartZ = entry.Key;
            float segmentEndZ = segmentStartZ + segmentLength; // Estimate end based on start

            // Check if the *entire* segment is behind the despawn distance
            if (segmentEndZ < playerTransform.position.z - despawnDistance)
            {
                // Destroy all GameObjects associated with this segment Z position
                foreach (GameObject obj in entry.Value)
                {
                    Destroy(obj);
                }
                // Mark this segment's Z key for removal from the dictionary
                segmentsToRemove.Add(segmentStartZ);
            }
        }

        // Remove the marked segments from the active list
        foreach (float key in segmentsToRemove)
        {
            activeSegments.Remove(key);
        }
    }

    // Visualize spawn and despawn zones in the editor
    void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Vector3 spawnLinePos = playerTransform.position + Vector3.forward * spawnDistance;
            Vector3 despawnLinePos = playerTransform.position - Vector3.forward * despawnDistance;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(spawnLinePos + Vector3.left * 50, spawnLinePos + Vector3.right * 50);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(despawnLinePos + Vector3.left * 50, despawnLinePos + Vector3.right * 50);

             // Draw line showing how far generated
            Gizmos.color = Color.blue;
            Vector3 generatedLinePos = new Vector3(0,0, currentZGenerated);
            Gizmos.DrawLine(generatedLinePos + Vector3.left * 50, generatedLinePos + Vector3.right * 50);


        }
    }
}