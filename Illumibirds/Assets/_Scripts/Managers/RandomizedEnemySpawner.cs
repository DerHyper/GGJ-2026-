using System;
using System.Collections.Generic;
using Examples.Enemies;
using Rooms;
using UnityEngine;

public class RandomizedEnemySpawner : MonoBehaviour
{
    public List<EnemyWave> possibleEnemyWaves;
    public Action OnWaveDefeated;

    [SerializeField] RandomAbilityPickup abilityPickupPrefab;

    private Room currentCombatRoom;
    private bool initialized;

    private void Start()
    {
        initialized = true;
        Subscribe();
    }

    private void OnEnable()
    {
        // Only subscribe on re-enable (after Start has run once)
        if (initialized)
            Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        RoomCombatManager.Required.OnRoomCleared += OnRoomCleared;
    }

    private void Unsubscribe()
    {
        // Use Instance (not Required) for cleanup - may be null during shutdown
        if (RoomCombatManager.Instance != null)
            RoomCombatManager.Instance.OnRoomCleared -= OnRoomCleared;
    }

    public void SpawnEnemies()
    {
        var roomManager = RoomManager.Required;

        var room = roomManager.GetCurrentRoom();
        if (room == null) return;

        currentCombatRoom = room;
        var spawnPositions = room.SpawnPositions;

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning($"No spawn positions in room {room.Id}");
            return;
        }

        if (possibleEnemyWaves == null || possibleEnemyWaves.Count == 0) return;

        int rnd = UnityEngine.Random.Range(0, possibleEnemyWaves.Count);
        EnemyWave wave = possibleEnemyWaves[rnd];

        int enemyCount = Mathf.Min(wave.enemies.Count, spawnPositions.Count);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyBase newEnemy = Instantiate(wave.enemies[i], spawnPositions[i], Quaternion.identity);
            room.RegisterEnemy(newEnemy);
        }
    }

    private void OnRoomCleared(Room room)
    {
        if (room == currentCombatRoom)
        {
            OnWaveDefeated?.Invoke();
            Debug.Log("CLEAR WAVE");
            if (abilityPickupPrefab != null)
            {
                Instantiate(abilityPickupPrefab, Vector2.zero, Quaternion.identity);
            }
            currentCombatRoom = null;
        }
    }
}
