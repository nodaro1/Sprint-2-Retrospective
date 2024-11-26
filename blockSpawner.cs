using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlockSpawner : MonoBehaviour
{
    [Header("Block Settings")]
    public GameObject blockPrefab;        // create setting which allows you to assign the block prefab to this variable in the script (done in unity GUI later)
    public int numberOfBlocks = 3;        // num blocks to spawn
    public Vector3 spawnPosition = new Vector3(0, 1, 0); // block starting position
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);  // set offset between blocks

    // optional setting: can spawn blocks at runtime (button press)
    void Update()
    {
        // ex: press S key to spawn blocks
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnBlocks();
        }
    }

    // spawn blocks in a stack
    public void SpawnBlocks()
    {
        for (int i = 0; i < numberOfBlocks; i++)
        {
            Vector3 position = spawnPosition + i * spawnOffset;
            Instantiate(blockPrefab, position, Quaternion.identity);
        }
    }

    //  spawn blocks at the start automatically
    void Start()
    {
        SpawnBlocks();
    }
}

