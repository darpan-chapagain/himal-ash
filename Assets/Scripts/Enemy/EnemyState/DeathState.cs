public class DeathState : State
{
    protected override string AnimBoolName => "isDead";

    public DeathState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        base.Enter();
        enemy.ApplySharedDeathImmediateEffects();
    }
}
