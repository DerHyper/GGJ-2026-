using System.Collections.Generic;
using Examples.Enemies;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Rooms
{
    public class Room
    {
        public int Id { get; set; }
        public List<Vector3Int> TilePositions { get; set; } = new List<Vector3Int>();
        public List<Vector3Int> DoorPositions { get; set; } = new List<Vector3Int>();
        public List<Vector3> SpawnPositions { get; set; } = new List<Vector3>();
        public Bounds WorldBounds { get; set; }
        public bool IsRevealed { get; set; }
        public List<GameObject> BlackOverlayTiles { get; set; } = new List<GameObject>();

        // Camera bounds (for Cinemachine confiner)
        public Collider2D CameraBounds { get; set; }

        // Combat tracking
        public List<EnemyBase> Enemies { get; set; } = new List<EnemyBase>();
        public bool IsCleared => Enemies.Count == 0;
        public bool DoorsOpen { get; set; }

        public Room(int id)
        {
            Id = id;
        }

        public void CalculateWorldBounds(Tilemap tilemap)
        {
            if (TilePositions.Count == 0)
            {
                WorldBounds = new Bounds(Vector3.zero, Vector3.zero);
                return;
            }

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);
            Vector3 cellSize = tilemap.cellSize;

            foreach (var pos in TilePositions)
            {
                Vector3 worldPos = tilemap.CellToWorld(pos);
                min = Vector3.Min(min, worldPos);
                max = Vector3.Max(max, worldPos + cellSize);
            }

            Vector3 center = (min + max) / 2f;
            Vector3 size = max - min;
            WorldBounds = new Bounds(center, size);
        }

        public bool ContainsWorldPosition(Vector3 worldPos)
        {
            // Ignore Z for 2D check
            Vector3 pos2D = new Vector3(worldPos.x, worldPos.y, WorldBounds.center.z);
            return WorldBounds.Contains(pos2D);
        }

        public void RegisterEnemy(EnemyBase enemy)
        {
            if (!Enemies.Contains(enemy))
            {
                Enemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(EnemyBase enemy)
        {
            Enemies.Remove(enemy);
        }
    }
}
