using UnityEngine;

public interface EnemyState
{
    public abstract void OnStart(GameObject gameObject);
    public abstract void OnUpdate(GameObject gameObject);
}