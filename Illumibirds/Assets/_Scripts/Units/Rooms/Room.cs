using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public Tilemap WalkableTiles;
    public BoxCollider2D cameraBounds;

    void Start()
    {
        WalkableTiles = Finder.TryFindInChildren(gameObject.transform, "WalkableTiles", out Transform walkableTilesObj) 
            ? walkableTilesObj.GetComponent<Tilemap>() 
            : null;

        SetUpCameraBounds();
    }

    private void SetUpCameraBounds()
    {
        Tilemap walkableTiles = RoomManager.Instance.GetCurrentRoom().WalkableTiles;
        BoundsInt bounds = walkableTiles.cellBounds;
        Vector3 min = walkableTiles.CellToWorld(bounds.min);
        Vector3 max = walkableTiles.CellToWorld(bounds.max);

        cameraBounds = gameObject.AddComponent<BoxCollider2D>();
        cameraBounds.isTrigger = true;
        cameraBounds.offset = new Vector2(min.x+((max.x - min.x) / 2), min.y+((max.y - min.y) / 2 +.5f));
        cameraBounds.size = new Vector2(max.x - min.x, max.y - min.y + 1f);
    }
}
