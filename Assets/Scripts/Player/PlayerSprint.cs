using UnityEngine;

/// <summary>Moving horizontally at run speed (activated by double-tapping a direction). Sprint momentum carries into jumps and falls.</summary>
public class PlayerSprint : PlayerState
{
    public PlayerSprint(Player player) : base(player) { }

    public override void Enter()
    {
        player._isIdle = false;
        player._isWalking = false;
        player._isSprinting = true;
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
        float speed = player._isSprinting ? player.RunSpeed : player.WalkSpeed;
        player.ApplyHorizontalMovement(direction.x, speed);

        HandleTransitions(direction, isGrounded);
    }

    private void HandleTransitions(Vector2 direction, bool isGrounded)
    {
        bool isMovingHorizontally = Mathf.Abs(direction.x) > 0.1f;

        if (AttackPressed && isGrounded) { player.ChangeState(player.attackState); return; }
        if (player.blockPressed && isGrounded) { player.ChangeState(player.blockState); return; }
        if (!isMovingHorizontally && isGrounded) { player.ChangeState(player.idleState); return; }
        if (isMovingHorizontally && isGrounded && !player._isSprinting) { player.ChangeState(player.walkState); return; }

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
        // Don't reset _isSprinting here - the destination state's Enter() handles it.
        // Jump/Fall do NOT reset it, so sprint momentum carries into the air.
    }
}
