using UnityEngine;

/// <summary>Terminal state when the player dies. Plays the death animation. Player.cs handles the timers and restart logic.</summary>
public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();
        if (animator != null)
            animator.SetBool("isDead", true);
    }

    public override void Exit()
    {
        base.Exit();
        if (animator != null)
            animator.SetBool("isDead", false);
    }
}
