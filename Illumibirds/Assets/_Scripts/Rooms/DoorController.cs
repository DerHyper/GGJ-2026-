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

            foreach (var doorPos in room.DoorPositions)
            {
                SetDoorOpen(doorPos, room);
                doorStates[doorPos] = true;

                // Clear wall tiles adjacent to door (above and below)
                ClearAdjacentWalls(doorPos);

                // Also clear walls at the target room's entrance
                var worldPos = scanner.CellToWorld(doorPos);
                var targetRoom = RoomManager.Required.FindAdjacentRoom(room, worldPos);
                if (targetRoom != null)
                {
                    ClearTargetRoomEntrance(room, doorPos, targetRoom);
                }
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

        private void SetDoorOpen(Vector3Int pos, Room sourceRoom = null)
        {
            var currentTile = tilemap.GetTile<GameTile>(pos);

            if (currentTile != null && currentTile.tileType == GameTile.TileType.DoorClosed)
            {
                if (currentTile.openDoorTile != null)
                {
                    tilemap.SetTile(pos, currentTile.openDoorTile);
                    tilemap.RefreshTile(pos);

                    // Verify the swap
                    var newTile = tilemap.GetTile<GameTile>(pos);
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

            // Update collider state - open doors are passable
            UpdateDoorCollider(pos, true, sourceRoom);
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

        private void UpdateDoorCollider(Vector3Int pos, bool isOpen, Room sourceRoom = null)
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

                    // Set target room when opening
                    if (isOpen && sourceRoom != null)
                    {
                        var targetRoom = RoomManager.Required.FindAdjacentRoom(sourceRoom, worldPos);
                        doorTrigger.SetTargetRoom(targetRoom);
                    }
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
                }
            }
        }

        /// <summary>
        /// Clears wall tiles at the target room's entrance to allow passage.
        /// Scans from the source door all the way to the target room, clearing any blocking walls.
        /// </summary>
        private void ClearTargetRoomEntrance(Room sourceRoom, Vector3Int doorPos, Room targetRoom)
        {
            var doorWorldPos = scanner.CellToWorld(doorPos);
            bool goingUp = doorWorldPos.y > sourceRoom.WorldBounds.center.y;

            // Calculate the Y range to scan - from source door to target room interior
            int startY = doorPos.y;
            int endY;
            int step;

            if (goingUp)
            {
                // Scan upward from door to target room's interior
                endY = Mathf.CeilToInt(targetRoom.WorldBounds.min.y) + 5;
                step = 1;
            }
            else
            {
                // Scan downward from door to target room's interior
                endY = Mathf.FloorToInt(targetRoom.WorldBounds.max.y) - 5;
                step = -1;
            }
            
            // Scan along the path and clear any walls
            for (int y = startY; goingUp ? y <= endY : y >= endY; y += step)
            {
                var pos = new Vector3Int(doorPos.x, y, doorPos.z);

                if (clearedWallTiles.ContainsKey(pos)) continue;

                var tile = tilemap.GetTile<GameTile>(pos);
                if (tile == null) continue;

                if (tile.tileType == GameTile.TileType.Wall || tile.tileType == GameTile.TileType.HalfWall)
                {
                    clearedWallTiles[pos] = tile;
                    tilemap.SetTile(pos, null);
                    tilemap.RefreshTile(pos);
                }
                else if (tile.tileType == GameTile.TileType.DoorClosed)
                {
                    // Open any closed doors along the path
                    if (tile.openDoorTile != null)
                    {
                        tilemap.SetTile(pos, tile.openDoorTile);
                        tilemap.RefreshTile(pos);
                        doorStates[pos] = true;
                    }
                }
            }
        }
    }
}
