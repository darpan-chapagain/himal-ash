using UnityEngine;

/// <summary>Bridges the player's Health events into FSM transitions (Damaged/Death).</summary>
public class Damage : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Health health;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.28f;
    [SerializeField] private float knockbackForce = 12f;

    private void Awake()
    {
        player = player != null ? player : GetComponent<Player>() ?? GetComponentInParent<Player>();
        health = health != null ? health : GetComponent<Health>() ?? GetComponentInParent<Health>();
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.OnDamaged += HandleDamage;
        health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.OnDamaged -= HandleDamage;
        health.OnDeath -= HandleDeath;
    }

    private void HandleDamage(Vector2 sourcePosition)
    {
        if (player == null || player._isDead) return;

        int knockbackDir = sourcePosition.x <= transform.position.x ? 1 : -1;
        player.ChangeState(new PlayerDamagedState(player, knockbackDir, knockbackForce, knockbackDuration));
    }

    private void HandleDeath(Vector2 sourcePosition)
    {
        if (player == null) return;
        player.HandleFatalDamage(sourcePosition);
    }
}
