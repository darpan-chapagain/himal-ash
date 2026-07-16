using UnityEngine;

/// <summary>Holding the block button. Locks movement and plays the block animation. Exits when block is released.</summary>
public class PlayerBlockState : PlayerState
{
    public PlayerBlockState(Player player) : base(player) { }

    public override void Enter()
    {
        player.ApplyHorizontalMovement(0f, 0f);
        player._isSprinting = false;

        player._isIdle = false;
        player._isWalking = false;
        player._isSprinting = false;
        player._isJumping = false;
        player._isAttacking = false;
        player._isCasting = false;
        player._isBlocking = true;
    }

    public override void Update()
    {
        player.ApplyHorizontalMovement(0f, 0f);

        if (player.blockReleased)
        {
            TransitionToAppropriateState();
            return;
        }

        if (JumpPressed && player._isGrounded)
        {
            player.ResetJumpStates();
            player.ChangeState(player.jumpState);
        }
    }

    private void TransitionToAppropriateState()
    {
        Vector2 direction = player._direction;
        bool isGrounded = player._isGrounded;

        if (!isGrounded)
        {
            player.ChangeState(player._velocity.y > 0.1f ? player.jumpState : player.fallState);
            return;
        }

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            player.ChangeState(player._isSprinting ? player.sprintState : player.walkState);
            return;
        }

        player.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        player._isBlocking = false;
    }
}
