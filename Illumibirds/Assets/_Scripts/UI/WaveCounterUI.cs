using System;
using TMPro;
using UnityEngine;

public class WaveCounterUI : MonoBehaviour
{
    public static event Action<int> OnMilestoneReached;

    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] int milestoneWave = 10;

    private int currentWave = 0;
    private RandomizedEnemySpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<RandomizedEnemySpawner>();
        if (spawner != null)
        {
            spawner.OnWaveDefeated += IncrementWave;
        }
        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnWaveDefeated -= IncrementWave;
        }
    }

    void IncrementWave()
    {
        currentWave++;
        UpdateDisplay();

        if (currentWave == milestoneWave)
        {
            OnMilestoneReached?.Invoke(currentWave);
        }
    }

    void UpdateDisplay()
    {
        if (waveText != null)
        {
            waveText.text = $"Room {currentWave}";
        }
    }
}
