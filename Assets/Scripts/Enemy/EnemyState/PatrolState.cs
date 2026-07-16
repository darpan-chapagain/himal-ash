using UnityEngine;

public class PatrolState : State
{
    protected override string AnimBoolName => "isWalking";

    public PatrolState(Enemy enemy) : base(enemy) { }

    public override void FixedUpdate()
    {
        if (enemySenses.GetChaseTarget() != null)
        {
            stateMachine.ChangeState(new ChaseState(enemy));
            return;
        }

        if (enemySenses.IsAtCliff() || enemySenses.IsAtWall())
        {
            enemy.Flip();
            return;
        }

        rb.linearVelocity = new Vector2(config.patrolSpeed * enemy.FacingDirection, rb.linearVelocity.y);
    }
}
