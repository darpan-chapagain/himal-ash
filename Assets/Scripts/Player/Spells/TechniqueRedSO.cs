using UnityEngine;

[CreateAssetMenu(fileName = "TechniqueRed", menuName = "Spells/TechniqueRed")]
public class TechniqueRedSO : SpellSO
{
    public float maxDistance = 10f;

    public override void Cast(Player player)
    {
        if (spellPrefab == null)
        {
            Debug.LogWarning($"Spell prefab is missing on {spellName} SO!");
            return;
        }

        Vector2 direction = player.FacingRight ? Vector2.right : Vector2.left;
        Vector3 spawnPos = player.transform.position + (Vector3)direction * 0.8f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion spawnRot = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject fx = Object.Instantiate(spellPrefab, spawnPos, spawnRot);
        TechniqueRedProjectile projectile = fx.GetComponentInChildren<TechniqueRedProjectile>(true);
        if (projectile == null)
        {
            Debug.LogError("Failed to get TechniqueRedProjectile component from spawned object!");
            return;
        }

        projectile.speed = speed;
        projectile.damage = damage;
        projectile.maxDistance = maxDistance;
        projectile.Initialize(direction, enemyLayer);
    }
}
