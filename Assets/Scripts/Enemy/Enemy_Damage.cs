using UnityEngine;

public class Enemy_Damage : MonoBehaviour
{
    private Enemy _enemy;
    private Health _health;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>() ?? GetComponentInParent<Enemy>();
        _health = GetComponent<Health>() ?? GetComponentInParent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDamaged += HandleDamage;
            _health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDamaged -= HandleDamage;
            _health.OnDeath -= HandleDeath;
        }
    }

    private void HandleDamage(Vector2 sourcePosition)
    {
        if (_enemy == null || _enemy.IsDead()) return;

        int knockbackDir = sourcePosition.x <= transform.position.x ? 1 : -1;
        _enemy.StateMachine?.ChangeState(new DamagedState(_enemy, knockbackDir));
    }

    private void HandleDeath(Vector2 _)
    {
        if (_enemy == null || _enemy.IsDead()) return;
        _enemy.StateMachine?.ChangeState(new DeathState(_enemy));
    }
}
