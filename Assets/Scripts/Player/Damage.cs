using UnityEngine;

/// <summary>
/// Minimal single-player stub: exposes the entry points a future Health/Enemy system
/// will call to hurt or kill the player. Not wired to anything yet on its own.
/// </summary>
public class Damage : MonoBehaviour
{
    [SerializeField] private Player player;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.28f;
    [SerializeField] private float knockbackForce = 12f;

    private void Awake()
    {
        player = player != null ? player : GetComponent<Player>() ?? GetComponentInParent<Player>();
    }

    public void ApplyDamage(Vector2 sourcePosition)
    {
        if (player == null || player._isDead) return;

        int knockbackDir = sourcePosition.x <= transform.position.x ? 1 : -1;
        player.ChangeState(new PlayerDamagedState(player, knockbackDir, knockbackForce, knockbackDuration));
    }

    public void ApplyFatalDamage(Vector2 sourcePosition)
    {
        if (player == null) return;
        player.HandleFatalDamage(sourcePosition);
    }
}
