using UnityEngine;

/// <summary>Falling downward (not from a jump). Allows air control, double-jump, and wall-jump while falling.</summary>
public class PlayerFall : PlayerState
{
    public PlayerFall(Player player) : base(player) { }

    public override void Enter()
    {
        player._isIdle = false;
        player._isWalking = false;
        player._isJumping = true;
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

        // NOTE (known quirk, replicated faithfully from the reference project):
        // this applies a jump impulse directly, THEN switches to PlayerJump, whose own
        // Enter() re-evaluates jump conditions and can apply a second impulse on the
        // same input. Flagged in the devlog to revisit after playtesting.
        if (JumpPressed && player.IsTouchingWall())
        {
            player.ChangeState(player.wallJumpState);
        }
        else if (!isGrounded && JumpPressed && player._jumpCount < player.maxJumps)
        {
            player.ResetJumpStates();
            player._jumpCount++;
            float force = (player._jumpCount == 1) ? player.jumpForce : player.doubleJumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            player.ChangeState(player.jumpState);
        }

        if (!isGrounded) return;

        bool isMovingHorizontally = Mathf.Abs(direction.x) > 0.1f;
        if (isMovingHorizontally)
            player.ChangeState(player._isSprinting ? player.sprintState : player.walkState);
        else
            player.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        player._isJumping = false;
    }
}
