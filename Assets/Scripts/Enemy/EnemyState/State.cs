using UnityEngine;

public abstract class State
{
    protected Enemy enemy;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected virtual string AnimBoolName => null;
    protected EnemyConfig config;
    protected Enemy_Senses enemySenses;
    protected StateMachine stateMachine;
    protected Enemy_Combat combat;

    protected State(Enemy enemy)
    {
        this.enemy = enemy;
        this.rb = enemy.RB;
        this.animator = enemy.Animator;
        config = enemy.config;
        this.enemySenses = enemy.enemySenses;
        this.stateMachine = enemy.StateMachine;
        this.combat = enemy.combat;
    }

    public virtual void Enter()
    {
        if (animator != null && !string.IsNullOrEmpty(AnimBoolName))
            animator.SetBool(AnimBoolName, true);
    }

    public virtual void OnAnimationFinished() { }

    public virtual void Exit()
    {
        if (animator != null && !string.IsNullOrEmpty(AnimBoolName))
            animator.SetBool(AnimBoolName, false);
    }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
