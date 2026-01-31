using UnityEngine;

namespace Rooms
{
    /// <summary>
    /// ScriptableObject defining room template metadata for procedural map generation.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomTemplate", menuName = "Rooms/Room Template")]
    public class RoomTemplate : ScriptableObject
    {
        [Header("Prefab")]
        [Tooltip("Room prefab containing a Tilemap component")]
        public GameObject prefab;

        [Header("Stacking")]
        [Tooltip("Height in tiles used for vertical stacking. 0 = auto-detect from prefab bounds (may include empty space).")]
        public int stackingHeight;

        [Header("Room Properties")]
        [Tooltip("Minimum floor number (inclusive). 0 = starting room.")]
        public int minFloor;

        [Tooltip("Maximum floor number (inclusive). -1 = no limit. Set to 0 for starting-room-only.")]
        public int maxFloor = -1;

        [Tooltip("Weight for random selection (higher = more likely)")]
        [Range(1, 10)]
        public int selectionWeight = 1;
    }
}
