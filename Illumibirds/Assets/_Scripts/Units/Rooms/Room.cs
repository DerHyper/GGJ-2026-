using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public Tilemap WalkableTiles;
    public List<Transform> possibleEnemySpawns;
    public Transform playerSpawn;

    void Start()
    {
        WalkableTiles = Finder.TryFindInChildren(gameObject.transform, "WalkableTiles", out Transform walkableTilesObj) 
            ? walkableTilesObj.GetComponent<Tilemap>() 
            : null;
    }
}
