using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyConfig config;
    public Health health;
    public Rigidbody2D RB { get; private set; }
    public StateMachine StateMachine { get; private set; }
    public int FacingDirection { get; private set; } = 1;

    public Enemy_Combat combat { get; private set; }
    public Animator Animator { get; private set; }
    public Enemy_Senses enemySenses { get; private set; }

    private bool _isDead;
    private bool _deathPresentationApplied;
    private Coroutine _deathHideRoutine;
    private const float DeathHideDelaySeconds = 1.2f;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        StateMachine = new StateMachine();
        enemySenses = GetComponent<Enemy_Senses>();
        Animator = GetComponentInChildren<Animator>(true);
        combat = GetComponent<Enemy_Combat>();
    }

    private void Start()
    {
        if (health != null && health.GetHealth() <= 0f)
        {
            StateMachine.Initialize(new DeathState(this));
            return;
        }

        StateMachine.Initialize(new IdleState(this));
    }

    private void FixedUpdate()
    {
        if (!_isDead && health != null && health.GetHealth() <= 0f)
        {
            StateMachine.ChangeState(new DeathState(this));
            return;
        }

        StateMachine.CurrentState?.FixedUpdate();
        StateMachine.CurrentState?.Update();
    }

    /// <summary>Animation event hook: call this when the attack/damaged clip finishes.</summary>
    public void OnAnimationFinished()
    {
        StateMachine.CurrentState?.OnAnimationFinished();
    }

    /// <summary>Animation event hook: call this on the exact melee hit frame.</summary>
    public void OnMeleeAttackHit()
    {
        if (_isDead) return;
        if (StateMachine.CurrentState is MeleeAttackState attackState)
            attackState.ApplyAttackHit();
    }

    public void Flip()
    {
        FacingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * FacingDirection;
        transform.localScale = scale;
    }

    public void FaceTarget(Transform target)
    {
        float offset = target.position.x - transform.position.x;
        int directionOffset = offset >= 0f ? 1 : -1;
        if (directionOffset != FacingDirection)
            Flip();
    }

    public void SetDead(bool isDead) => _isDead = isDead;
    public bool IsDead() => _isDead;

    /// <summary>Disables physics/collider, plays the death animation, then hides the GameObject after a delay.</summary>
    internal void ApplySharedDeathImmediateEffects()
    {
        if (_deathPresentationApplied) return;
        _deathPresentationApplied = true;
        SetDead(true);

        if (RB != null)
        {
            RB.linearVelocity = Vector2.zero;
            RB.simulated = false;
        }

        if (TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        if (Animator != null)
            Animator.SetBool("isDead", true);

        if (_deathHideRoutine == null && isActiveAndEnabled)
            _deathHideRoutine = StartCoroutine(HideAfterDeathDelayCoroutine());
    }

    private IEnumerator HideAfterDeathDelayCoroutine()
    {
        yield return new WaitForSeconds(DeathHideDelaySeconds);
        if (this != null)
            gameObject.SetActive(false);
        _deathHideRoutine = null;
    }
}
