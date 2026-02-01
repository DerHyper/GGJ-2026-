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
            Debug.Log($"DoorTrigger.Start: Initializing at {transform.position}");
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
            Debug.Log($"DoorTrigger.SetDoorOpen: {cellPosition} -> {open}");
            isDoorOpen = open;
            // When door is closed, it should block movement
            // The tilemap collider handles blocking, this is for trigger detection
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"DoorTrigger.OnTriggerEnter2D: {other.name} entered, isDoorOpen={isDoorOpen}, isPlayer={other.CompareTag("Player")}");
            if (!isDoorOpen) return;
            if (!other.CompareTag("Player")) return;
            if (roomManager == null) return;

            var currentRoom = roomManager.CurrentRoom;
            if (currentRoom == null) return;

            // Find the target room by checking cells adjacent to the door
            // One of them will belong to a different room
            Room targetRoom = null;
            Vector3Int[] adjacentOffsets = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

            foreach (var offset in adjacentOffsets)
            {
                var adjacentCell = cellPosition + offset;
                var room = roomManager.GetRoomContainingCell(adjacentCell);
                if (room != null && room != currentRoom)
                {
                    targetRoom = room;
                    break;
                }
            }

            if (targetRoom != null)
            {
                Debug.Log($"DoorTrigger: Door at {cellPosition} leads to room {targetRoom.Id}");
                roomManager.EnterRoom(targetRoom);
            }
        }

        public Vector3Int CellPosition => cellPosition;
        public bool IsDoorOpen => isDoorOpen;
    }
}
