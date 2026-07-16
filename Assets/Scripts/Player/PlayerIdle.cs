using UnityEngine;

/// <summary>Default state when the player is standing still on the ground. Transitions to Walk, Sprint, Jump, Attack, Block, or Fall.</summary>
public class PlayerIdle : PlayerState
{
    public PlayerIdle(Player player) : base(player) { }

    public override void Enter()
    {
        player._isIdle = true;
        player._isWalking = false;
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
        if (isGrounded)
            player.ApplyHorizontalMovement(0f, 0f);

        HandleTransitions(direction, isGrounded);
    }

    private void HandleTransitions(Vector2 direction, bool isGrounded)
    {
        bool isMovingHorizontally = Mathf.Abs(direction.x) > 0.1f;

        if (AttackPressed && isGrounded) { player.ChangeState(player.attackState); return; }
        if (player.blockPressed && isGrounded) { player.ChangeState(player.blockState); return; }

        if (isMovingHorizontally && isGrounded)
        {
            player.ChangeState(player._isSprinting ? player.sprintState : player.walkState);
            return;
        }

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

        if (!isGrounded && player._velocity.y < -0.1f)
            player.ChangeState(player.fallState);
    }

    public override void Exit()
    {
        player._isIdle = false;
    }
}
