using UnityEngine;

/// <summary>Melee/spell hit detection - finds a target's Health component and damages it.</summary>
public class Combat : MonoBehaviour
{
    private const int AttackDamage = 3;

    [Header("References")]
    public Player player;

    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    private bool hitAppliedThisAttack;

    private void Awake()
    {
        if (player == null)
            player = GetComponent<Player>() ?? GetComponentInParent<Player>();

        if (attackPoint == null)
            Debug.LogWarning("Combat: attackPoint is not assigned. Using object transform as fallback; check Inspector placement.");

        if (enemyLayer.value == 0)
            Debug.LogWarning("Combat: enemyLayer is empty. No enemies will be detected until a layer is assigned.");
    }

    private void OnValidate()
    {
        if (attackRadius < 0.01f)
            attackRadius = 0.01f;
    }

    public void TryAttack() => TryAttack(AttackDamage);

    public void TryAttack(int damageAmount)
    {
        Vector2 attackPosition = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] enemyHits = Physics2D.OverlapCircleAll(attackPosition, attackRadius, enemyLayer);

        if (enemyHits.Length == 0)
        {
            LogDebug("attack pressed - no enemy in range");
            return;
        }

        foreach (Collider2D enemyHit in enemyHits)
        {
            Health health = enemyHit.GetComponent<Health>()
                ?? enemyHit.GetComponentInParent<Health>()
                ?? enemyHit.GetComponentInChildren<Health>();

            if (health != null)
            {
                health.ChangeHealth(-damageAmount, transform.position);
                LogDebug($"attack pressed - hit {enemyHit.gameObject.name} for {damageAmount} damage");
                return;
            }
        }

        LogDebug("attack pressed - enemy collider found but no Health component on collider/parent/child");
    }

    public void AttackAnimationFinished()
    {
        if (player == null) return;
        hitAppliedThisAttack = false;
        if (player.currentState is PlayerAttackState state)
            state.AttackAnimationFinished();
    }

    public void AttackHitFrame() => TryAttackOncePerAttack();
    public void ComboAttack1Hit() => TryAttackOncePerAttack();
    public void ComboAttack2Hit() => TryAttackOncePerAttack();
    public void ComboAttack3Hit() => TryAttackOncePerAttack();

    public void SpellCastHitFrame()
    {
        if (player != null && player.currentState is PlayerSpellCastState state)
            state.SpellCastHitFrame();
    }

    public void SpellCastAnimationFinished()
    {
        if (player != null && player.currentState is PlayerSpellCastState state)
            state.SpellCastAnimationFinished();
    }

    public void ResetAttackHitGuard() => hitAppliedThisAttack = false;

    private void TryAttackOncePerAttack()
    {
        if (hitAppliedThisAttack) return;
        hitAppliedThisAttack = true;
        TryAttack();
    }

    private void LogDebug(string message)
    {
        if (player != null && player.EnableCombatLogs)
            Debug.Log(message);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.color = attackPoint != null ? new Color(0.2f, 0.9f, 0.2f, 0.9f) : new Color(1f, 0.65f, 0f, 0.9f);
        Gizmos.DrawWireSphere(center, attackRadius);
        Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
        Gizmos.DrawLine(transform.position, center);
    }
}
