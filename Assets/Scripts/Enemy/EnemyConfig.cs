using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Enemy/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement Settings")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 8f;

    [Header("Patrol Settings")]
    public float groundCheckDistance = 0.7f;
    public LayerMask groundLayer;

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;

    [Header("Chase Settings")]
    [Tooltip("Radius for spotting the target (overlap + gizmo).")]
    public float chaseRange = 5f;
    public LayerMask targetLayer;
    [Tooltip("Must be closer than this before the enemy stops approaching further (a 'dead zone' band between chase and attack range).")]
    public float turnThreshold = 0.2f;

    [Header("Attack Settings")]
    public float attackRange = 1.2f;
    public int attackDamage = 2;
    public float attackCooldown = 1f;
    [Tooltip("No Animator/attack clip exists yet - this is how long the attack state holds before returning to Idle on its own.")]
    public float attackAnimationDuration = 0.5f;
    [Tooltip("Seconds into the attack before the hit is applied, when no animation event does it first.")]
    public float attackHitDelay = 0.2f;

    [Header("Damaged Settings")]
    [Tooltip("Horizontal knockback strength when entering DamagedState from melee (scaled in DamagedState).")]
    public float knockbackforce = 12f;
}
