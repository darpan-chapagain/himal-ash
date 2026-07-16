using UnityEngine;

public class ChaseState : State
{
    private Transform target;

    protected override string AnimBoolName => "isWalking";

    public ChaseState(Enemy enemy) : base(enemy) { }

    public override void FixedUpdate()
    {
        if (enemySenses == null || stateMachine == null || enemy == null || rb == null || config == null)
            return;

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
            stateMachine.ChangeState(new IdleState(enemy));
            return;
        }

        if (enemySenses.IsAtCliff() || enemySenses.IsAtWall())
        {
            enemy.Flip();
            stateMachine.ChangeState(new IdleState(enemy));
            return;
        }

        rb.linearVelocity = new Vector2(config.chaseSpeed * enemy.FacingDirection, rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        rb.linearVelocity = Vector2.zero;
    }
}
