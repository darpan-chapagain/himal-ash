using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Core single-player controller: input, state machine, movement, animations, death.
/// </summary>
public class Player : MonoBehaviour
{
    #region Singleton & Constants

    public static Player Instance;
    public bool FacingRight => _facingRight;
    private const int SpellDamage = 10;

    #endregion

    #region State Machine

    public PlayerState currentState;
    public PlayerIdle idleState;
    public PlayerJump jumpState;
    public PlayerFall fallState;
    public PlayerWalk walkState;
    public PlayerSprint sprintState;
    public PlayerAttackState attackState;
    public PlayerSpellCastState spellCastState;
    public PlayerWallJumpState wallJumpState;
    public PlayerBlockState blockState;
    public PlayerDamagedState damagedState;
    public PlayerDeathState deathState;

    #endregion

    #region Runtime State

    private bool _facingRight = true;
    public int _jumpCount;
    public float speedMultiplier = 1f;

    public bool _isGrounded = true;
    public Vector2 _direction;
    public Vector2 _velocity;
    public bool _isSprinting;

    public bool _isIdle = true;
    public bool _isWalking;
    public bool _isJumping;
    public bool _isAttacking;
    public bool _isCasting;
    public bool _isWallJumping;
    public bool _isBlocking;
    public bool _isDamaged;
    public int _attackStep;
    public bool _isDead;

    private float _deathAnimHoldEndTime;
    private float _returnToMenuEndTime;
    private float _deathKnockbackPhysicsEndTime;
    private bool _showDeathUiPhase;
    private float _spawnGraceEndTime;
    private bool _restartPending;

    #endregion

    #region Inspector Settings

    [Header("Jump Settings")]
    [SerializeField] public float jumpForce = 12f;
    [SerializeField] public int maxJumps = 2;
    [SerializeField] public float doubleJumpForce = 10f;
    [SerializeField] public Vector2 wallJumpForce = new Vector2(10f, 15f);
    [SerializeField] public float airAcceleration = 15f;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float doubleTapTimeWindow = 0.3f;

    [Header("Combo Settings")]
    [SerializeField] private float comboTapTimeWindow = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool enableStateLogs = false;
    [SerializeField] private bool enableCombatLogs = false;
    [SerializeField] private float fallThreshold = -20f;

    [Header("Death")]
    [SerializeField] private float deathKnockbackHorizontal = 5.5f;
    [SerializeField] private float deathKnockbackUpward = 2.8f;
    [SerializeField] private float deathKnockbackPhysicsDuration = 0.24f;
    [Tooltip("Time after death before showing YOU DIED (let death clip play). Match your Dead animation length.")]
    [SerializeField] private float deathAnimationHoldSeconds = 0.88f;
    [Tooltip("How long the YOU DIED overlay stays before restarting.")]
    [SerializeField] private float deathUiSeconds = 3f;

    [Header("Dynamic Gravity Settings")]
    [SerializeField] private float fallMultiplier = 3.0f;
    [SerializeField] private float lowJumpMultiplier = 2.5f;

    [Header("Wall Jump Settings")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.25f;
    public LayerMask wallLayer;
    public bool touchingWall;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 boxSize = new Vector2(0.3f, 0.1f);
    [SerializeField] private float castDistance = 0.2f;

    [Header("References")]
    public PlayerInput playerInput;
    public Combat combat;
    public Damage damage;
    public Animator animator;

    #endregion

    #region Public Accessors

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float ComboTapTimeWindow => comboTapTimeWindow;
    public bool EnableStateLogs => enableStateLogs;
    public bool EnableCombatLogs => enableCombatLogs;
    public float FallMultiplier => fallMultiplier;
    public float LowJumpMultiplier => lowJumpMultiplier;

    #endregion

    #region Private Fields

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _spellcastAction;
    private InputAction _blockAction;

    private float _lastTapTime;
    private float _lastDirection;
    private bool _wasMoving;

    public bool jumpPressed { get; private set; }
    public bool jumpReleased { get; private set; }
    public bool attackPressed { get; private set; }
    public bool attackReleased { get; private set; }
    public bool spellCastPressed { get; private set; }
    public bool blockPressed { get; private set; }
    public bool blockReleased { get; private set; }

    private bool _prevJump, _prevAttack, _prevSpell, _prevBlock;
    private int _lastProcessedAttackStep;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        idleState = new PlayerIdle(this);
        jumpState = new PlayerJump(this);
        fallState = new PlayerFall(this);
        walkState = new PlayerWalk(this);
        sprintState = new PlayerSprint(this);
        attackState = new PlayerAttackState(this);
        spellCastState = new PlayerSpellCastState(this);
        wallJumpState = new PlayerWallJumpState(this);
        blockState = new PlayerBlockState(this);
        damagedState = new PlayerDamagedState(this, 1, 0f, 0f);
        deathState = new PlayerDeathState(this);
        currentState = idleState;

        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        if (combat == null)
            combat = GetComponent<Combat>() ?? GetComponentInChildren<Combat>();
        if (damage == null)
            damage = GetComponent<Damage>() ?? GetComponentInChildren<Damage>();
    }

    private void Start()
    {
        Instance = this;
        SetupPlayerInput();

        _spawnGraceEndTime = Time.time + 0.25f;
        _isGrounded = true;
        _isIdle = true;
        currentState = idleState;
        currentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        LogDebug($"ChangeState: From {currentState?.GetType().Name} to {newState.GetType().Name}");
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void ResetJumpStates()
    {
        jumpPressed = false;
        jumpReleased = false;
    }

    public void ResetAttackStates()
    {
        attackPressed = false;
        attackReleased = false;
    }

    #endregion

    #region Input Reading

    private void SetupPlayerInput()
    {
        if (playerInput == null) return;

        playerInput.enabled = true;
        playerInput.ActivateInput();

        _moveAction = playerInput.actions["Move"];
        _jumpAction = playerInput.actions["Jump"];
        _attackAction = playerInput.actions["Attack"];
        _spellcastAction = playerInput.actions["Spellcast"];
        _blockAction = playerInput.actions["Block"];
    }

    private Vector2 ReadMoveInput()
    {
        return _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
    }

    private bool IsPointerOverUI() =>
        UnityEngine.EventSystems.EventSystem.current != null &&
        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

    #endregion

    #region Main Loop

    private void FixedUpdate()
    {
        if (_isDead)
        {
            HandleDeathTick();
            return;
        }

        Vector2 direction = ReadMoveInput();
        bool pointerOverUI = IsPointerOverUI();
        bool jumpDown = _jumpAction != null && _jumpAction.IsPressed();
        bool attackDown = _attackAction != null && _attackAction.IsPressed() && !pointerOverUI;
        bool spellDown = _spellcastAction != null && _spellcastAction.IsPressed() && !pointerOverUI;
        bool blockDown = _blockAction != null && _blockAction.IsPressed() && !pointerOverUI;

        bool grounded = IsGrounded();

        if (transform.position.y < fallThreshold)
        {
            Die();
            return;
        }

        jumpPressed = jumpDown && !_prevJump;
        jumpReleased = !jumpDown && _prevJump;
        attackPressed = attackDown && !_prevAttack;
        attackReleased = !attackDown && _prevAttack;
        bool spellPressed = spellDown && !_prevSpell;
        spellCastPressed = spellPressed;
        blockPressed = blockDown && !_prevBlock;
        blockReleased = !blockDown && _prevBlock;

        if (!grounded && _rb != null && Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
            grounded = true;

        _isGrounded = grounded;
        _direction = direction;

        if (grounded)
            _jumpCount = 0;

        HandleFlipLogic(direction);

        if (spellPressed)
            TryEnterSpellCastState();

        currentState?.FixedUpdate();
        currentState?.Update();

        if (_rb != null)
            _velocity = _rb.linearVelocity;

        ApplyDynamicGravity(grounded);

        _prevJump = jumpDown;
        _prevAttack = attackDown;
        _prevSpell = spellDown;
        _prevBlock = blockDown;
    }

    private void Update()
    {
        transform.localScale = new Vector3(_facingRight ? 1f : -1f, 1f, 1f);
        HandleAnimations();
    }

    /// <summary>Handles the death timer sequence: play death anim -> show "YOU DIED" -> restart.</summary>
    private void HandleDeathTick()
    {
        if (Time.time >= _deathAnimHoldEndTime)
        {
            if (!_showDeathUiPhase)
            {
                _showDeathUiPhase = true;
                _returnToMenuEndTime = Time.time + deathUiSeconds;
            }
            else if (Time.time >= _returnToMenuEndTime)
            {
                RestartSceneAfterDeath();
            }
        }

        if (_rb != null)
        {
            if (Time.time < _deathKnockbackPhysicsEndTime)
            {
                _rb.simulated = true;
                Vector2 v = _rb.linearVelocity;
                v.x *= 0.9f;
                _rb.linearVelocity = v;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.simulated = false;
            }
        }
    }

    #endregion

    #region Movement & Physics

    /// <summary>Detects double-tap sprint: if the player taps the same direction twice quickly, enable sprinting.</summary>
    public void UpdateSprintStateFromDirection(Vector2 direction)
    {
        bool isMoving = Mathf.Abs(direction.x) > 0.1f;

        if (isMoving && !_wasMoving)
        {
            float currentDirection = Mathf.Sign(direction.x);
            float currentTime = Time.time;

            if (Mathf.Abs(currentDirection - _lastDirection) < 0.1f &&
                currentTime - _lastTapTime < doubleTapTimeWindow)
            {
                _isSprinting = true;
            }

            _lastTapTime = currentTime;
            _lastDirection = currentDirection;
        }

        if (!isMoving || (isMoving && Mathf.Abs(Mathf.Sign(direction.x) - _lastDirection) > 0.1f))
            _isSprinting = false;

        _wasMoving = isMoving;
    }

    /// <summary>Sets horizontal velocity directly.</summary>
    public void ApplyHorizontalMovement(float directionX, float speed)
    {
        if (_rb == null) return;

        float targetVx = directionX * speed * speedMultiplier;
        _rb.linearVelocity = new Vector2(targetVx, _rb.linearVelocity.y);
    }

    private void HandleFlipLogic(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.1f)
            _facingRight = direction.x > 0;
    }

    /// <summary>Makes falling feel heavier and short-hops snappier by scaling gravity based on velocity.</summary>
    private void ApplyDynamicGravity(bool isGrounded)
    {
        if (isGrounded || _rb == null) return;

        if (_rb.linearVelocity.y < -0.01f)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (_rb.linearVelocity.y > 0.01f && !(_jumpAction != null && _jumpAction.IsPressed()))
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    #endregion

    #region Ground / Wall / Ceiling Checks

    public bool IsGrounded()
    {
        if (_collider == null) return false;

        if (Time.time < _spawnGraceEndTime)
            return true;

        return EvaluateGroundedFromPhysics();
    }

    private bool EvaluateGroundedFromPhysics()
    {
        if (_collider == null) return false;

        if (_collider.IsTouchingLayers(groundLayer))
            return true;

        float checkHeight = 0.5f;
        Vector2 checkOrigin = new Vector2(_collider.bounds.center.x, _collider.bounds.min.y + (checkHeight / 2f));
        Vector2 size = new Vector2(_collider.bounds.size.x * 0.9f, checkHeight);

        RaycastHit2D hit = Physics2D.BoxCast(checkOrigin, size, 0f, Vector2.down, castDistance + 0.1f, groundLayer);
        if (hit.collider != null) return true;

        Collider2D overlap = Physics2D.OverlapBox(checkOrigin, size, 0f, groundLayer);
        return overlap != null;
    }

    public bool IsTouchingWall()
    {
        if (wallCheck == null) return false;
        touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        return touchingWall;
    }

    /// <summary>Returns the direction to push the player AWAY from the wall they're touching.</summary>
    public Vector2 GetWallDirection()
    {
        if (wallCheck == null) return _facingRight ? Vector2.left : Vector2.right;

        Vector2 leftCheckPos = new Vector2(wallCheck.position.x - 0.1f, wallCheck.position.y);
        bool touchingLeft = Physics2D.OverlapCircle(leftCheckPos, wallCheckRadius, wallLayer);

        Vector2 rightCheckPos = new Vector2(wallCheck.position.x + 0.1f, wallCheck.position.y);
        bool touchingRight = Physics2D.OverlapCircle(rightCheckPos, wallCheckRadius, wallLayer);

        if (touchingLeft && touchingRight)
            return _facingRight ? Vector2.left : Vector2.right;
        if (touchingLeft) return Vector2.right;
        if (touchingRight) return Vector2.left;

        return _facingRight ? Vector2.left : Vector2.right;
    }

    public bool IsTouchingCeiling()
    {
        if (_collider == null) return false;
        float checkDist = 0.15f;
        Vector2 size = new Vector2(_collider.bounds.size.x * 0.8f, checkDist);
        Vector2 checkOrigin = new Vector2(_collider.bounds.center.x, _collider.bounds.max.y + (checkDist / 2));
        Collider2D overlap = Physics2D.OverlapBox(checkOrigin, size, 0f, groundLayer);
        return overlap != null;
    }

    #endregion

    #region Animation

    /// <summary>Pushes all state booleans into the Animator so visuals match the FSM.</summary>
    private void HandleAnimations()
    {
        if (animator == null) return;

        if (_isDead)
        {
            animator.SetBool("isDead", true);
            animator.SetBool("isDamaged", false);
            return;
        }

        animator.SetBool("isDead", false);
        animator.SetBool("isDamaged", _isDamaged);

        bool groundedForAnim = _isGrounded || EvaluateGroundedFromPhysics();

        animator.SetBool("isGrounded", groundedForAnim);
        animator.SetBool("isIdle", _isIdle);
        animator.SetBool("isWalking", _isWalking);
        animator.SetBool("isSprinting", _isSprinting);
        animator.SetBool("isJumping", _isJumping);
        animator.SetBool("isAttacking", _isAttacking);
        animator.SetBool("isCasting", _isCasting);
        animator.SetBool("isWallJumping", _isWallJumping);
        animator.SetBool("isBlocking", _isBlocking);

        float yVelocityForAnim = groundedForAnim ? 0f : _velocity.y;
        animator.SetFloat("yVelocity", yVelocityForAnim);

        if (_attackStep != _lastProcessedAttackStep)
        {
            if (_attackStep > 0)
                TriggerAttackAnimation(_attackStep);
            else
                ResetAttackTriggers();
            _lastProcessedAttackStep = _attackStep;
        }
    }

    private void TriggerAttackAnimation(int step)
    {
        if (animator == null) return;

        string stepStateName = step == 1 ? "Attack1" : (step == 2 ? "Attack2" : "Attack3");

        if (TryCrossFadeAttackState(stepStateName) || TryCrossFadeAttackState("Attack"))
            return;

        if (HasAnimatorParameter(stepStateName, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(stepStateName);
        else if (HasAnimatorParameter("Attack", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Attack");
    }

    private bool TryCrossFadeAttackState(string stateName)
    {
        if (animator == null) return false;
        int stateHash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, stateHash)) return false;
        animator.CrossFade(stateHash, 0.02f, 0, 0f);
        return true;
    }

    private void ResetAttackTriggers()
    {
        if (animator == null) return;
        ResetTriggerIfExists("Attack1");
        ResetTriggerIfExists("Attack2");
        ResetTriggerIfExists("Attack3");
        ResetTriggerIfExists("Attack");
    }

    private void ResetTriggerIfExists(string triggerName)
    {
        if (HasAnimatorParameter(triggerName, AnimatorControllerParameterType.Trigger))
            animator.ResetTrigger(triggerName);
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

    #endregion

    #region Death

    /// <summary>Called by Damage when HP reaches zero.</summary>
    public void HandleFatalDamage(Vector2 damageWorldSource)
    {
        BeginDeathSequence(damageWorldSource, "Player died from damage!");
    }

    private void Die()
    {
        BeginDeathSequence(null, "Player fell off the map! Dying...");
    }

    private void BeginDeathSequence(Vector2? damageWorldSource, string logMessage)
    {
        if (_isDead) return;

        Debug.Log(logMessage);
        ApplyDeathKnockbackImpulse(damageWorldSource);

        _isDead = true;
        _isDamaged = false;
        _showDeathUiPhase = false;
        _deathAnimHoldEndTime = Time.time + deathAnimationHoldSeconds;
        _deathKnockbackPhysicsEndTime = Time.time + deathKnockbackPhysicsDuration;
        ChangeState(deathState);
    }

    public bool ShouldShowDeathOverlay()
    {
        return _isDead && _showDeathUiPhase && Time.time < _returnToMenuEndTime;
    }

    private void ApplyDeathKnockbackImpulse(Vector2? damageWorldSource)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) return;

        float dirX;
        float addY = deathKnockbackUpward;
        if (damageWorldSource.HasValue && damageWorldSource.Value.sqrMagnitude > 1e-4f)
        {
            dirX = damageWorldSource.Value.x <= transform.position.x ? 1f : -1f;
        }
        else
        {
            dirX = Mathf.Abs(transform.localScale.x) > 0.01f ? -Mathf.Sign(transform.localScale.x) : 1f;
            addY *= 0.5f;
        }

        _rb.simulated = true;
        float vy = Mathf.Max(_rb.linearVelocity.y, 0f) + addY;
        _rb.linearVelocity = new Vector2(dirX * deathKnockbackHorizontal, vy);
    }

    /// <summary>Placeholder until a menu/scene system exists: just reloads the current scene.</summary>
    private void RestartSceneAfterDeath()
    {
        if (_restartPending) return;
        _restartPending = true;
        StartCoroutine(RestartSceneDeferred());
    }

    private IEnumerator RestartSceneDeferred()
    {
        yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion

    #region Spell Cast

    private void TryEnterSpellCastState()
    {
        if (spellCastState == null) { SpellAttack(); return; }
        if (currentState is PlayerSpellCastState) return;
        ChangeState(spellCastState);
    }

    public void SpellAttack()
    {
        if (combat != null)
            combat.TryAttack(SpellDamage);
    }

    #endregion

    #region Debug

    private void LogDebug(string message)
    {
        if (enableStateLogs)
            Debug.Log(message);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, boxSize.x / 2);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }

    #endregion
}
