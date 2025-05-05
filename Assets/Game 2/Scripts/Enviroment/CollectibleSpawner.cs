using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CollectibleSpawner : MonoBehaviour
{
    [Header("Spawning Setup")] [Tooltip("A list of collectible prefabs to randomly choose from.")] [SerializeField]
    private List<GameObject> collectiblePrefabs = new List<GameObject>();

    [Tooltip("Minimum time (seconds) between collectible spawns.")] [SerializeField]
    private float minSpawnInterval = 3.0f;

    [Tooltip("Maximum time (seconds) between collectible spawns.")] [SerializeField]
    private float maxSpawnInterval = 7.0f;

    [Header("Spawn Position")]
    [Tooltip("The minimum Y coordinate (height) where collectibles will spawn.")]
    [SerializeField]
    private float minSpawnYPosition = 1.0f;

    [Tooltip("The maximum Y coordinate (height) where collectibles will spawn.")] [SerializeField]
    private float maxSpawnYPosition = 2.5f;

    [Tooltip("How far away along the spawner's forward (+Z) axis collectibles will spawn.")] [SerializeField]
    private float spawnZDistance = 40.0f;

    [Tooltip("The minimum X coordinate offset relative to the spawner's position.")] [SerializeField]
    private float minSpawnXOffset = -5.0f;

    [Tooltip("The maximum X coordinate offset relative to the spawner's position.")] [SerializeField]
    private float maxSpawnXOffset = 5.0f;


    private Coroutine _spawnCoroutine;

    void Start()
    {
        // Input Validation
        if (collectiblePrefabs == null || collectiblePrefabs.Count == 0 ||
            collectiblePrefabs.Any(prefab => prefab == null))
        {
            Debug.LogError(
                "CollectibleSpawner: Collectible Prefabs list is not assigned, is empty, or contains null elements!",
                this);
            if (collectiblePrefabs != null) collectiblePrefabs.RemoveAll(item => item == null);
            if (collectiblePrefabs == null || collectiblePrefabs.Count == 0)
            {
                enabled = false;
                return;
            }

            Debug.LogWarning("CollectibleSpawner: Proceeding with the valid prefabs in the list.", this);
        }

        // Interval/Offset Validation
        if (maxSpawnInterval < minSpawnInterval)
        {
            Debug.LogWarning("CollectibleSpawner: Max Spawn Interval should be >= Min Spawn Interval. Adjusting max.",
                this);
            maxSpawnInterval = minSpawnInterval;
        }

        if (maxSpawnXOffset < minSpawnXOffset)
        {
            Debug.LogWarning("CollectibleSpawner: Max Spawn X should be >= Min Spawn X. Adjusting max.", this);
            maxSpawnXOffset = minSpawnXOffset;
        }

        if (maxSpawnYPosition < minSpawnYPosition)
        {
            Debug.LogWarning("CollectibleSpawner: Max Spawn Y should be >= Min Spawn Y. Adjusting max.", this);
            maxSpawnYPosition = minSpawnYPosition;
        }

        // Start Spawning
        _spawnCoroutine = StartCoroutine(SpawnLoop());
        Debug.Log("CollectibleSpawner started.", this);
    }

    void OnDisable()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
            Debug.Log("CollectibleSpawner stopped.", this);
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            // Wait for a random amount of time within the defined interval
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Safety check in case the list becomes empty at runtime (unlikely but safe)
            if (collectiblePrefabs.Count == 0)
            {
                Debug.LogWarning("CollectibleSpawner: Prefab list is empty, cannot spawn.", this);
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Choose a random prefab from the list
            int randomIndex = Random.Range(0, collectiblePrefabs.Count);
            GameObject prefabToSpawn = collectiblePrefabs[randomIndex];

            // Safety check if a null entry somehow remained in the list
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"CollectibleSpawner: Prefab at index {randomIndex} is null, skipping this spawn.",
                    this);
                continue;
            }


            // Calculate Spawn Position
            float randomXOffset = Random.Range(minSpawnXOffset, maxSpawnXOffset);
            float randomYPosition = Random.Range(minSpawnYPosition, maxSpawnYPosition);

            // Calculate base position offset by X and Z relative to spawner's local axes
            Vector3 spawnPosition = transform.position
                                    + transform.right * randomXOffset // Offset left/right
                                    + transform.forward * spawnZDistance; // Offset forward
            spawnPosition.y = randomYPosition; // Set the height


            // Determine Spawn Rotation (Random)
            Quaternion spawnRotation = Random.rotation;

            // Instantiate the chosen collectible
            GameObject newCollectible = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        }
    }
}