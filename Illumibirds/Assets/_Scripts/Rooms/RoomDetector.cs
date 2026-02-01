using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Rooms
{
    [RequireComponent(typeof(TilemapScanner))]
    [RequireComponent(typeof(Tilemap))]
    public class RoomDetector : MonoBehaviour
    {
        private TilemapScanner scanner;
        private Tilemap tilemap;

        private HashSet<Vector3Int> globalVisited = new HashSet<Vector3Int>();
        private int nextRoomId = 0;

        private void Awake()
        {
            scanner = GetComponent<TilemapScanner>();
            tilemap = GetComponent<Tilemap>();
        }

        public List<Room> DetectRooms()
        {
            var rooms = new List<Room>();
            globalVisited.Clear();
            nextRoomId = 0;

            var doorPositions = scanner.GetDoorPositions();

            foreach (var doorPos in doorPositions)
            {
                var neighbors = GetNeighbors(doorPos);
                foreach (var neighbor in neighbors)
                {
                    if (!globalVisited.Contains(neighbor) && scanner.IsWalkable(neighbor))
                    {
                        var room = FloodFillFromPosition(neighbor, doorPos);
                        if (room.TilePositions.Count > 0)
                        {
                            rooms.Add(room);
                        }
                    }
                }
            }

            // Also detect any rooms that might not have doors (like starting room)
            var floorPositions = scanner.GetFloorPositions();
            foreach (var floorPos in floorPositions)
            {
                if (!globalVisited.Contains(floorPos))
                {
                    var room = FloodFillFromPosition(floorPos, null);
                    if (room.TilePositions.Count > 0)
                    {
                        rooms.Add(room);
                    }
                }
            }

            return rooms;
        }

        private Room FloodFillFromPosition(Vector3Int startPos, Vector3Int? associatedDoor)
        {
            var room = new Room(nextRoomId++);
            var queue = new Queue<Vector3Int>();
            var localVisited = new HashSet<Vector3Int>();

            queue.Enqueue(startPos);
            localVisited.Add(startPos);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var tile = scanner.GetGameTileAt(current);

                if (tile == null) continue;

                // Add doors to the room's door list but don't flood through them
                if (tile.tileType == GameTile.TileType.Door || tile.tileType == GameTile.TileType.DoorClosed)
                {
                    if (!room.DoorPositions.Contains(current))
                    {
                        room.DoorPositions.Add(current);
                    }
                    continue;
                }

                // Only add walkable non-door tiles to room
                if (!tile.isWalkable) continue;

                room.TilePositions.Add(current);
                globalVisited.Add(current);

                var neighbors = GetNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (!localVisited.Contains(neighbor) && !globalVisited.Contains(neighbor))
                    {
                        var neighborTile = scanner.GetGameTileAt(neighbor);
                        if (neighborTile != null && (neighborTile.isWalkable || neighborTile.tileType == GameTile.TileType.Door || neighborTile.tileType == GameTile.TileType.DoorClosed))
                        {
                            queue.Enqueue(neighbor);
                            localVisited.Add(neighbor);
                        }
                    }
                }
            }

            if (associatedDoor.HasValue && !room.DoorPositions.Contains(associatedDoor.Value))
            {
                room.DoorPositions.Add(associatedDoor.Value);
            }

            room.CalculateWorldBounds(tilemap);
            return room;
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
            return scanner.GetGameTileAt(pos) != null;
        }

        /// <summary>
        /// Detects a room within the specified bounds. Used for incremental detection
        /// when new rooms are procedurally generated.
        /// </summary>
        /// <param name="bounds">The tile bounds to search within</param>
        /// <returns>The detected room, or null if no room found</returns>
        public Room DetectRoomInBounds(BoundsInt bounds)
        {
            // Find floor tiles within bounds to start flood fill
            Vector3Int? startPos = null;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (globalVisited.Contains(pos)) continue;

                var tile = scanner.GetGameTileAt(pos);
                if (tile != null && tile.isWalkable && tile.tileType != GameTile.TileType.Door && tile.tileType != GameTile.TileType.DoorClosed)
                {
                    startPos = pos;
                    break;
                }
            }

            if (!startPos.HasValue)
            {
                return null;
            }

            return FloodFillInBounds(startPos.Value, bounds);
        }

        /// <summary>
        /// Flood fill constrained to the specified bounds.
        /// </summary>
        private Room FloodFillInBounds(Vector3Int startPos, BoundsInt bounds)
        {
            var room = new Room(nextRoomId++);
            var queue = new Queue<Vector3Int>();
            var localVisited = new HashSet<Vector3Int>();

            queue.Enqueue(startPos);
            localVisited.Add(startPos);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Skip if outside bounds
                if (!bounds.Contains(current)) continue;

                var tile = scanner.GetGameTileAt(current);
                if (tile == null) continue;

                // Add doors to the room's door list but don't flood through them
                if (tile.tileType == GameTile.TileType.Door || tile.tileType == GameTile.TileType.DoorClosed)
                {
                    if (!room.DoorPositions.Contains(current))
                    {
                        room.DoorPositions.Add(current);
                    }
                    continue;
                }

                // Only add walkable non-door tiles to room
                if (!tile.isWalkable) continue;

                room.TilePositions.Add(current);
                globalVisited.Add(current);

                var neighbors = GetNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (!localVisited.Contains(neighbor) && !globalVisited.Contains(neighbor))
                    {
                        // Only enqueue if within bounds
                        if (bounds.Contains(neighbor))
                        {
                            var neighborTile = scanner.GetGameTileAt(neighbor);
                            if (neighborTile != null && (neighborTile.isWalkable || neighborTile.tileType == GameTile.TileType.Door || neighborTile.tileType == GameTile.TileType.DoorClosed))
                            {
                                queue.Enqueue(neighbor);
                                localVisited.Add(neighbor);
                            }
                        }
                    }
                }
            }

            if (room.TilePositions.Count > 0)
            {
                room.CalculateWorldBounds(tilemap);
                return room;
            }

            return null;
        }

        /// <summary>
        /// Clears visited state for tiles within the specified bounds.
        /// Used when unloading rooms.
        /// </summary>
        public void ClearVisitedInBounds(BoundsInt bounds)
        {
            var toRemove = new List<Vector3Int>();
            foreach (var pos in globalVisited)
            {
                if (bounds.Contains(pos))
                {
                    toRemove.Add(pos);
                }
            }

            foreach (var pos in toRemove)
            {
                globalVisited.Remove(pos);
            }
        }

        /// <summary>
        /// Resets all detection state. Use before regenerating the entire map.
        /// </summary>
        public void ResetDetectionState()
        {
            globalVisited.Clear();
            nextRoomId = 0;
        }
    }
}
