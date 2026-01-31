using UnityEngine;

namespace Rooms
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = Color.red;
        [SerializeField] private float gizmoRadius = 0.3f;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
        }
    }
}
