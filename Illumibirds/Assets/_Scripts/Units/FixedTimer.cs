using UnityEngine;

public class FixedTimer
{
    private float _currentTime = 0;
    private float _timerStart = 0;
    private bool _isTimerRunning = false;

    public void Start()
    {
        _timerStart = Time.time;
        _isTimerRunning = true;
    }

    private void Update()
    {
        _currentTime = Time.time - _timerStart;
    }

    public bool IsRunning()
    {
        return _isTimerRunning;
    }

    public float GetTime()
    {
        Update();
        return _currentTime;
    }
}