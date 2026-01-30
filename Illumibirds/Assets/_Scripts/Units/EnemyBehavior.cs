using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{

    public EnemyState CurrentState;
    private void Update() 
    {
        CurrentState.OnUpdate();
    }
}
