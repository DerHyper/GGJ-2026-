using System.Collections.Generic;
using Examples.Enemies;
using UnityEngine;

public class RandomizedEnemySpawner : MonoBehaviour
{
    public List<EnemyBase> possibleEnemies;
    public List<Transform> possiblePositions;

    public int maxAmount;
    int amount;
    // List<Transform> usedPositions;

    void Start()
    {
        if(maxAmount > possiblePositions.Count) Debug.LogWarning("Max enemiAmount is higher than Possible Spawn Count");
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        amount = UnityEngine.Random.Range(1, maxAmount + 1);

        for (int i = 0; i < amount; i++)
        {
            SpawnRandomEnemy(possiblePositions[i].position);
        }
    }

    void SpawnRandomEnemy(Vector2 pos)
    {
        int rnd = UnityEngine.Random.Range(0, possibleEnemies.Count);
        EnemyBase enemy = possibleEnemies[rnd];

        Instantiate(enemy, pos, Quaternion.identity);
        Destroy(this.gameObject, 0.1f);
    }
}