using UnityEngine;

public class ApproachState : EnemyState
{
    public void OnStart(GameObject gameObject)
    {
        
    }
    
    public void OnUpdate(GameObject gameObject)
    {
        EnemyBehavior enemyBehavior = gameObject.GetComponent<EnemyBehavior>();
        AStarPathfinding pathfinding = enemyBehavior.pathfinding;

        Vector2 nextWalkPoint = pathfinding.GetNextPointWorld(
            gameObject.transform.position, 
            Finder.FindPlayer().position);

        MoveTowards(gameObject, nextWalkPoint);
    }

    private void MoveTowards(GameObject gameObject, Vector2 targetPosition)
    {
        float step = gameObject.GetComponent<EnemyBehavior>().movementSpeed * Time.deltaTime;
        gameObject.transform.position = Vector2.MoveTowards(
            gameObject.transform.position, 
            targetPosition, 
            step);
    }
}