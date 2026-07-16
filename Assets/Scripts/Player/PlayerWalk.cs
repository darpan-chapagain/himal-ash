using UnityEngine;

/// <summary>Moving horizontally at walk speed. Transitions to Sprint (double-tap), Idle (stop), Jump, Attack, Block, or Fall.</summary>
public class PlayerWalk : PlayerState
{
    public PlayerWalk(Player player) : base(player) { }

    public override void Enter()
    {
        player._isIdle = false;
        player._isWalking = true;
        player._isSprinting = false;
        player._isJumping = false;
        player._isAttacking = false;
        player._isCasting = false;
    }

    public override void Update()
    {
        base.Update();

        Vector2 direction = player._direction;
        bool isGrounded = player._isGrounded;

        player.UpdateSprintStateFromDirection(direction);
        player.ApplyHorizontalMovement(direction.x, player.WalkSpeed);

        HandleTransitions(direction, isGrounded);
    }

    private void HandleTransitions(Vector2 direction, bool isGrounded)
    {
        bool isMovingHorizontally = Mathf.Abs(direction.x) > 0.1f;

        if (AttackPressed && isGrounded) { player.ChangeState(player.attackState); return; }
        if (player.blockPressed && isGrounded) { player.ChangeState(player.blockState); return; }
        if (isMovingHorizontally && isGrounded && player._isSprinting) { player.ChangeState(player.sprintState); return; }
        if (!isMovingHorizontally && isGrounded) { player.ChangeState(player.idleState); return; }

        if (JumpPressed && isGrounded && !(player.currentState is PlayerJump))
        {
            player.ResetJumpStates();
            player.ChangeState(player.jumpState);
            return;
        }

        if (!isGrounded && player._velocity.y > 0.1f && !(player.currentState is PlayerJump))
        {
            player.ChangeState(player.jumpState);
            return;
        }

        if (!isGrounded)
            player.ChangeState(player.fallState);
    }

    public override void Exit()
    {
        player._isWalking = false;
    }
}
