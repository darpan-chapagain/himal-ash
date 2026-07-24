using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/SpellSO")]
public abstract class SpellSO : ScriptableObject
{
    public abstract void Cast(Player player);

    [Header("Basic Info")]
    public string spellName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Combat Stats")]
    public float damage = 10f;
    public float cooldown = 4f;
    public float speed = 10f;
    public float areaRadius = 5f;
    public LayerMask enemyLayer;

    [Header("Visuals")]
    public GameObject spellPrefab;
}
