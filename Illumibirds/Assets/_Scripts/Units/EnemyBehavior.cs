using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] public EnemySO enemyData;
    [SerializeField] public EnemyState CurrentState;
    public AStarPathfinding pathfinding;
    
    private void Start()
    {
        pathfinding = new AStarPathfinding();
        CurrentState = new ApproachState();
        // movementSpeed = enemyData.movementSpeed;
    }
    private void Update() 
    {
        CurrentState.OnUpdate(gameObject);
    }
}
