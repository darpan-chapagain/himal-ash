using UnityEngine;

/// <summary>
/// Base class for all player states (Idle, Walk, Jump, Attack, etc.).
/// Each state has Enter(), Update(), FixedUpdate(), and Exit() hooks.
/// Player calls FixedUpdate() then Update() on the current state every physics step.
/// </summary>
public abstract class PlayerState
{
    protected Player player;
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Damage damage;
    protected Combat combat;

    public PlayerState(Player player)
    {
        this.player = player;
        this.animator = player.animator;
        this.rb = player.GetComponent<Rigidbody2D>();
        this.damage = player.damage;
        this.combat = player.combat;
    }

    public bool JumpPressed => player.jumpPressed;
    public bool JumpReleased => player.jumpReleased;
    public bool AttackPressed => player.attackPressed;
    public bool AttackReleased => player.attackReleased;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
