using UnityEngine;

namespace Rooms
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DoorTrigger : MonoBehaviour
    {
        private RoomManager roomManager;
        private BoxCollider2D triggerCollider;
        private BoxCollider2D blockingCollider;
        private bool isDoorOpen;
        private Vector3Int cellPosition;
        private Room targetRoom;

        private void Start()
        {
            Debug.Log($"DoorTrigger.Start: Initializing at {transform.position}");
            roomManager = RoomManager.Instance;
            triggerCollider = GetComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;

            // Create a blocking collider for when door is closed
            blockingCollider = gameObject.AddComponent<BoxCollider2D>();
            blockingCollider.size = triggerCollider.size;
            blockingCollider.offset = triggerCollider.offset;
            blockingCollider.isTrigger = false;
            blockingCollider.enabled = true; // Doors start closed

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
            // Enable blocking collider when door is closed
            if (blockingCollider != null)
            {
                blockingCollider.enabled = !open;
            }
        }

        public void SetTargetRoom(Room room)
        {
            targetRoom = room;
            Debug.Log($"DoorTrigger: Door at {cellPosition} now leads to room {room?.Id}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"DoorTrigger.OnTriggerEnter2D: {other.name} entered, isDoorOpen={isDoorOpen}, isPlayer={other.CompareTag("Player")}");
            if (!isDoorOpen) return;
            if (!other.CompareTag("Player")) return;
            if (roomManager == null) return;
            if (targetRoom == null) return;

            var currentRoom = roomManager.CurrentRoom;
            if (currentRoom == null || targetRoom == currentRoom) return;

            Debug.Log($"DoorTrigger: Player entering room {targetRoom.Id}");
            roomManager.EnterRoom(targetRoom);
        }

        public Vector3Int CellPosition => cellPosition;
        public bool IsDoorOpen => isDoorOpen;
    }
}
