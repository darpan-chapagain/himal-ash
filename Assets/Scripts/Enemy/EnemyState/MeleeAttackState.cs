using UnityEngine;

public class MeleeAttackState : State
{
    private bool hasAppliedDamage;
    private float enterTime;

    protected override string AnimBoolName => "isAttacking";

    public MeleeAttackState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        base.Enter();
        hasAppliedDamage = false;
        enterTime = Time.time;
        rb.linearVelocity = Vector2.zero;
    }

    // No Animator/attack clip exists yet, so this state can't rely purely on
    // OnAnimationFinished()/animation events to know when the hit lands or when
    // the attack ends - config.attackHitDelay/attackAnimationDuration cover that
    // until real animations exist. A real animation event would still finish
    // this early via OnAnimationFinished()/ApplyAttackHit().
    public override void FixedUpdate()
    {
        if (config == null) return;

        float elapsed = Time.time - enterTime;

        if (!hasAppliedDamage && elapsed >= config.attackHitDelay)
            ApplyAttackHit();

        if (elapsed >= config.attackAnimationDuration)
            stateMachine.ChangeState(new IdleState(enemy));
    }

    public void ApplyAttackHit()
    {
        if (hasAppliedDamage) return;
        hasAppliedDamage = true;
        combat?.PerformMeleeAttack();
    }

    public override void OnAnimationFinished()
    {
        stateMachine.ChangeState(new IdleState(enemy));
    }
}
