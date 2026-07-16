using UnityEngine;

/// <summary>A 3-step melee combo with buffered input between steps.</summary>
public class PlayerAttackState : PlayerState
{
    private readonly float attack1Duration = 0.45f;
    private readonly float attack2Duration = 0.42f;
    private readonly float attack3Duration = 0.42f;

    private float stepStartTime;
    private int currentStep;
    private bool queuedNextStep;
    private bool waitingForNextStepInput;
    private float waitForNextStepStartTime;

    public PlayerAttackState(Player player) : base(player) { }

    public override void Enter()
    {
        currentStep = 1;
        queuedNextStep = false;
        waitingForNextStepInput = false;
        waitForNextStepStartTime = 0f;

        // Stop run/walk momentum as soon as attack starts.
        LockHorizontalMovement();
        player._isSprinting = false;

        player._isIdle = false;
        player._isWalking = false;
        player._isSprinting = false;
        player._isJumping = false;
        player._isAttacking = true;
        player._isCasting = false;

        StartAttackStep(1);
        LogDebug("Attack State: Started Attack1");
    }

    public override void Update()
    {
        // Keep horizontal movement locked during the whole attack state.
        LockHorizontalMovement();

        if (TryHandleImmediateInputTransitions())
            return;

        // Allow buffering next combo step during the current step.
        if (currentStep < 3 && player.attackPressed)
            queuedNextStep = true;

        // After a step finishes, allow a short input window for next step.
        if (waitingForNextStepInput)
        {
            if (player.attackPressed)
            {
                StartAttackStep(currentStep + 1);
                waitingForNextStepInput = false;
                return;
            }

            if ((Time.time - waitForNextStepStartTime) > player.ComboTapTimeWindow)
                TransitionToNextState();

            return;
        }

        float currentStepDuration = currentStep == 1 ? attack1Duration : (currentStep == 2 ? attack2Duration : attack3Duration);
        if ((Time.time - stepStartTime) >= currentStepDuration)
            AttackAnimationFinished();
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

        bool isMovingHorizontally = Mathf.Abs(direction.x) > 0.1f;
        if (isMovingHorizontally)
        {
            player.ChangeState(player._isSprinting ? player.sprintState : player.walkState);
            return;
        }

        player.ChangeState(player.idleState);
    }

    public void AttackAnimationFinished()
    {
        if (currentStep < 3)
        {
            if (queuedNextStep)
            {
                StartAttackStep(currentStep + 1);
                queuedNextStep = false;
                return;
            }

            waitingForNextStepInput = true;
            waitForNextStepStartTime = Time.time;
            return;
        }

        TransitionToNextState();
    }

    public override void Exit()
    {
        currentStep = 1;
        queuedNextStep = false;
        waitingForNextStepInput = false;
        waitForNextStepStartTime = 0f;

        player._isAttacking = false;
        player._attackStep = 0;

        LogDebug("Attack State: Exited");
    }

    private bool TryHandleImmediateInputTransitions()
    {
        bool isGrounded = player._isGrounded;

        if (JumpPressed && isGrounded)
        {
            player.ResetJumpStates();
            player.ChangeState(player.jumpState);
            return true;
        }

        if (!isGrounded)
        {
            player.ChangeState(player._velocity.y > 0.1f ? player.jumpState : player.fallState);
            return true;
        }

        return false;
    }

    private void LockHorizontalMovement()
    {
        player.ApplyHorizontalMovement(0f, 0f);
    }

    private void StartAttackStep(int step)
    {
        currentStep = step;
        stepStartTime = Time.time;
        waitingForNextStepInput = false;

        if (player.combat != null)
            player.combat.ResetAttackHitGuard();

        // Set the attack step. Player.HandleAnimations() picks this up to trigger the animator.
        player._attackStep = step;
    }

    private void LogDebug(string message)
    {
        if (player.EnableStateLogs)
            Debug.Log(message);
    }
}
