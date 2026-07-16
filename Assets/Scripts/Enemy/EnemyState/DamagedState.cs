using UnityEngine;

public class DamagedState : State
{
    protected override string AnimBoolName => "isDamaged";
    private const float KnockbackScale = 0.45f;

    private readonly float knockbackVelocity;
    private float knockbackDuration;

    public DamagedState(Enemy enemy, int knockbackDir) : base(enemy)
    {
        float mag = config != null ? config.knockbackforce * KnockbackScale : 0f;
        knockbackVelocity = knockbackDir * mag;
    }

    public override void Enter()
    {
        base.Enter();
        knockbackDuration = 0.25f;

        if (rb == null) return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(new Vector2(knockbackVelocity, 0f), ForceMode2D.Impulse);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (rb == null || stateMachine == null) return;

        rb.linearVelocity = new Vector2(knockbackVelocity, rb.linearVelocity.y);
        knockbackDuration -= Time.fixedDeltaTime;

        if (knockbackDuration <= 0f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(new IdleState(enemy));
        }
    }
}
