using System;
using UnityEngine;

/// <summary>Shared HP component used by both Player and Enemy. Single-player: no networking.</summary>
public class Health : MonoBehaviour
{
    public event Action<Vector2> OnDamaged;
    public event Action<Vector2> OnDeath;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private float _lastObservedHealth = float.MinValue;
    private Vector2 _lastDamageSource;
    private bool _deathEventSent;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
        _lastObservedHealth = currentHealth;
        _deathEventSent = currentHealth <= 0f;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void ChangeHealth(int amount) => ChangeHealth(amount, transform.position);

    public void ChangeHealth(int amount, Vector2 sourcePosition)
    {
        if (amount == 0) return;

        float before = currentHealth;
        float after = Mathf.Clamp(before + amount, 0f, maxHealth);
        if (Mathf.Approximately(before, after)) return;

        if (amount < 0)
            _lastDamageSource = sourcePosition;

        currentHealth = after;
        NotifyHealthEventsIfChanged(currentHealth, sourcePosition);
    }

    public float GetHealth() => currentHealth;

    public void SetHealth(float amount)
    {
        currentHealth = Mathf.Clamp(amount, 0f, maxHealth);
        _lastObservedHealth = currentHealth;
    }

    private void NotifyHealthEventsIfChanged(float observedHealth, Vector2? sourcePosition)
    {
        if (_lastObservedHealth == float.MinValue)
        {
            _lastObservedHealth = observedHealth;
            return;
        }

        if (Mathf.Approximately(observedHealth, _lastObservedHealth))
            return;

        float delta = observedHealth - _lastObservedHealth;
        _lastObservedHealth = observedHealth;

        if (delta < 0f)
        {
            if (observedHealth <= 0f)
            {
                if (!_deathEventSent)
                {
                    _deathEventSent = true;
                    Vector2 deathSource = sourcePosition ?? _lastDamageSource;
                    OnDeath?.Invoke(deathSource);
                    Debug.Log($"{gameObject.name} is dead!");
                }
            }
            else
            {
                _deathEventSent = false;
                OnDamaged?.Invoke(sourcePosition ?? _lastDamageSource);
            }

            return;
        }

        if (delta > 0f)
            _deathEventSent = observedHealth <= 0f;
    }
}
