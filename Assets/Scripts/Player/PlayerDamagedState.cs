using UnityEngine;

/// <summary>Brief knockback state when the player takes a hit. Slides horizontally for a set duration, then returns to Idle.</summary>
public class PlayerDamagedState : PlayerState
{
    private readonly float knockbackVelocity;
    private readonly float knockbackDuration;
    private float timer;

    public PlayerDamagedState(Player player, int knockbackDirection, float knockbackForce, float knockbackDuration) : base(player)
    {
        knockbackVelocity = knockbackDirection * knockbackForce;
        this.knockbackDuration = knockbackDuration;
    }

    public override void Enter()
    {
        base.Enter();
        timer = knockbackDuration;

        player._isDamaged = true;
        const float knockbackLift = 0.85f;
        rb.linearVelocity = new Vector2(knockbackVelocity, rb.linearVelocity.y + knockbackLift);
    }

    public override void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(knockbackVelocity, rb.linearVelocity.y);
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            player.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        player._isDamaged = false;
    }
}
