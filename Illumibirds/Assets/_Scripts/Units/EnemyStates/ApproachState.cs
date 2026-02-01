using Examples.Enemies;
using UnityEngine;

public class ApproachState : EnemyState
{
    private EnemyBase enemyBase;
    private int instanceId;

    public void OnStart(GameObject gameObject)
    {
        enemyBase = gameObject.GetComponent<EnemyBase>();
        instanceId = gameObject.GetInstanceID();
    }

    public void OnUpdate(GameObject gameObject)
    {
        if (enemyBase.TargetIsInRange())
        {
            enemyBase.ChangeState(new AttackState());
            return;
        }

        Transform player = Finder.FindPlayer();
        if (player == null) return;

        Vector2 nextWalkPoint = AStarPathfinding.Instance.GetNextPointWorld(
            gameObject.transform.position,
            player.position,
            instanceId);

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