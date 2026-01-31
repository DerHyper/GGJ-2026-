using Examples.Enemies;
using UnityEngine;

public class ApproachState : EnemyState
{

    EnemyBase enemyBase;
    public void OnStart(GameObject gameObject)
    {
        enemyBase = gameObject.GetComponent<EnemyBase>();
    }
    
    public void OnUpdate(GameObject gameObject)
    {
        
        if (enemyBase.TargetIsInRange())
        {
            enemyBase.ChangeState(new AttackState());
            return;
        }
        

        AStarPathfinding pathfinding = enemyBase.pathfinding;

        Vector2 nextWalkPoint = pathfinding.GetNextPointWorld(
            gameObject.transform.position, 
            Finder.FindPlayer().position);

        MoveTowards(gameObject, nextWalkPoint);
    }

    private void MoveTowards(GameObject gameObject, Vector2 targetPosition)
    {
        float step = enemyBase.movementSpeed * Time.deltaTime;
        gameObject.transform.position = Vector2.MoveTowards(
            gameObject.transform.position, 
            targetPosition, 
            step);
    }
}