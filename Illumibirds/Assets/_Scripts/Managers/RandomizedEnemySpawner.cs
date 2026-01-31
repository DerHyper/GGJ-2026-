using System;
using System.Collections.Generic;
using Examples.Enemies;
using UnityEngine;

public class RandomizedEnemySpawner : MonoBehaviour
{
    public List<EnemyWave> possibleEnemyWaves;
    List<Transform> possiblePositions;

    List<EnemyBase> spawnedEnemies;

    public Action OnWaveDefeated;
    public void SpawnEnemies()
    {
        spawnedEnemies = new();
        
        possiblePositions = RoomManager.Instance.GetCurrentRoom().possibleEnemySpawns;
        int rnd = UnityEngine.Random.Range(0, possibleEnemyWaves.Count);
        EnemyWave wave = possibleEnemyWaves[rnd];

        for (int i = 0; i < wave.enemies.Count; i++)
        {
            EnemyBase newEnemy = Instantiate(wave.enemies[i], possiblePositions[i].position, Quaternion.identity);
            SubscribeToEnemyDeath(newEnemy);
        }
    }

    // void SpawnRandomEnemyWave(Vector2 pos)
    // {
    //     int rnd = UnityEngine.Random.Range(0, possibleEnemyWaves.Count);
    //     EnemyWave wave = possibleEnemyWaves[rnd];

    //     for (int i = 0; i < wave.enemies.Count; i++)
    //     {
    //         Instantiate(wave.enemies[i], pos, Quaternion.identity);

    //     }
    //     // Destroy(this.gameObject, 0.1f);
    // }



    public void SubscribeToEnemyDeath(EnemyBase enemy)
    {
        if (!spawnedEnemies.Contains(enemy)) spawnedEnemies.Add(enemy);
        enemy.OnDie += OnEnemyDied;
    }


    void OnEnemyDied(EnemyBase diedEnemy)
    {
        diedEnemy.OnDie -= OnEnemyDied;

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (!spawnedEnemies[i]._isDead)
                break;

            OnWaveDefeated?.Invoke();
        }
    }

    void OnDisable()
    {
        if (spawnedEnemies == null || spawnedEnemies.Count == 0) return;

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            spawnedEnemies[i].OnDie -= OnEnemyDied;
        }
    }
}