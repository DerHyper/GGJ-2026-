using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] public EnemySO enemyData;
    [SerializeField] public EnemyState CurrentState;

    private void Start()
    {
        CurrentState = new ApproachState();
        // movementSpeed = enemyData.movementSpeed;
    }

    private void Update()
    {
        CurrentState.OnUpdate(gameObject);
    }

    private void OnDestroy()
    {
        AStarPathfinding.Instance.RemoveRequester(GetInstanceID());
    }
}
