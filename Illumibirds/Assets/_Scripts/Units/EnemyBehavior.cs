using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] public EnemyState CurrentState;
    private void Update() 
    {
        CurrentState.OnUpdate(gameObject);
    }
}
