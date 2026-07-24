using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's spell inventory, switching between spells, cooldowns, and casting.
/// Spell logic lives in each SpellSO; this only tracks which one is current and when it last fired.
/// </summary>
public class Magic : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Header("Spell State")]
    [SerializeField] private List<SpellSO> availableSpells = new List<SpellSO>();
    public IReadOnlyList<SpellSO> AvailableSpells => availableSpells;

    public int currentSpellIndex { get; private set; }

    public SpellSO CurrentSpell => (availableSpells != null && availableSpells.Count > 0 && currentSpellIndex >= 0 && currentSpellIndex < availableSpells.Count)
        ? availableSpells[currentSpellIndex] : null;

    private readonly Dictionary<SpellSO, float> _spellLastCastTimes = new Dictionary<SpellSO, float>();
    private bool _hitApplied;

    private void Awake()
    {
        if (player == null)
            player = GetComponent<Player>() ?? GetComponentInParent<Player>();
    }

    #region Spell Switching

    public void NextSpell()
    {
        if (availableSpells == null || availableSpells.Count == 0) return;
        currentSpellIndex = (currentSpellIndex + 1) % availableSpells.Count;
    }

    public void PreviousSpell()
    {
        if (availableSpells == null || availableSpells.Count == 0) return;
        currentSpellIndex = (currentSpellIndex - 1 + availableSpells.Count) % availableSpells.Count;
    }

    public void SetCurrentSpellIndex(int index)
    {
        if (availableSpells == null || availableSpells.Count == 0)
        {
            currentSpellIndex = 0;
            return;
        }

        currentSpellIndex = Mathf.Clamp(index, 0, availableSpells.Count - 1);
    }

    #endregion

    #region Casting

    /// <summary>Returns true if the current spell is off cooldown and ready to use.</summary>
    public bool CanCastCurrentSpell()
    {
        if (CurrentSpell == null) return false;
        if (!_spellLastCastTimes.ContainsKey(CurrentSpell)) return true;

        return Time.time - _spellLastCastTimes[CurrentSpell] >= CurrentSpell.cooldown;
    }

    /// <summary>Called from the spell cast animation's hit frame (or its timeout fallback).</summary>
    public void SpellCastHitFrame()
    {
        if (_hitApplied) return;
        _hitApplied = true;
        CastCurrentSpell();
    }

    public void ResetCast()
    {
        _hitApplied = false;
    }

    private void CastCurrentSpell()
    {
        if (CurrentSpell == null) return;

        _spellLastCastTimes[CurrentSpell] = Time.time;
        CurrentSpell.Cast(player);
    }

    public void SpellCastAnimationFinished()
    {
        if (player != null && player.currentState is PlayerSpellCastState state)
            state.SpellCastAnimationFinished();
    }

    #endregion
}
