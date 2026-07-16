using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    private EnemyConfig config;
    private Enemy enemy;

    private float lastAttackTime;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        config = enemy.config;
    }

    public bool CanMeleeAttack()
    {
        if (config == null) return false;
        return Time.time >= lastAttackTime + config.attackCooldown;
    }

    public void PerformMeleeAttack()
    {
        if (attackPoint == null || config == null) return;

        lastAttackTime = Time.time;
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, config.attackRange, config.targetLayer);

        if (hit == null) return;

        // Attack can hit target child colliders, so resolve Health from self/parent/children.
        Health health = hit.GetComponent<Health>();
        health ??= hit.GetComponentInParent<Health>();
        health ??= hit.GetComponentInChildren<Health>();

        if (health != null)
            health.ChangeHealth(-config.attackDamage, transform.position);
    }
}
