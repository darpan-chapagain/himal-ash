using UnityEngine;

/// <summary>Pushes the player off a wall it's touching, in the opposite direction.</summary>
public class PlayerWallJumpState : PlayerState
{
    private readonly float horizontalJumpPercentage = 0.7f;

    public PlayerWallJumpState(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        player._isIdle = false;
        player._isWalking = false;
        player._isSprinting = false;
        player._isJumping = false;
        player._isWallJumping = true;
        player._isAttacking = false;
        player._isCasting = false;

        if (animator != null)
            animator.SetBool("isWallJumping", true);

        Vector2 wallDirection = player.GetWallDirection();
        Vector2 wallJumpVelocity = new Vector2(
            wallDirection.x * player.wallJumpForce.x * horizontalJumpPercentage,
            player.wallJumpForce.y
        );

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(wallJumpVelocity, ForceMode2D.Impulse);

        // Reset jump count to allow air control after wall jump
        player._jumpCount = 1;
        player.ResetJumpStates();
    }

    public override void Update()
    {
        base.Update();

        // Allow chaining wall jumps - if touching wall and press jump, reset wall jump
        if (JumpPressed && player.IsTouchingWall())
        {
            player.ChangeState(player.wallJumpState);
            return;
        }

        if (player._isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            if (Mathf.Abs(player._direction.x) < 0.1f)
                player.ChangeState(player.idleState);
            else
                player.ChangeState(player._isSprinting ? player.sprintState : player.walkState);
            return;
        }

        if (rb.linearVelocity.y < -0.1f)
        {
            player.ChangeState(player.fallState);
            return;
        }

        // Allow double jump during wall jump
        if (JumpPressed && player._jumpCount < player.maxJumps)
        {
            player.ChangeState(player.jumpState);
        }
    }

    public override void FixedUpdate()
    {
        if (!player._isGrounded)
        {
            if (rb.linearVelocity.y < -0.01f)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (player.FallMultiplier - 1) * Time.fixedDeltaTime;
            else if (rb.linearVelocity.y > 0.01f && !player.jumpPressed)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (player.LowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    public override void Exit()
    {
        base.Exit();
        player._isWallJumping = false;
        if (animator != null)
            animator.SetBool("isWallJumping", false);
    }
}
