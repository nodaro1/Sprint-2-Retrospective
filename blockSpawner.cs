using UnityEngine;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour
{
    [Header("Block Settings")]
    public GameObject blockPrefab;
    public int numberOfBlocks = 3;
    public Vector3 spawnPosition = new Vector3(0, 1, 0);
    public Vector3 spawnOffset = new Vector3(3, 0, 0);

    private List<GameObject> spawnedBlocks = new List<GameObject>();

    
    void Start()
    {
        // SpawnBlocks();  // COMMENT THIS OUT
    }
    

    // Make this public so GameManager or UI can call it:
    public List<GameObject> SpawnBlocks()
    {
        // Clear any previous blocks if needed
        foreach (var block in spawnedBlocks)
        {
            Destroy(block);
        }
        spawnedBlocks.Clear();

        for (int i = 0; i < numberOfBlocks; i++)
        {
            Vector3 position = spawnPosition + i * spawnOffset;
            GameObject block = Instantiate(blockPrefab, position, Quaternion.identity);
            spawnedBlocks.Add(block);
        }
        return spawnedBlocks;
    }
}
