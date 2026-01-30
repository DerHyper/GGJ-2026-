using UnityEngine;

namespace Examples.Enemies
{
    /// <summary>
    /// Simple melee enemy that moves toward player and attacks when in range.
    /// </summary>
    public class MeleeEnemy : EnemyBase
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _stopDistance = 1.5f;

        protected override void UpdateBehavior()
        {
            if (_target == null) return;

            float distance = Vector2.Distance(transform.position, _target.position);

            // Move toward player if not in attack range
            if (distance > _stopDistance)
            {
                MoveTowardTarget();
            }
            else
            {
                StopMoving();
                TryAttack();
            }
        }

        private void MoveTowardTarget()
        {
            if (_rb == null || _target == null) return;

            Vector2 direction = (_target.position - transform.position).normalized;
            _rb.linearVelocity = direction * _moveSpeed;

            // Face movement direction
            if (direction.x != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
            }
        }

        private void StopMoving()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
