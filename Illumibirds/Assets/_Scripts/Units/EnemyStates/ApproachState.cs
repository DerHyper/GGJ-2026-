using Examples.Enemies;
using UnityEngine;

public class ApproachState : EnemyState
{
    private const float SeparationRadius = 1.5f;   // How close before pushing
    private const float SeparationWeight = 0.5f;   // Strength of push vs pathfinding

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
        Vector2 currentPos = gameObject.transform.position;
        Vector2 toTarget = (targetPosition - currentPos).normalized;
        Vector2 separation = CalculateSeparationForce(currentPos);

        Vector2 finalDirection = (toTarget + separation * SeparationWeight).normalized;

        float step = enemyBase.movementSpeed * Time.deltaTime;
        gameObject.transform.position = currentPos + finalDirection * step;
    }

    private Vector2 CalculateSeparationForce(Vector2 myPosition)
    {
        Vector2 separationForce = Vector2.zero;

        var room = Rooms.RoomCombatManager.Instance?.CurrentCombatRoom;
        if (room == null)
        {
            Debug.Log($"[Separation] No combat room for {instanceId}");
            return separationForce;
        }

        Debug.Log($"[Separation] Room has {room.Enemies.Count} enemies");

        foreach (var enemy in room.Enemies)
        {
            if (enemy == null || enemy.gameObject.GetInstanceID() == instanceId)
                continue;

            Vector2 toMe = myPosition - (Vector2)enemy.transform.position;
            float distance = toMe.magnitude;

            if (distance < SeparationRadius && distance > 0.01f)
            {
                float strength = 1f - (distance / SeparationRadius);
                separationForce += toMe.normalized * strength;
                // Debug.Log($"[Separation] Force from {enemy.name}: {toMe.normalized * strength}, dist={distance}");
            }
        }

        return separationForce;
    }
}