using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public Tilemap WalkableTiles;

    void Start()
    {
        WalkableTiles = Finder.TryFindInChildren(gameObject.transform, "WalkableTiles", out Transform walkableTilesObj) 
            ? walkableTilesObj.GetComponent<Tilemap>() 
            : null;
    }
}
