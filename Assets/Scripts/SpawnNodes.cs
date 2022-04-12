using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNodes : MonoBehaviour
{
    int numtoSpawn = 35;
    public float spawnOffset = 0.32f;        
    public float currentSpawnOffset;

    void Start()
    {
        if (gameObject.name == "Node")
        {
            currentSpawnOffset = spawnOffset;
            for (int i = 0; i < numtoSpawn; i++)
            {
                GameObject clone = Instantiate(gameObject, new Vector3(transform.position.x , transform.position.y + currentSpawnOffset, transform.position.z), Quaternion.identity);
                currentSpawnOffset += spawnOffset;
            }
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
