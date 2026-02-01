using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapScanner : MonoBehaviour
    {
        private Tilemap tilemap;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
        }

        public Tilemap Tilemap => tilemap;

        public List<Vector3Int> GetTilePositions(GameTile.TileType type)
        {
            var positions = new List<Vector3Int>();

            if (tilemap == null)
            {
                Debug.LogWarning("TilemapScanner: No tilemap assigned.");
                return positions;
            }

            var bounds = tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile<GameTile>(pos);
                if (tile != null && tile.tileType == type)
                {
                    positions.Add(pos);
                }
            }

            return positions;
        }

        public List<Vector3Int> GetDoorPositions()
        {
            var positions = new List<Vector3Int>();
            positions.AddRange(GetTilePositions(GameTile.TileType.Door));
            positions.AddRange(GetTilePositions(GameTile.TileType.DoorClosed));
            return positions;
        }

        public List<Vector3Int> GetSpawnPositions()
        {
            return GetTilePositions(GameTile.TileType.Spawn);
        }

        public List<Vector3Int> GetWallPositions()
        {
            return GetTilePositions(GameTile.TileType.Wall);
        }

        public List<Vector3Int> GetFloorPositions()
        {
            return GetTilePositions(GameTile.TileType.Floor);
        }

        public GameTile GetGameTileAt(Vector3Int position)
        {
            if (tilemap == null)
            {
                Debug.LogWarning("TilemapScanner: No tilemap assigned.");
                return null;
            }

            return tilemap.GetTile<GameTile>(position);
        }

        public bool IsWalkable(Vector3Int position)
        {
            var tile = GetGameTileAt(position);
            return tile != null && tile.isWalkable;
        }

        public Vector3 CellToWorld(Vector3Int cellPosition)
        {
            return tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2f;
        }

        public List<Vector3Int> GetNeighbors(Vector3Int pos)
        {
            return new List<Vector3Int>
            {
                pos + Vector3Int.up,
                pos + Vector3Int.down,
                pos + Vector3Int.left,
                pos + Vector3Int.right
            };
        }

        public bool IsTileAt(Vector3Int pos)
        {
            return GetGameTileAt(pos) != null;
        }

        public Vector3 CellSize => tilemap != null ? tilemap.cellSize : Vector3.one;
    }
}
