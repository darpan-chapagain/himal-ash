using UnityEngine;

[CreateAssetMenu(fileName = "TechniqueBlue", menuName = "Spells/TechniqueBlue")]
public class TechniqueBlueSO : SpellSO
{
    public override void Cast(Player player)
    {
        if (spellPrefab != null)
            Object.Instantiate(spellPrefab, player.transform.position, Quaternion.identity);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(player.transform.position, areaRadius, enemyLayer);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Health health = enemyCollider.GetComponent<Health>() ?? enemyCollider.GetComponentInParent<Health>();
            if (health != null)
                health.ChangeHealth((int)-damage, player.transform.position);
        }
    }
}
