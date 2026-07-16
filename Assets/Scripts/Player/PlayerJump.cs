using UnityEngine;

/// <summary>Handles jump physics (ground jump and air/double jump) and owns the "still ascending" logic.</summary>
public class PlayerJump : PlayerState
{
    public PlayerJump(Player player) : base(player) { }

    private float jumpStartTime;
    private const float MIN_JUMP_DURATION = 0.1f;

    private void ReplayJumpAnimation()
    {
        if (animator == null) return;
        animator.Play("Jumping", 0, 0f);
    }

    public override void Enter()
    {
        jumpStartTime = Time.time;

        player._isIdle = false;
        player._isWalking = false;
        // Note: _isSprinting is NOT reset here so sprint momentum carries through the jump
        player._isJumping = true;
        player._isAttacking = false;
        player._isCasting = false;

        ReplayJumpAnimation();

        bool isGrounded = player._isGrounded;

        // Check for wall jump first (higher priority)
        if (player.IsTouchingWall())
        {
            player.ChangeState(player.wallJumpState);
            return;
        }

        if (isGrounded)
        {
            player._jumpCount = 1;
            float force = player.jumpForce;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
        else if (player._jumpCount == 0 || player._jumpCount < player.maxJumps)
        {
            if (player._jumpCount == 0) player._jumpCount = 1;
            else player._jumpCount++;

            float force = (player._jumpCount == 1) ? player.jumpForce : player.doubleJumpForce;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            ReplayJumpAnimation();
            jumpStartTime = Time.time;
        }
    }

    public override void Update()
    {
        base.Update();

        Vector2 direction = player._direction;
        bool isGrounded = player._isGrounded;

        HandleMovement(direction);
        HandleDoubleJumpInput(isGrounded);
        HandleTransitions(direction, isGrounded);
    }

    private void HandleMovement(Vector2 direction)
    {
        player.UpdateSprintStateFromDirection(direction);
        float speed = player._isSprinting ? player.RunSpeed : player.WalkSpeed;
        player.ApplyHorizontalMovement(direction.x, speed);
    }

    private void HandleDoubleJumpInput(bool isGrounded)
    {
        // Allow wall jump even if "grounded" when touching wall (fixes same-layer issue)
        if (JumpPressed && player.IsTouchingWall())
        {
            player.ChangeState(player.wallJumpState);
        }
        else if (!isGrounded && JumpPressed)
        {
            if (player._jumpCount < player.maxJumps)
            {
                player.ResetJumpStates();
                player._jumpCount++;

                float force = (player._jumpCount == 1) ? player.jumpForce : player.doubleJumpForce;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
                ReplayJumpAnimation();
                jumpStartTime = Time.time;
            }
        }
    }

    private void HandleTransitions(Vector2 direction, bool isGrounded)
    {
        if (player.IsTouchingCeiling() && rb != null && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        bool isMovingUpwards = player._velocity.y > 0.1f;
        bool isFalling = player._velocity.y < -0.1f;

        if (isFalling)
        {
            player.ChangeState(player.fallState);
            return;
        }

        if (isGrounded && !isMovingUpwards && (Time.time - jumpStartTime) > MIN_JUMP_DURATION)
        {
            if (Mathf.Abs(player._velocity.y) < 0.5f)
            {
                player.ChangeState(player.idleState);
                return;
            }
        }

        if (isGrounded && Mathf.Abs(direction.x) > 0.1f)
        {
            player.ChangeState(player._isSprinting ? player.sprintState : player.walkState);
        }
    }

    public override void Exit()
    {
        player._isJumping = false;
    }
}
