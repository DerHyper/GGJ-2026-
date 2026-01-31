using System.Collections.Generic;
using UnityEngine;

namespace Rooms
{
    /// <summary>
    /// Runtime data class tracking a procedurally generated room.
    /// </summary>
    public class GeneratedRoom
    {
        /// <summary>
        /// World positions of spawn points in this room.
        /// </summary>
        public List<Vector3> SpawnPositions { get; set; } = new List<Vector3>();

        /// <summary>
        /// Floor number (0 = starting room, 1 = first room above, etc.)
        /// </summary>
        public int FloorNumber { get; set; }

        /// <summary>
        /// Tile bounds in the master tilemap.
        /// </summary>
        public BoundsInt TileBounds { get; set; }

        /// <summary>
        /// Reference to the detected Room instance from RoomDetector.
        /// </summary>
        public Room DetectedRoom { get; set; }

        /// <summary>
        /// The template used to generate this room.
        /// </summary>
        public RoomTemplate Template { get; set; }

        /// <summary>
        /// World offset where this room was stamped.
        /// </summary>
        public Vector3Int WorldOffset { get; set; }

        /// <summary>
        /// Actual size of this room in tiles (may differ from template.size if auto-detected).
        /// </summary>
        public Vector2Int ActualSize { get; set; }

        /// <summary>
        /// Whether this room has been loaded into the tilemap.
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// The instantiated prefab GameObject containing visual sprites.
        /// </summary>
        public GameObject RoomInstance { get; set; }

        public GeneratedRoom(int floorNumber, RoomTemplate template, Vector3Int worldOffset, Vector2Int actualSize, BoundsInt tileBounds)
        {
            FloorNumber = floorNumber;
            Template = template;
            WorldOffset = worldOffset;
            ActualSize = actualSize;
            TileBounds = tileBounds;
            IsLoaded = false;
        }

        /// <summary>
        /// Gets the Y position of the bottom of this room in world units.
        /// </summary>
        public float GetBottomY(float cellHeight)
        {
            return WorldOffset.y * cellHeight;
        }

        /// <summary>
        /// Gets the Y position of the top of this room in world units.
        /// </summary>
        public float GetTopY(float cellHeight)
        {
            return (WorldOffset.y + ActualSize.y) * cellHeight;
        }
    }
}
