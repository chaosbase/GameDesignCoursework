using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CircleSpawner : NetworkBehaviour
{
    public GameObject safeInPrefab;
    public GameObject safeOutPrefab;
    
    void generateCircle(GameObject prefab) {
        GameObject circle = (GameObject)Instantiate(
            prefab, 
            //transform.position,
            new Vector3(0, 0, 0),
            Quaternion.identity
            );
        
        NetworkServer.Spawn(circle);
    }

    public override void OnStartServer() {
        generateCircle(safeInPrefab);
        generateCircle(safeOutPrefab);
    }
}
