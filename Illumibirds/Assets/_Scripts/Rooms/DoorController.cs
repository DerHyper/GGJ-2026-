using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Rooms
{
    [RequireComponent(typeof(TilemapScanner))]
    [RequireComponent(typeof(Tilemap))]
    public class DoorController : MonoBehaviour
    {
        public static DoorController Instance { get; private set; }

        private Tilemap tilemap;
        private TilemapScanner scanner;

        private Dictionary<Vector3Int, bool> doorStates = new Dictionary<Vector3Int, bool>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                scanner = GetComponent<TilemapScanner>();
                tilemap = GetComponent<Tilemap>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Skip auto-init if RoomManager handles it via procedural generation
            if (RoomManager.Instance != null && RoomManager.Instance.UseProceduralGeneration)
            {
                return;
            }
            InitializeDoors();
        }

        private void InitializeDoors()
        {
            var doorPositions = scanner.GetDoorPositions();
            foreach (var pos in doorPositions)
            {
                // All doors start closed
                doorStates[pos] = false;
                SetDoorClosed(pos);
            }
        }

        /// <summary>
        /// Registers doors within the specified bounds. Used for incremental door setup
        /// when new rooms are procedurally generated.
        /// </summary>
        public void RegisterDoorsInBounds(BoundsInt bounds)
        {
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (doorStates.ContainsKey(pos)) continue;

                var tile = tilemap.GetTile<GameTile>(pos);
                if (tile != null && (tile.tileType == GameTile.TileType.Door || tile.tileType == GameTile.TileType.DoorClosed))
                {
                    doorStates[pos] = false;
                    SetDoorClosed(pos);
                }
            }
        }

        /// <summary>
        /// Unregisters doors within the specified bounds. Used when unloading rooms.
        /// </summary>
        public void UnregisterDoorsInBounds(BoundsInt bounds)
        {
            var toRemove = new List<Vector3Int>();
            foreach (var kvp in doorStates)
            {
                if (bounds.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var pos in toRemove)
            {
                doorStates.Remove(pos);
            }
        }

        public void OpenDoorsForRoom(Room room)
        {
            if (room == null) return;

            foreach (var doorPos in room.DoorPositions)
            {
                SetDoorOpen(doorPos);
                doorStates[doorPos] = true;
            }

            room.DoorsOpen = true;
            Debug.Log($"DoorController: Opened doors for room {room.Id}");
        }

        public void CloseDoorsForRoom(Room room)
        {
            if (room == null) return;

            foreach (var doorPos in room.DoorPositions)
            {
                SetDoorClosed(doorPos);
                doorStates[doorPos] = false;
            }

            room.DoorsOpen = false;
            Debug.Log($"DoorController: Closed doors for room {room.Id}");
        }

        public bool IsDoorOpen(Vector3Int doorPos)
        {
            return doorStates.TryGetValue(doorPos, out bool isOpen) && isOpen;
        }

        private void SetDoorOpen(Vector3Int pos)
        {
            var currentTile = tilemap.GetTile<GameTile>(pos);
            if (currentTile != null && currentTile.tileType == GameTile.TileType.DoorClosed)
            {
                if (currentTile.openDoorTile != null)
                {
                    tilemap.SetTile(pos, currentTile.openDoorTile);
                }
            }

            // Update collider state - open doors are passable
            UpdateDoorCollider(pos, true);
        }

        private void SetDoorClosed(Vector3Int pos)
        {
            var currentTile = tilemap.GetTile<GameTile>(pos);
            if (currentTile != null && currentTile.tileType == GameTile.TileType.Door)
            {
                if (currentTile.closedDoorTile != null)
                {
                    tilemap.SetTile(pos, currentTile.closedDoorTile);
                }
            }

            // Update collider state - closed doors are impassable
            UpdateDoorCollider(pos, false);
        }

        private void UpdateDoorCollider(Vector3Int pos, bool isOpen)
        {
            // Find the door trigger at this position and update it
            var worldPos = scanner.CellToWorld(pos);
            var colliders = Physics2D.OverlapPointAll(worldPos);

            foreach (var col in colliders)
            {
                var doorTrigger = col.GetComponent<DoorTrigger>();
                if (doorTrigger != null)
                {
                    doorTrigger.SetDoorOpen(isOpen);
                }
            }
        }

        public Tilemap Tilemap => tilemap;
    }
}
