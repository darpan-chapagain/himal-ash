using UnityEngine;

public class Enemy_Senses : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform attackPoint;

    public bool IsAtCliff()
    {
        if (groundCheck == null || config == null) return false;
        return !Physics2D.Raycast(groundCheck.position, Vector2.down, config.groundCheckDistance, config.groundLayer);
    }

    public bool IsAtWall()
    {
        if (wallCheck == null || config == null) return false;
        int facing = enemy != null ? enemy.FacingDirection : 1;
        return Physics2D.Raycast(wallCheck.position, Vector2.right * facing, config.wallCheckDistance, config.wallLayer);
    }

    public bool IsInAttackRange(Transform target)
    {
        if (!target || attackPoint == null || config == null) return false;
        float distance = Vector2.Distance(target.position, attackPoint.position);
        return distance <= config.attackRange;
    }

    public Transform GetChaseTarget()
    {
        if (attackPoint == null || config == null) return null;
        float range = config.chaseRange;
        LayerMask layer = config.targetLayer;
        if (range <= 0f || layer.value == 0) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, range, layer);
        if (hits == null || hits.Length == 0) return null;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            if (enemy != null && (hits[i].transform == enemy.transform || hits[i].transform.IsChildOf(enemy.transform))) continue;
            return hits[i].transform;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null || wallCheck == null || config == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * config.groundCheckDistance);

        Gizmos.color = Color.blue;
        int facing = enemy != null ? enemy.FacingDirection : 1;
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * facing * config.wallCheckDistance);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, config.attackRange);
        }
    }
}
