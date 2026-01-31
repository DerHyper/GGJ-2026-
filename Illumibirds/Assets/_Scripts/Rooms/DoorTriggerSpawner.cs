using System.Collections.Generic;
using Tiles;
using UnityEngine;

namespace Rooms
{
    /// <summary>
    /// Spawns DoorTrigger colliders at all door positions detected by TilemapScanner.
    /// Attach this to the same GameObject as TilemapScanner.
    /// </summary>
    [RequireComponent(typeof(TilemapScanner))]
    public class DoorTriggerSpawner : MonoBehaviour
    {
        [SerializeField] private Vector2 triggerSize = new Vector2(1f, 1f);

        private TilemapScanner scanner;

        private void Awake()
        {
            scanner = GetComponent<TilemapScanner>();
        }

        private void Start()
        {
            // Skip auto-spawn if RoomManager handles it via procedural generation
            if (RoomManager.Instance != null && RoomManager.Instance.UseProceduralGeneration)
            {
                return;
            }
            SpawnDoorTriggers();
        }

        public void SpawnDoorTriggers()
        {
            if (scanner == null)
            {
                Debug.LogWarning("DoorTriggerSpawner: No TilemapScanner assigned");
                return;
            }

            var doorPositions = scanner.GetDoorPositions();
            Debug.Log($"DoorTriggerSpawner: Spawning {doorPositions.Count} door triggers");

            foreach (var doorPos in doorPositions)
            {
                CreateDoorTrigger(doorPos);
            }
        }

        private void CreateDoorTrigger(Vector3Int cellPos)
        {
            var worldPos = scanner.CellToWorld(cellPos);

            var triggerObj = new GameObject($"DoorTrigger_{cellPos.x}_{cellPos.y}");
            triggerObj.transform.position = worldPos;
            triggerObj.transform.parent = transform;

            var collider = triggerObj.AddComponent<BoxCollider2D>();
            collider.size = triggerSize;
            collider.isTrigger = true;

            triggerObj.AddComponent<DoorTrigger>();
        }

        /// <summary>
        /// Spawns door triggers within the specified bounds.
        /// Used for incremental trigger creation when new rooms are generated.
        /// </summary>
        public void SpawnTriggersInBounds(BoundsInt bounds)
        {
            if (scanner == null)
            {
                Debug.LogWarning("DoorTriggerSpawner: No TilemapScanner assigned");
                return;
            }

            int count = 0;
            foreach (var pos in bounds.allPositionsWithin)
            {
                var tile = scanner.GetGameTileAt(pos);
                if (tile != null && (tile.tileType == GameTile.TileType.Door || tile.tileType == GameTile.TileType.DoorClosed))
                {
                    // Check if trigger already exists at this position
                    if (!DoorTriggerExistsAt(pos))
                    {
                        CreateDoorTrigger(pos);
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                Debug.Log($"DoorTriggerSpawner: Spawned {count} door triggers in bounds");
            }
        }

        /// <summary>
        /// Destroys door triggers within the specified bounds.
        /// Used when unloading rooms.
        /// </summary>
        public void DestroyTriggersInBounds(BoundsInt bounds)
        {
            var triggersToDestroy = new List<GameObject>();

            foreach (Transform child in transform)
            {
                var cellPos = scanner != null
                    ? GetCellPosFromTriggerName(child.name)
                    : Vector3Int.zero;

                if (bounds.Contains(cellPos))
                {
                    triggersToDestroy.Add(child.gameObject);
                }
            }

            foreach (var trigger in triggersToDestroy)
            {
                Destroy(trigger);
            }
        }

        private bool DoorTriggerExistsAt(Vector3Int cellPos)
        {
            string expectedName = $"DoorTrigger_{cellPos.x}_{cellPos.y}";
            foreach (Transform child in transform)
            {
                if (child.name == expectedName)
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3Int GetCellPosFromTriggerName(string name)
        {
            // Parse "DoorTrigger_X_Y" format
            if (!name.StartsWith("DoorTrigger_")) return Vector3Int.zero;

            var parts = name.Substring("DoorTrigger_".Length).Split('_');
            if (parts.Length >= 2 &&
                int.TryParse(parts[0], out int x) &&
                int.TryParse(parts[1], out int y))
            {
                return new Vector3Int(x, y, 0);
            }

            return Vector3Int.zero;
        }
    }
}
