using System;
using System.Collections.Generic;
using GAS.Pickups;
using UnityEngine;

namespace Rooms
{
    /// <summary>
    /// Manages powerup spawning when rooms are cleared.
    /// Spawns a random pickup from the configured list at the room center.
    /// </summary>
    public class PowerupManager : MonoBehaviour
    {
        public static PowerupManager Instance { get; private set; }

        [Header("Powerup Settings")]
        [Tooltip("List of possible powerup prefabs to spawn")]
        [SerializeField] private List<Pickup> possiblePowerups = new();

        [Tooltip("Vertical offset from room center when spawning")]
        [SerializeField] private float spawnYOffset = 0f;

        [Tooltip("Skip spawning powerup in the starting room")]
        [SerializeField] private bool skipStartingRoom = true;

        [Header("Debug")]
        [SerializeField] private bool debugLog = true;

        public event Action<Pickup, Room> OnPowerupSpawned;

        private bool hasSkippedFirstRoom;

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
            // Subscribe to room cleared event
            if (RoomCombatManager.Instance != null)
            {
                RoomCombatManager.Instance.OnRoomCleared += HandleRoomCleared;
            }
            else
            {
                // Try again in Start if RoomCombatManager isn't ready yet
                Invoke(nameof(SubscribeToRoomCleared), 0.1f);
            }
        }

        private void OnDisable()
        {
            if (RoomCombatManager.Instance != null)
            {
                RoomCombatManager.Instance.OnRoomCleared -= HandleRoomCleared;
            }
        }

        private void SubscribeToRoomCleared()
        {
            if (RoomCombatManager.Instance != null)
            {
                RoomCombatManager.Instance.OnRoomCleared += HandleRoomCleared;
            }
        }

        private void HandleRoomCleared(Room room)
        {
            if (room == null) return;

            // Optionally skip the starting room
            if (skipStartingRoom && !hasSkippedFirstRoom)
            {
                hasSkippedFirstRoom = true;
                if (debugLog)
                    Debug.Log("PowerupManager: Skipping powerup spawn for starting room");
                return;
            }

            SpawnRandomPowerup(room);
        }

        public Pickup SpawnRandomPowerup(Room room)
        {
            if (possiblePowerups == null || possiblePowerups.Count == 0)
            {
                Debug.LogWarning("PowerupManager: No powerups configured!");
                return null;
            }

            // Pick a random powerup
            int randomIndex = UnityEngine.Random.Range(0, possiblePowerups.Count);
            Pickup powerupPrefab = possiblePowerups[randomIndex];

            if (powerupPrefab == null)
            {
                Debug.LogWarning($"PowerupManager: Powerup at index {randomIndex} is null!");
                return null;
            }

            // Spawn at room center
            Vector3 spawnPosition = room.WorldBounds.center;
            spawnPosition.y += spawnYOffset;
            spawnPosition.z = 0f;

            Pickup spawnedPowerup = Instantiate(powerupPrefab, spawnPosition, Quaternion.identity);

            if (debugLog)
                Debug.Log($"PowerupManager: Spawned {powerupPrefab.name} at {spawnPosition} in room {room.Id}");

            OnPowerupSpawned?.Invoke(spawnedPowerup, room);

            return spawnedPowerup;
        }

        /// <summary>
        /// Manually spawn a specific powerup at a position.
        /// </summary>
        public Pickup SpawnPowerup(Pickup powerupPrefab, Vector3 position)
        {
            if (powerupPrefab == null) return null;

            Pickup spawnedPowerup = Instantiate(powerupPrefab, position, Quaternion.identity);

            if (debugLog)
                Debug.Log($"PowerupManager: Manually spawned {powerupPrefab.name} at {position}");

            return spawnedPowerup;
        }

        /// <summary>
        /// Get a random powerup from the configured list (without spawning).
        /// Useful for UI previews or custom spawn logic.
        /// </summary>
        public Pickup GetRandomPowerupPrefab()
        {
            if (possiblePowerups == null || possiblePowerups.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, possiblePowerups.Count);
            return possiblePowerups[randomIndex];
        }

        public IReadOnlyList<Pickup> PossiblePowerups => possiblePowerups;
    }
}
