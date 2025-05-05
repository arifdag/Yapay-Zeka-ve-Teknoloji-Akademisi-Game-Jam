using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class PrefabSpawner : MonoBehaviour
{
    [Header("General Spawning Setup")]
    [Tooltip("Minimum time (seconds) between spawn attempts *for each type* (flying/ground).")]
    [SerializeField]
    private float minSpawnInterval = 2.0f;

    [Tooltip("Maximum time (seconds) between spawn attempts *for each type* (flying/ground).")] [SerializeField]
    private float maxSpawnInterval = 5.0f;

    [Tooltip("How far away along the spawner's forward (+Z) axis objects will spawn.")] [SerializeField]
    private float spawnZDistance = 50.0f;

    [Tooltip("The minimum X coordinate offset relative to the spawner's position.")] [SerializeField]
    private float minSpawnXOffset = -20.0f;

    [Tooltip("The maximum X coordinate offset relative to the spawner's position.")] [SerializeField]
    private float maxSpawnXOffset = 20.0f;

    [Header("Flying Object Spawning")] [Tooltip("Enable spawning of flying objects?")] [SerializeField]
    private bool enableFlyingSpawning = true;

    [Tooltip("The list of flying prefabs to potentially spawn.")] [SerializeField]
    private List<GameObject> flyingPrefabs = new List<GameObject>();

    [Tooltip("The minimum Y coordinate (height) where flying objects will spawn.")] [SerializeField]
    private float minFlyingYPosition = 8.0f;

    [Tooltip("The maximum Y coordinate (height) where flying objects will spawn.")] [SerializeField]
    private float maxFlyingYPosition = 15.0f;

    [Tooltip("Rotation for flying objects. Adjust based on prefab orientation.")] [SerializeField]
    private Vector3 flyingObjectRotation = new Vector3(-90, 0, -180);

    [Header("Flying Object Behavior")]
    [Tooltip(
        "How long (in seconds) the spawned flying object should move straight (Requires a 'SpawnedObjectMovement' script).")]
    [SerializeField]
    private float flyingObjectMoveDuration = 10.0f;

    [Tooltip("How fast the spawned flying object should move (Requires a 'SpawnedObjectMovement' script).")]
    [SerializeField]
    private float flyingObjectSpeed = 30.0f;


    [Header("Ground Object Spawning")]
    [Tooltip("Enable spawning of objects at a fixed ground height?")]
    [SerializeField]
    private bool enableGroundSpawning = false;

    [Tooltip("The list of ground prefabs to potentially spawn.")] [SerializeField]
    private List<GameObject> groundPrefabs = new List<GameObject>();

    [Tooltip("The fixed Y coordinate (height) where ground objects will spawn.")] [SerializeField]
    private float groundSpawnYPosition = 0.5f;

    [Tooltip("Rotation for ground objects. Adjust based on prefab orientation. Uses spawner's Y rotation.")]
    [SerializeField]
    private Vector3 groundObjectRotation = Vector3.zero;

    [Header("Ground Object Behavior")] // Added for ground movement
    [Tooltip(
        "How long (in seconds) the spawned ground object should move straight (Requires a 'SpawnedObjectMovement' script).")]
    [SerializeField]
    private float groundObjectMoveDuration = 8.0f;

    [Tooltip("How fast the spawned ground object should move (Requires a 'SpawnedObjectMovement' script).")]
    [SerializeField]
    private float groundObjectSpeed = 20.0f;


    [Header("Common Visuals")]
    [Tooltip("Assign a random color to each spawned object (Requires a Renderer component)? Applies to both types.")]
    [SerializeField]
    private bool assignRandomColor = true;

    // --- Private Variables ---
    private Coroutine _flyingSpawnCoroutine; // Coroutine for flying objects
    private Coroutine _groundSpawnCoroutine; // Coroutine for ground objects
    private bool canSpawnFlying = false;
    private bool canSpawnGround = false;

    void Start()
    {
        ValidateIntervalsAndOffsets();

        if (enableFlyingSpawning)
        {
            canSpawnFlying = ValidatePrefabList(flyingPrefabs, "Flying Prefabs");
            if (canSpawnFlying)
            {
                CheckPrefabsForComponent<SpawnedObjectMovement>(flyingPrefabs, nameof(SpawnedObjectMovement),
                    "Warning: Flying prefab missing movement script.");
                if (assignRandomColor)
                    CheckPrefabsForComponent<Renderer>(flyingPrefabs, nameof(Renderer),
                        "Warning: Flying prefab missing Renderer for random color.");
            }
            else
            {
                Debug.LogWarning($"{nameof(PrefabSpawner)}: Flying spawning is enabled but list is empty/invalid.",
                    this);
            }
        }
        else
        {
            canSpawnFlying = false;
        }


        if (enableGroundSpawning)
        {
            canSpawnGround = ValidatePrefabList(groundPrefabs, "Ground Prefabs");
            if (canSpawnGround)
            {
                // Check for movement script on ground prefabs too
                CheckPrefabsForComponent<SpawnedObjectMovement>(groundPrefabs, nameof(SpawnedObjectMovement),
                    "Warning: Ground prefab missing movement script.");
                if (assignRandomColor)
                    CheckPrefabsForComponent<Renderer>(groundPrefabs, nameof(Renderer),
                        "Warning: Ground prefab missing Renderer for random color.");
            }
            else
            {
                Debug.LogWarning($"{nameof(PrefabSpawner)}: Ground spawning is enabled but list is empty/invalid.",
                    this);
            }
        }
        else
        {
            canSpawnGround = false;
        }

        // Final Check & Start Coroutines
        if (!canSpawnFlying && !canSpawnGround)
        {
            Debug.LogError(
                $"{nameof(PrefabSpawner)}: No valid spawning types enabled or configured. Disabling spawner.", this);
            enabled = false;
            return;
        }

        string startMessage = $"{nameof(PrefabSpawner)} started.";
        if (canSpawnFlying)
        {
            _flyingSpawnCoroutine = StartCoroutine(FlyingSpawnLoop());
            startMessage += " Flying Spawning Active.";
        }

        if (canSpawnGround)
        {
            _groundSpawnCoroutine = StartCoroutine(GroundSpawnLoop());
            startMessage += " Ground Spawning Active.";
        }

        Debug.Log(startMessage, this);
    }

    void OnDisable()
    {
        bool stoppedSomething = false;
        if (_flyingSpawnCoroutine != null)
        {
            StopCoroutine(_flyingSpawnCoroutine);
            _flyingSpawnCoroutine = null;
            stoppedSomething = true;
        }

        if (_groundSpawnCoroutine != null)
        {
            StopCoroutine(_groundSpawnCoroutine);
            _groundSpawnCoroutine = null;
            stoppedSomething = true;
        }

        if (stoppedSomething)
        {
            Debug.Log($"{nameof(PrefabSpawner)} stopped spawning coroutines.", this);
        }
    }


    private void ValidateIntervalsAndOffsets()
    {
        if (maxSpawnInterval < minSpawnInterval)
        {
            Debug.LogWarning(
                $"{nameof(PrefabSpawner)}: Max Spawn Interval should be >= Min Spawn Interval. Adjusting max.", this);
            maxSpawnInterval = minSpawnInterval;
        }

        if (maxSpawnXOffset < minSpawnXOffset)
        {
            Debug.LogWarning($"{nameof(PrefabSpawner)}: Max Spawn X should be >= Min Spawn X. Adjusting max.", this);
            maxSpawnXOffset = minSpawnXOffset;
        }

        if (maxFlyingYPosition < minFlyingYPosition) // Check flying Y range
        {
            Debug.LogWarning($"{nameof(PrefabSpawner)}: Max Flying Y should be >= Min Flying Y. Adjusting max.", this);
            maxFlyingYPosition = minFlyingYPosition;
        }
    }

    private bool ValidatePrefabList(List<GameObject> prefabList, string listName)
    {
        if (prefabList == null || prefabList.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < prefabList.Count; i++)
        {
            if (prefabList[i] == null)
            {
                Debug.LogError($"{nameof(PrefabSpawner)}: Entry {i} in the {listName} list is null!", this);
                return false;
            }
        }

        return true;
    }

    private void CheckPrefabsForComponent<T>(List<GameObject> prefabs, string componentName, string warningMessage)
        where T : Component
    {
        if (prefabs == null || prefabs.Count == 0) return;

        foreach (var prefab in prefabs)
        {
            if (prefab != null && prefab.GetComponentInChildren<T>() == null)
            {
                Debug.LogWarning(
                    $"{nameof(PrefabSpawner)}: {warningMessage} Prefab '{prefab.name}' has no {componentName} component in its hierarchy.",
                    this);
            }
        }
    }


    // Spawning Coroutines

    private IEnumerator FlyingSpawnLoop()
    {
        if (flyingPrefabs == null || flyingPrefabs.Count == 0) yield break;

        while (enabled)
        {
            // Wait independently
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Select Random Flying Prefab
            int randomIndex = Random.Range(0, flyingPrefabs.Count);
            GameObject prefabToSpawn = flyingPrefabs[randomIndex];
            if (prefabToSpawn == null)
            {
                /* Error already logged in validation */
                continue;
            } // Skip

            // Calculate Spawn Position & Rotation
            float randomXOffset = Random.Range(minSpawnXOffset, maxSpawnXOffset);
            float randomYPosition = Random.Range(minFlyingYPosition, maxFlyingYPosition);
            Vector3 spawnPosition = transform.position
                                    + transform.right * randomXOffset
                                    + transform.forward * spawnZDistance;
            spawnPosition.y = randomYPosition;
            Quaternion spawnRotation = Quaternion.Euler(flyingObjectRotation);

            // Instantiate
            GameObject newInstance = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
            newInstance.name = prefabToSpawn.name + "_FlyingInstance";

            // Configure Movement
            SpawnedObjectMovement movementScript = newInstance.GetComponent<SpawnedObjectMovement>();
            if (movementScript != null)
            {
                movementScript.Initialize(flyingObjectMoveDuration, flyingObjectSpeed);
            }

            // Assign Random Color
            AssignColorIfEnabled(newInstance);
        }
    }

    private IEnumerator GroundSpawnLoop()
    {
        if (groundPrefabs == null || groundPrefabs.Count == 0) yield break;

        while (enabled)
        {
            // Wait independently
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Select Random Ground Prefab
            int randomIndex = Random.Range(0, groundPrefabs.Count);
            GameObject prefabToSpawn = groundPrefabs[randomIndex];
            if (prefabToSpawn == null)
            {
                /* Error already logged in validation */
                continue;
            } // Skip

            // Calculate Spawn Position & Rotation
            float randomXOffset = Random.Range(minSpawnXOffset, maxSpawnXOffset);
            Vector3 spawnPosition = transform.position
                                    + transform.right * randomXOffset
                                    + transform.forward * spawnZDistance;
            spawnPosition.y = groundSpawnYPosition; // Fixed Y
            // Apply configured rotation relative to spawner's Y rotation
            Quaternion spawnRotation = Quaternion.Euler(groundObjectRotation.x,
                transform.eulerAngles.y + groundObjectRotation.y, groundObjectRotation.z);


            // Instantiate
            GameObject newInstance = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
            newInstance.name = prefabToSpawn.name + "_GroundInstance";

            // Configure Movement (Now for ground objects too!)
            SpawnedObjectMovement movementScript = newInstance.GetComponent<SpawnedObjectMovement>();
            if (movementScript != null)
            {
                // Use ground-specific duration and speed
                movementScript.Initialize(groundObjectMoveDuration, groundObjectSpeed);
            }

            // Assign Random Color
            AssignColorIfEnabled(newInstance);
        }
    }

    // Helper Methods
    private void AssignColorIfEnabled(GameObject instance)
    {
        if (assignRandomColor)
        {
            Renderer objectRenderer = instance.GetComponentInChildren<Renderer>();
            if (objectRenderer != null)
            {
                Color randomColor = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f, 1f, 1f);
                objectRenderer.material.color = randomColor;
            }
        }
    }


    // Gizmos Visualization
    void OnDrawGizmosSelected()
    {
        // Common calculations
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        float sizeX = maxSpawnXOffset - minSpawnXOffset;
        float sizeZ = 0.1f; // Small depth for visualization
        Vector3 centerBase = Vector3.forward * spawnZDistance +
                             Vector3.right * ((minSpawnXOffset + maxSpawnXOffset) / 2.0f);


        // Flying Spawn Area Gizmo
        if (enableFlyingSpawning && flyingPrefabs != null && flyingPrefabs.Count > 0)
        {
            Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.5f); // Cyan
            float midYFlying = (minFlyingYPosition + maxFlyingYPosition) / 2.0f;
            float sizeYFlying = maxFlyingYPosition - minFlyingYPosition;
            Vector3 localCenterFlying = centerBase + Vector3.up * (midYFlying - transform.position.y);
            Gizmos.DrawWireCube(localCenterFlying, new Vector3(sizeX, sizeYFlying, sizeZ));
        }

        // Ground Spawn Area Gizmo
        if (enableGroundSpawning && groundPrefabs != null && groundPrefabs.Count > 0)
        {
            Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.5f); // Orange
            float sizeYGround = 0.2f; // Thin line for visualization
            Vector3 localCenterGround = centerBase + Vector3.up * (groundSpawnYPosition - transform.position.y);
            // Draw a line instead of a box for single height
            Vector3 lineStart = localCenterGround - Vector3.right * (sizeX / 2.0f);
            Vector3 lineEnd = localCenterGround + Vector3.right * (sizeX / 2.0f);
            Gizmos.DrawLine(lineStart, lineEnd);
            // Optionally draw short vertical lines at ends
            Gizmos.DrawLine(lineStart, lineStart + Vector3.up * 0.5f);
            Gizmos.DrawLine(lineEnd, lineEnd + Vector3.up * 0.5f);
        }

        // Reset Gizmos matrix
        Gizmos.matrix = Matrix4x4.identity;
    }
}