using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("Assign the player GameObject here (must have the 'Player' tag or assign manually).")]
    [SerializeField]
    private Transform playerTransform;

    [Header("Prefabs")] [Tooltip("The prefab for a single road segment.")] [SerializeField]
    private GameObject roadPrefab;

    [Tooltip("The prefab for a single ground segment (optional).")] [SerializeField]
    private GameObject groundPrefab;

    [Tooltip("A list of building prefabs to randomly choose from.")] [SerializeField]
    private List<GameObject> buildingPrefabs = new List<GameObject>();

    [Header("Generation Settings")]
    [Tooltip("The length of a single road/ground segment along the Z-axis.")]
    [SerializeField]
    private float roadSegmentLength = 20f;

    [Tooltip("How far ahead of the player (along Z) should new segments be spawned.")] [SerializeField]
    private float spawnDistance = 60f;

    [Tooltip("How far behind the player (along Z) should old segments be destroyed.")] [SerializeField]
    private float despawnDistance = 40f;

    [Tooltip("The fixed X position for *regular* buildings on the right side.")] [SerializeField]
    private float buildingXOffsetRight = 10f;

    [Tooltip("The fixed X position for *regular* buildings on the left side.")] [SerializeField]
    private float buildingXOffsetLeft = -10f; // Should be negative

    [Tooltip("Minimum gap between buildings along the Z-axis.")] [SerializeField]
    private float minBuildingSpacingZ = 2f;

    [Tooltip("Minimum Y position for spawned buildings.")] [SerializeField]
    private float minBuildingY = 0f;

    [Tooltip("Maximum Y position for spawned buildings.")] [SerializeField]
    private float maxBuildingY = 0.5f;

    [Header("Special Building Settings")]
    [Tooltip("The exact name of the special large building prefab.")]
    [SerializeField]
    private string specialBuildingName = "Buildings_22";

    [Tooltip("Spawn the special building at least once every N *other* buildings on each side.")] [SerializeField]
    private int specialBuildingInterval = 20;

    [Tooltip(
        "How much *further* from the center (X=0) the special building should spawn compared to regular buildings.")]
    [SerializeField]
    private float specialBuildingExtraDistance = 5f;


    [Header("Initial Spawn")] [Tooltip("How many segments forward to initially spawn.")] [SerializeField]
    private int initialForwardSegments = 5;

    [Tooltip("How many segments backward to initially spawn.")] [SerializeField]
    private int initialBackwardSegments = 2;


    // Private Variables
    private float lastPlayerZ = -Mathf.Infinity;
    private float nextRoadSpawnZ = 0f;
    private float nextBuildingSpawnZRight = 0f;
    private float nextBuildingSpawnZLeft = 0f;

    private List<GameObject> spawnedRoads = new List<GameObject>();
    private List<GameObject> spawnedGrounds = new List<GameObject>();
    private List<GameObject> spawnedBuildings = new List<GameObject>();
    private Dictionary<GameObject, Bounds> buildingBoundsCache = new Dictionary<GameObject, Bounds>();

    private GameObject specialBuildingPrefabRef = null;
    private int buildingsSinceSpecialLeft = 0;
    private int buildingsSinceSpecialRight = 0;

    void Start()
    {
        // Find Player
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
            else
            {
                Debug.LogError("EnvMgr: Player not assigned & no 'Player' tag found! Disabling.");
                this.enabled = false;
                return;
            }
        }

        // Validate Required Prefabs
        if (roadPrefab == null)
        {
            Debug.LogError("EnvMgr: Road Prefab missing! Disabling.");
            this.enabled = false;
            return;
        }

        // Validate Optional / Lists
        if (groundPrefab == null) Debug.LogWarning("EnvMgr: Ground Prefab not assigned.");
        if (buildingPrefabs == null) buildingPrefabs = new List<GameObject>();
        buildingPrefabs.RemoveAll(item => item == null);
        if (buildingPrefabs.Count == 0) Debug.LogWarning("EnvMgr: Building Prefabs list empty.");

        // Validate Settings
        if (roadSegmentLength <= 0)
        {
            Debug.LogError("EnvMgr: Road Segment Length must be positive! Disabling.");
            this.enabled = false;
            return;
        }

        if (specialBuildingInterval <= 0)
        {
            Debug.LogWarning("EnvMgr: Special Building Interval <= 0. Disabling rule.");
            specialBuildingInterval = int.MaxValue;
        }

        if (buildingXOffsetLeft > 0)
            Debug.LogWarning(
                "EnvMgr: buildingXOffsetLeft is positive. Buildings on left will likely spawn on the wrong side of the road.");
        if (buildingXOffsetRight < 0)
            Debug.LogWarning(
                "EnvMgr: buildingXOffsetRight is negative. Buildings on right will likely spawn on the wrong side of the road.");
        if (specialBuildingExtraDistance < 0)
            Debug.LogWarning(
                "EnvMgr: specialBuildingExtraDistance is negative. Special building will spawn *closer* to the center.");

        // Find Special Building Prefab
        if (!string.IsNullOrEmpty(specialBuildingName) && buildingPrefabs.Count > 0)
        {
            specialBuildingPrefabRef = buildingPrefabs.FirstOrDefault(p => p != null && p.name == specialBuildingName);
            if (specialBuildingPrefabRef == null)
                Debug.LogWarning($"EnvMgr: Special building '{specialBuildingName}' not found in list.");
            else Debug.Log($"EnvMgr: Special building '{specialBuildingName}' found.");
        }

        // Pre-calculate Bounds
        CacheBuildingBounds();

        // Initial World Gen
        float startZ = Mathf.Floor(playerTransform.position.z / roadSegmentLength) * roadSegmentLength;
        nextRoadSpawnZ = startZ - (initialBackwardSegments * roadSegmentLength);
        nextBuildingSpawnZLeft = nextRoadSpawnZ;
        nextBuildingSpawnZRight = nextRoadSpawnZ;
        float initialSpawnEndZ = startZ + (initialForwardSegments * roadSegmentLength) + spawnDistance;
        while (nextRoadSpawnZ < initialSpawnEndZ) SpawnRoadSegment();
        while (buildingPrefabs.Count > 0 &&
               (nextBuildingSpawnZLeft < initialSpawnEndZ || nextBuildingSpawnZRight < initialSpawnEndZ))
        {
            bool sl = false;
            if (nextBuildingSpawnZLeft < initialSpawnEndZ)
            {
                SpawnBuilding(true);
                sl = true;
            }

            bool sr = false;
            if (nextBuildingSpawnZRight < initialSpawnEndZ)
            {
                SpawnBuilding(false);
                sr = true;
            }

            if (!sl && !sr) break;
        }

        lastPlayerZ = playerTransform.position.z;
        Debug.Log($"EnvMgr: Initial spawn done. Next Road/Ground Z: {nextRoadSpawnZ:F1}");
    }

    void Update()
    {
        if (playerTransform == null || !this.enabled) return;
        ManageWorld();
    }


    void ManageWorld()
    {
        float currentZ = playerTransform.position.z;
        float spawnTriggerZ = currentZ + spawnDistance;
        float despawnTriggerZ = currentZ - despawnDistance;

        while (nextRoadSpawnZ < spawnTriggerZ) SpawnRoadSegment(); // Spawn Roads & Ground
        if (buildingPrefabs.Count > 0)
        {
            // Spawn Buildings
            while (nextBuildingSpawnZLeft < spawnTriggerZ) SpawnBuilding(true);
            while (nextBuildingSpawnZRight < spawnTriggerZ) SpawnBuilding(false);
        }

        CleanupOldObjects(despawnTriggerZ); // Despawn
    }

    void SpawnRoadSegment()
    {
        Vector3 spawnPos = new Vector3(0, 0, nextRoadSpawnZ);
        if (roadPrefab != null)
            spawnedRoads.Add(Instantiate(roadPrefab, spawnPos, Quaternion.identity, this.transform));
        if (groundPrefab != null)
            spawnedGrounds.Add(Instantiate(groundPrefab, spawnPos, Quaternion.identity, this.transform));
        nextRoadSpawnZ += roadSegmentLength;
    }

    void SpawnBuilding(bool spawnOnLeft)
    {
        if (buildingPrefabs.Count == 0) return;

        GameObject prefabToSpawn = null;
        bool isSpecialSpawn = false;

        // Determine Prefab
        int currentCounter = spawnOnLeft ? buildingsSinceSpecialLeft : buildingsSinceSpecialRight;
        bool shouldForceSpecial = specialBuildingPrefabRef != null && currentCounter >= specialBuildingInterval;

        if (shouldForceSpecial)
        {
            prefabToSpawn = specialBuildingPrefabRef;
            isSpecialSpawn = true;
        }
        else
        {
            int prefabIndex = Random.Range(0, buildingPrefabs.Count);
            prefabToSpawn = buildingPrefabs[prefabIndex];
            if (specialBuildingPrefabRef != null && prefabToSpawn == specialBuildingPrefabRef)
            {
                isSpecialSpawn = true; // Randomly selected the special one
            }
        }

        if (prefabToSpawn == null)
        {
            // Safety check
            Debug.LogWarning("PrefabToSpawn is null, skipping.");
            float adv = minBuildingSpacingZ > 0 ? minBuildingSpacingZ : 1.0f;
            if (spawnOnLeft) nextBuildingSpawnZLeft += adv;
            else nextBuildingSpawnZRight += adv;
            return;
        }

        // Get Bounds & Z Length
        if (!buildingBoundsCache.TryGetValue(prefabToSpawn, out Bounds buildingBounds))
        {
            Debug.LogError($"Missing bounds for {prefabToSpawn.name}. Using default(1,1,1).");
            buildingBounds = new Bounds(Vector3.zero, Vector3.one);
        }

        float buildingLengthZ = buildingBounds.size.z;
        if (buildingLengthZ <= 0)
        {
            buildingLengthZ = 1.0f;
            Debug.LogWarning($"Zero Z bounds for {prefabToSpawn.name}.");
        }

        // Determine Spawn Position
        float spawnX;

        if (isSpecialSpawn)
        {
            // Calculate position *further away* from the center
            if (spawnOnLeft)
            {
                // Start with regular left offset (negative) and subtract extra distance (positive) -> more negative
                spawnX = buildingXOffsetLeft - specialBuildingExtraDistance;
            }
            else
            {
                // Start with regular right offset (positive) and add extra distance (positive) -> more positive
                spawnX = buildingXOffsetRight + specialBuildingExtraDistance;
            }
        }
        else
        {
            // Use regular offset
            spawnX = spawnOnLeft ? buildingXOffsetLeft : buildingXOffsetRight;
        }

        float spawnY = Random.Range(minBuildingY, maxBuildingY);
        float currentSpawnStartZoneZ = spawnOnLeft ? nextBuildingSpawnZLeft : nextBuildingSpawnZRight;
        float adjustedSpawnCenterZ = currentSpawnStartZoneZ + (buildingLengthZ / 2f);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, adjustedSpawnCenterZ);

        // Determine Rotation
        Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f); // Fixed rotation

        // Instantiate & Track
        GameObject newBuilding = Instantiate(prefabToSpawn, spawnPos, spawnRot, this.transform);
        spawnedBuildings.Add(newBuilding);

        // Update Counters & Next Spawn Z
        float nextSpawnStartZoneZ = currentSpawnStartZoneZ + buildingLengthZ + minBuildingSpacingZ;
        if (spawnOnLeft)
        {
            nextBuildingSpawnZLeft = nextSpawnStartZoneZ;
            if (isSpecialSpawn) buildingsSinceSpecialLeft = 0;
            else buildingsSinceSpecialLeft++;
        }
        else
        {
            nextBuildingSpawnZRight = nextSpawnStartZoneZ;
            if (isSpecialSpawn) buildingsSinceSpecialRight = 0;
            else buildingsSinceSpecialRight++;
        }
    }

    void CleanupOldObjects(float despawnTriggerZ)
    {
        CleanupList(spawnedRoads, despawnTriggerZ, true);
        CleanupList(spawnedGrounds, despawnTriggerZ, true);
        CleanupList(spawnedBuildings, despawnTriggerZ, false);
    }

    void CleanupList(List<GameObject> objectList, float triggerZ, bool useSegmentLength)
    {
        for (int i = objectList.Count - 1; i >= 0; i--)
        {
            GameObject obj = objectList[i];
            if (obj == null)
            {
                objectList.RemoveAt(i);
                continue;
            }

            float checkZ = obj.transform.position.z + (useSegmentLength ? roadSegmentLength : 0);
            if (checkZ < triggerZ)
            {
                Destroy(obj);
                objectList.RemoveAt(i);
            }
        }
    }

    void CacheBuildingBounds()
    {
        buildingBoundsCache.Clear();
        if (buildingPrefabs == null) return;
        foreach (GameObject prefab in buildingPrefabs)
        {
            if (prefab == null) continue;
            Bounds combinedBounds = new Bounds();
            bool init = false;
            Renderer[] renders = prefab.GetComponentsInChildren<Renderer>();
            Collider[] colls = prefab.GetComponentsInChildren<Collider>();
            if (renders.Length > 0)
            {
                combinedBounds = renders[0].bounds;
                init = true;
                for (int i = 1; i < renders.Length; i++) combinedBounds.Encapsulate(renders[i].bounds);
            }

            if (colls.Length > 0)
            {
                if (!init)
                {
                    combinedBounds = colls[0].bounds;
                    init = true;
                }

                for (int i = (init ? 0 : 1); i < colls.Length; i++) combinedBounds.Encapsulate(colls[i].bounds);
            }

            if (init) buildingBoundsCache[prefab] = combinedBounds;
            else
            {
                Debug.LogError($"No Renderer/Collider on '{prefab.name}'! Using default bounds.");
                buildingBoundsCache[prefab] = new Bounds(Vector3.zero, Vector3.one);
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3 managerPos = transform.position;
        Vector3 referencePos = Application.isPlaying && playerTransform != null ? playerTransform.position : managerPos;
        referencePos.y = managerPos.y;

        float specialXLeft = buildingXOffsetLeft - specialBuildingExtraDistance;
        float specialXRight = buildingXOffsetRight + specialBuildingExtraDistance;
        float gizmoWidth = Mathf.Max(5f, Mathf.Abs(buildingXOffsetLeft), buildingXOffsetRight, Mathf.Abs(specialXLeft),
            specialXRight) + 1f; // Take max absolute offset + buffer

        Gizmos.color = Color.green; // Spawn Zone
        Vector3 spawnLine = referencePos + Vector3.forward * spawnDistance;
        Gizmos.DrawLine(spawnLine + Vector3.left * gizmoWidth, spawnLine + Vector3.right * gizmoWidth);
        Gizmos.DrawWireSphere(spawnLine, 0.5f);

        Gizmos.color = Color.red; // Despawn Zone
        Vector3 despawnLine = referencePos - Vector3.forward * despawnDistance;
        Gizmos.DrawLine(despawnLine + Vector3.left * gizmoWidth, despawnLine + Vector3.right * gizmoWidth);
        Gizmos.DrawWireSphere(despawnLine, 0.5f);

        if (Application.isPlaying)
        {
            // Building markers
            float markerSize = 1.0f;
            float yGizmo = (minBuildingY + maxBuildingY) / 2f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(buildingXOffsetLeft, yGizmo, nextBuildingSpawnZLeft),
                Vector3.one * markerSize);
            Gizmos.DrawWireCube(new Vector3(buildingXOffsetRight, yGizmo, nextBuildingSpawnZRight),
                Vector3.one * markerSize);

            if (specialBuildingPrefabRef != null)
            {
                // Special
                Gizmos.color = Color.magenta;
                if (buildingsSinceSpecialLeft >= specialBuildingInterval)
                    Gizmos.DrawWireCube(new Vector3(specialXLeft, yGizmo, nextBuildingSpawnZLeft),
                        Vector3.one * markerSize * 1.1f);
                if (buildingsSinceSpecialRight >= specialBuildingInterval)
                    Gizmos.DrawWireCube(new Vector3(specialXRight, yGizmo, nextBuildingSpawnZRight),
                        Vector3.one * markerSize * 1.1f);
            }
        }
    }
}