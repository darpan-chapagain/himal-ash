using UnityEngine;

public class IdleState : State
{
    private Transform target;

    protected override string AnimBoolName => "isIdle";

    public IdleState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        base.Enter();
        rb.linearVelocity = Vector2.zero;
    }

    public override void FixedUpdate()
    {
        target = enemySenses.GetChaseTarget();

        if (!target)
        {
            stateMachine.ChangeState(new PatrolState(enemy));
            return;
        }

        enemy.FaceTarget(target);

        if (enemySenses.IsInAttackRange(target) && combat != null && combat.CanMeleeAttack())
        {
            stateMachine.ChangeState(new MeleeAttackState(enemy));
            return;
        }

        float distance = Mathf.Abs(target.position.x - enemy.transform.position.x);

        if (distance <= config.turnThreshold)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (enemySenses.IsAtCliff() || enemySenses.IsAtWall())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        stateMachine.ChangeState(new ChaseState(enemy));
    }
}
