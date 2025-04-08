using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for checking prefab types easily

public class BlockSpawner : MonoBehaviour
{
    [Header("Shape Prefabs")]
    public GameObject blockPrefab;
    public GameObject cylinderPrefab;
    public GameObject conePrefab; // Added Cone Prefab

    [Header("Spawning Settings")]
    [Tooltip("The total maximum number of shapes (blocks + cylinders + maybe a cone) to generate for the preview/goal.")]
    public int totalNumberOfShapes = 6; // Default value - NOW primarily used by GameManager to determine target list size
    [Tooltip("Probability (0 to 1) of including a cone in the target stack definition.")]
    [Range(0f, 1f)]
    public float spawnConeChance = 0.5f; // NOW used by GameManager to decide if cone is in target list

    [Header("Block Spawn Area")]
    public Vector3 blockSpawnOrigin = new Vector3(-2, 1, 2);
    public Vector3 blockSpawnOffset = new Vector3(1.5f, 0, 0);

    [Header("Cylinder Spawn Area")]
    public Vector3 cylinderSpawnOrigin = new Vector3(-2, 1, -2);
    public Vector3 cylinderSpawnOffset = new Vector3(1.5f, 0, 0);

    [Header("Cone Spawn Area")] // Added Cone Spawn Area
    public Vector3 coneSpawnOrigin = new Vector3(0, 1, 0); // Example origin
    // Cones often spawn individually, but offset is available if needed
    public Vector3 coneSpawnOffset = new Vector3(0, 0, 0); // Usually not needed if only one cone spawns

    private List<GameObject> spawnedObjects = new List<GameObject>();
    public List<GameObject> SpawnedObjects => spawnedObjects;

    /*
    /// <summary>
    /// Spawns a mix of blocks, cylinders, and potentially one cone,
    /// totaling 'totalNumberOfShapes'.
    /// Clears any previously spawned objects.
    /// </summary>
    /// <returns>A list containing all spawned shapes.</returns>
    public List<GameObject> SpawnRandomizedShapes()
    {
        ClearSpawnedObjects();

        // --- 1. Validate Inputs ---
        if (blockPrefab == null || cylinderPrefab == null || conePrefab == null) // Check cone prefab too
        {
            Debug.LogError("Block, Cylinder, or Cone prefab is not assigned in BlockSpawner!", this);
            return spawnedObjects;
        }
        if (totalNumberOfShapes <= 0)
        {
            Debug.LogWarning("Total Number of Shapes is zero or less. No shapes will be spawned.", this);
            return spawnedObjects;
        }

        // --- 2. Decide if Cone Spawns ---
        bool spawnCone = Random.value < spawnConeChance;
        int shapesToSpawn = totalNumberOfShapes;
        GameObject spawnedCone = null; // Keep track if cone was spawned

        if (spawnCone && totalNumberOfShapes >= 1)
        {
            // Spawn the cone
            Vector3 conePosition = coneSpawnOrigin; // Using only origin for a single cone
            spawnedCone = Instantiate(conePrefab, conePosition, Quaternion.identity);
            spawnedObjects.Add(spawnedCone);
            shapesToSpawn--; // Reduce the count for other shapes
            Debug.Log("Spawning 1 Cone.");
        }
        else
        {
            Debug.Log("Not spawning a Cone this round.");
        }


        // --- 3. Spawn Remaining Blocks and Cylinders Randomly ---
        int blocksSpawned = 0;
        int cylindersSpawned = 0;

        // New Randomness: Iterate and decide each shape type
        for (int i = 0; i < shapesToSpawn; i++)
        {
            // Simple 50/50 split for remaining shapes, adjust if needed
            if (Random.value < 0.5f)
            {
                // Spawn Block
                Vector3 position = blockSpawnOrigin + blocksSpawned * blockSpawnOffset;
                GameObject block = Instantiate(blockPrefab, position, Quaternion.identity);
                spawnedObjects.Add(block);
                blocksSpawned++;
            }
            else
            {
                // Spawn Cylinder
                Vector3 position = cylinderSpawnOrigin + cylindersSpawned * cylinderSpawnOffset;
                GameObject cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity);
                spawnedObjects.Add(cylinder);
                cylindersSpawned++;
            }
        }

        Debug.Log($"Spawning {blocksSpawned} blocks and {cylindersSpawned} cylinders (Along with {(spawnedCone != null ? 1 : 0)} cone). Total: {spawnedObjects.Count}");

        // Optional: Shuffle if the initial spawn order should be random
        // ShuffleList(spawnedObjects);

        return spawnedObjects;
    }
    */

    // --- NEW METHOD ---
    /// <summary>
    /// Spawns the specific sequence of shapes provided in the list.
    /// Clears any previously spawned objects.
    /// Places blocks, cylinders, and cones in their designated starting areas.
    /// </summary>
    /// <param name="prefabsToSpawn">The exact list of prefabs (e.g., Cube, Cube, Cylinder, Cone) to instantiate.</param>
    /// <returns>A list containing all the instantiated GameObjects.</returns>
    public List<GameObject> SpawnSpecificShapes(List<GameObject> prefabsToSpawn)
    {
        ClearSpawnedObjects(); // Ensure clean slate

        // --- 1. Validate Inputs ---
        if (blockPrefab == null || cylinderPrefab == null || conePrefab == null)
        {
            Debug.LogError("Block, Cylinder, or Cone prefab is not assigned in BlockSpawner!", this);
            return spawnedObjects; // Return empty list
        }
        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0)
        {
            Debug.LogWarning("List of prefabs to spawn is null or empty. No shapes will be spawned.", this);
            return spawnedObjects; // Return empty list
        }

        // --- 2. Spawn the shapes from the provided list ---
        int blocksSpawnedCount = 0;
        int cylindersSpawnedCount = 0;
        int conesSpawnedCount = 0; // Track cones for placement/debugging

        foreach (GameObject prefab in prefabsToSpawn)
        {
            if (prefab == null)
            {
                Debug.LogWarning("Encountered a null prefab in the target list. Skipping.", this);
                continue;
            }

            GameObject newShape = null;
            Vector3 position = Vector3.zero;

            // Determine spawn location based on prefab type
            if (prefab == blockPrefab)
            {
                position = blockSpawnOrigin + blocksSpawnedCount * blockSpawnOffset;
                newShape = Instantiate(blockPrefab, position, Quaternion.identity);
                blocksSpawnedCount++;
            }
            else if (prefab == cylinderPrefab)
            {
                position = cylinderSpawnOrigin + cylindersSpawnedCount * cylinderSpawnOffset;
                newShape = Instantiate(cylinderPrefab, position, Quaternion.identity);
                cylindersSpawnedCount++;
            }
            else if (prefab == conePrefab)
            {
                // Cones usually spawn at a single origin, possibly offset per cone if multiple were allowed
                position = coneSpawnOrigin + conesSpawnedCount * coneSpawnOffset;
                newShape = Instantiate(conePrefab, position, Quaternion.identity);
                conesSpawnedCount++;
            }
            else
            {
                Debug.LogError($"Prefab '{prefab.name}' in the list does not match any known shape prefab (Block, Cylinder, Cone). Cannot spawn.", this);
                continue; // Skip this unknown prefab
            }

            if (newShape != null)
            {
                spawnedObjects.Add(newShape);
                // Ensure physics are enabled for player objects (GameManager also does this, but good redundancy)
                 Rigidbody rb = newShape.GetComponent<Rigidbody>();
                 if (rb != null)
                 {
                     rb.isKinematic = false;
                     rb.useGravity = true;
                 }
            }
        }

        Debug.Log($"Spawned specific shapes: {blocksSpawnedCount} Blocks, {cylindersSpawnedCount} Cylinders, {conesSpawnedCount} Cones. Total: {spawnedObjects.Count}");

        return spawnedObjects;
    }
    // --- END NEW METHOD ---


    /// <summary>
    /// Destroys all currently tracked spawned objects and clears the list.
    /// </summary>
    public void ClearSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }

    // Optional: Fisher-Yates Shuffle if needed (Not needed for spawning specific order)
    /*
    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    */
}
