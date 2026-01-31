using UnityEngine;

namespace Rooms
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DoorTrigger : MonoBehaviour
    {
        private RoomManager roomManager;
        private BoxCollider2D boxCollider;
        private bool isDoorOpen;
        private Vector3Int cellPosition;

        private void Start()
        {
            roomManager = RoomManager.Instance;
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;

            // Calculate cell position from world position
            if (roomManager != null && roomManager.Scanner != null)
            {
                var scanner = roomManager.Scanner;
                cellPosition = roomManager.Tilemap.WorldToCell(transform.position);
            }

            if (roomManager == null)
            {
                Debug.LogWarning("DoorTrigger: No RoomManager found in scene");
            }
        }

        public void SetDoorOpen(bool open)
        {
            isDoorOpen = open;
            // When door is closed, it should block movement
            // The tilemap collider handles blocking, this is for trigger detection
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isDoorOpen) return;
            if (!other.CompareTag("Player")) return;
            if (roomManager == null) return;

            // Determine which room the player is entering
            // The player is coming from their current room and entering an adjacent one
            var currentRoom = roomManager.CurrentRoom;
            if (currentRoom == null) return;

            // Find the room on the other side of this door
            Room targetRoom = null;
            var adjacentRooms = roomManager.GetAdjacentRooms(currentRoom);

            foreach (var adjRoom in adjacentRooms)
            {
                if (adjRoom.DoorPositions.Contains(cellPosition) && !adjRoom.IsRevealed)
                {
                    targetRoom = adjRoom;
                    break;
                }
            }

            if (targetRoom != null)
            {
                // Player is entering a new room
                roomManager.EnterRoom(targetRoom);
            }
        }

        public Vector3Int CellPosition => cellPosition;
        public bool IsDoorOpen => isDoorOpen;
    }
}
