 using System;
using Examples.Enemies;
using UnityEngine;

namespace Rooms
{
    // Run early to ensure singleton is ready before other scripts subscribe
    [DefaultExecutionOrder(-100)]
    public class RoomCombatManager : MonoBehaviour
    {
        public static RoomCombatManager Instance { get; private set; }
        public static RoomCombatManager Required => Instance
            ? Instance
            : throw new InvalidOperationException($"{nameof(RoomCombatManager)} instance not found. Ensure it exists in the scene.");

        public event Action<Room> OnRoomCleared;
        public event Action<Room> OnCombatStarted;

        private Room combatRoom;
        private bool inCombat;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EnemyBase.OnDie += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            EnemyBase.OnDie -= HandleEnemyDeath;
        }

        public void RegisterEnemiesInRooms()
        {
            // Find all enemies in the scene and register them to their rooms
            var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            var roomManager = RoomManager.Instance;

            foreach (var enemy in enemies)
            {
                var room = roomManager.GetRoomAtPosition(enemy.transform.position);
                if (room != null)
                {
                    room.RegisterEnemy(enemy);
                    Debug.Log($"RoomCombatManager: Registered {enemy.name} to room {room.Id}");
                }
            }
        }

        public void StartCombat(Room room)
        {
            if (room == null || inCombat) return;

            combatRoom = room;
            inCombat = true;

            // Close all doors for this room
            DoorController.Required.CloseDoorsForRoom(room);

            OnCombatStarted?.Invoke(room);
            Debug.Log($"RoomCombatManager: Combat started in room {room.Id} with {room.Enemies.Count} enemies");

            // If room has no enemies, immediately clear it
            if (room.IsCleared)
            {
                EndCombat();
            } else
            {
                AudioManager.Instance.FadeInLayer(2); // Fade in combat music layer
            }
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            if (combatRoom == null) return;

            // Remove enemy from room
            combatRoom.UnregisterEnemy(enemy);
            Debug.Log($"RoomCombatManager: Enemy died. {combatRoom.Enemies.Count} remaining in room {combatRoom.Id}");

            // Check if room is cleared
            if (combatRoom.IsCleared)
            {
                EndCombat();
            }
        }

        private void EndCombat()
        {
            if (!inCombat || combatRoom == null) return;

            Debug.Log($"RoomCombatManager: Room {combatRoom.Id} cleared!");

            // Open doors to adjacent rooms
            var roomManager = RoomManager.Instance;
            var adjacentRooms = roomManager.GetAdjacentRooms(combatRoom);

            foreach (var adjRoom in adjacentRooms)
            {
                // Open doors leading to unrevealed rooms
                if (!adjRoom.IsRevealed)
                {
                    DoorController.Required.OpenDoorsForRoom(adjRoom);
                }
            }

            // Also open doors of the current room (so player can leave)
            DoorController.Required.OpenDoorsForRoom(combatRoom);

            OnRoomCleared?.Invoke(combatRoom);
            AudioManager.Instance.FadeOutLayer(2); // Fade out combat music layer

            inCombat = false;
            combatRoom = null;
        }

        public bool IsInCombat => inCombat;
        public Room CurrentCombatRoom => combatRoom;
    }
}
