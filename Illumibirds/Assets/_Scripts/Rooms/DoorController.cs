using System;
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
        public static DoorController Required => Instance
            ? Instance
            : throw new InvalidOperationException($"{nameof(DoorController)} instance not found. Ensure it exists in the scene.");

        private Tilemap tilemap;
        private TilemapScanner scanner;

        private Dictionary<Vector3Int, bool> doorStates = new Dictionary<Vector3Int, bool>();

        // Stores wall tiles that were cleared for doorways (position -> original tile)
        private Dictionary<Vector3Int, GameTile> clearedWallTiles = new Dictionary<Vector3Int, GameTile>();

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
            if (RoomManager.Required.UseProceduralGeneration)
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

            Debug.Log($"DoorController.OpenDoorsForRoom: Room {room.Id} has {room.DoorPositions.Count} door positions");

            foreach (var doorPos in room.DoorPositions)
            {
                Debug.Log($"DoorController.OpenDoorsForRoom: Processing door at {doorPos}");
                SetDoorOpen(doorPos);
                doorStates[doorPos] = true;

                // Clear wall tiles adjacent to door (above and below)
                ClearAdjacentWalls(doorPos);
            }

            // Update door sprite visuals
            if (room.DoorVisuals != null)
            {
                room.DoorVisuals.OpenDoors();
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

                // Restore wall tiles adjacent to door
                RestoreAdjacentWalls(doorPos);
            }

            // Update door sprite visuals
            if (room.DoorVisuals != null)
            {
                room.DoorVisuals.CloseDoors();
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
            Debug.Log($"DoorController.SetDoorOpen: pos={pos}, tile={currentTile?.name ?? "NULL"}, type={currentTile?.tileType}");

            if (currentTile != null && currentTile.tileType == GameTile.TileType.DoorClosed)
            {
                Debug.Log($"DoorController.SetDoorOpen: openDoorTile={currentTile.openDoorTile?.name ?? "NULL"}");
                if (currentTile.openDoorTile != null)
                {
                    tilemap.SetTile(pos, currentTile.openDoorTile);
                    tilemap.RefreshTile(pos);
                    Debug.Log($"DoorController.SetDoorOpen: Swapped to open tile at {pos}");

                    // Verify the swap
                    var newTile = tilemap.GetTile<GameTile>(pos);
                    Debug.Log($"DoorController.SetDoorOpen: After swap - tile={newTile?.name ?? "NULL"}, colliderType={newTile?.colliderType}");
                }
                else
                {
                    Debug.LogWarning($"DoorController.SetDoorOpen: openDoorTile is NULL for {currentTile.name}!");
                }
            }
            else if (currentTile == null)
            {
                Debug.LogWarning($"DoorController.SetDoorOpen: No tile found at {pos}");
            }
            else
            {
                Debug.Log($"DoorController.SetDoorOpen: Tile at {pos} is not DoorClosed (is {currentTile.tileType})");
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
                    tilemap.RefreshTile(pos);
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

        /// <summary>
        /// Clears wall tiles adjacent to a door position to allow passage.
        /// Checks tiles above and below the door (for vertical room stacking).
        /// </summary>
        private void ClearAdjacentWalls(Vector3Int doorPos)
        {
            // Check positions above and below the door
            Vector3Int[] adjacentPositions = new Vector3Int[]
            {
                doorPos + Vector3Int.up,
                doorPos + Vector3Int.down,
                doorPos + Vector3Int.up * 2,
                doorPos + Vector3Int.down * 2
            };

            foreach (var pos in adjacentPositions)
            {
                if (clearedWallTiles.ContainsKey(pos)) continue;

                var tile = tilemap.GetTile<GameTile>(pos);
                if (tile != null && (tile.tileType == GameTile.TileType.Wall || tile.tileType == GameTile.TileType.HalfWall))
                {
                    // Store original tile and clear it
                    clearedWallTiles[pos] = tile;
                    tilemap.SetTile(pos, null);
                    tilemap.RefreshTile(pos);
                    Debug.Log($"DoorController: Cleared wall at {pos} for doorway at {doorPos}");
                }
            }
        }

        /// <summary>
        /// Restores wall tiles that were cleared for a doorway.
        /// </summary>
        private void RestoreAdjacentWalls(Vector3Int doorPos)
        {
            Vector3Int[] adjacentPositions = new Vector3Int[]
            {
                doorPos + Vector3Int.up,
                doorPos + Vector3Int.down,
                doorPos + Vector3Int.up * 2,
                doorPos + Vector3Int.down * 2
            };

            foreach (var pos in adjacentPositions)
            {
                if (clearedWallTiles.TryGetValue(pos, out var originalTile))
                {
                    tilemap.SetTile(pos, originalTile);
                    tilemap.RefreshTile(pos);
                    clearedWallTiles.Remove(pos);
                    Debug.Log($"DoorController: Restored wall at {pos}");
                }
            }
        }
    }
}
