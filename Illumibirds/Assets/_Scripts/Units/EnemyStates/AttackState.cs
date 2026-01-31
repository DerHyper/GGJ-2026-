using Examples.Enemies;
using GAS.Core;
using UnityEngine;

public class AttackState : EnemyState
{
    EnemyBase enemyBase;

    public void OnStart(GameObject gameObject)
    {
        enemyBase = gameObject.GetComponent<EnemyBase>();

    }

    public void OnUpdate(GameObject gameObject)
    {
        if (enemyBase == null) return;
        if (enemyBase._isDead) return;

        TurnToPlayer();

        if (enemyBase.IsTooCloseToTarget())
        {
            Debug.Log("TOO CLOSE");
        }
        else if (enemyBase.TargetIsInRange())
        {
            
            TryAttack();
        }
        else
        {
            enemyBase.ChangeState(new ApproachState());
        }
    }

    protected virtual void PerformAttack()
    {
        if (enemyBase._target == null) return;

        enemyBase._asc.TryActivateAbility(enemyBase.abilityToUse);

        // Debug.Log($"{enemyBase.name} attacked {enemyBase._target.name}");
    }


    protected virtual void TryAttack()
    {
        if (!enemyBase.CanAttack()) return;

        PerformAttack();
        enemyBase.ResetTimer();
    }

    void TurnToPlayer()
    {
        if (enemyBase._target != null && enemyBase.aimTransform != null)
        {
            Vector3 direction = enemyBase._target.position - enemyBase.aimTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            enemyBase.aimTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}