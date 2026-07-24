using UnityEngine;

/// <summary>Locks movement and plays a cast animation; the actual effect applies at a hit frame (animation event or timeout fallback).</summary>
public class PlayerSpellCastState : PlayerState
{
    private readonly float castDuration = 0.5f;
    private readonly float hitDelay = 0.18f; // seconds into the cast to apply the hit if no animation event fires first

    private float castStartTime;
    private bool hitApplied;
    private bool castFinished;

    public PlayerSpellCastState(Player player) : base(player) { }

    public override void Enter()
    {
        castStartTime = Time.time;
        hitApplied = false;
        castFinished = false;

        player._isSprinting = false;
        LockHorizontalMovement();

        if (player.combat != null)
            player.combat.ResetAttackHitGuard();

        if (player.magic != null)
            player.magic.ResetCast();

        player._isIdle = false;
        player._isWalking = false;
        player._isSprinting = false;
        player._isJumping = false;
        player._isAttacking = false;
        player._isCasting = true;

        if (HasAnimatorParameter("isCasting", AnimatorControllerParameterType.Bool))
            animator.SetBool("isCasting", true);

        if (player.EnableStateLogs)
            Debug.Log("SpellCast State: Entered");
    }

    public override void Update()
    {
        LockHorizontalMovement();

        // Fallback if no animation event calls SpellCastHitFrame() at the clip's hit frame.
        if (!hitApplied && (Time.time - castStartTime) >= hitDelay)
            SpellCastHitFrame();

        // Timeout fallback if no finish animation event is sent.
        if (castFinished || (Time.time - castStartTime) >= castDuration)
            TransitionToNextState();
    }

    // Animation Event: call this at the hit frame in the SpellCast clip.
    public void SpellCastHitFrame()
    {
        if (hitApplied) return;
        hitApplied = true;
        player.SpellAttack();
    }

    // Animation Event: call this at the end of the SpellCast clip.
    public void SpellCastAnimationFinished()
    {
        castFinished = true;
    }

    public override void Exit()
    {
        hitApplied = false;
        castFinished = false;

        player._isCasting = false;

        if (player.EnableStateLogs)
            Debug.Log("SpellCast State: Exited");
    }

    private void TransitionToNextState()
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

    private void LockHorizontalMovement()
    {
        player.ApplyHorizontalMovement(0f, 0f);
    }

    private bool HasAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == name && parameter.type == type)
                return true;
        }
        return false;
    }
}
