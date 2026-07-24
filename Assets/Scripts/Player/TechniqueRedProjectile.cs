using UnityEngine;

/// <summary>Moves in a straight line, predictive-collides via CircleCast (avoids tunneling), and applies damage on hit.</summary>
public class TechniqueRedProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float damage = 30f;
    public float maxDistance = 20f;
    public LayerMask collisionLayers;

    private Vector2 _direction;
    private Vector3 _startPosition;

    public void Initialize(Vector2 direction, LayerMask layers)
    {
        _direction = direction.normalized;
        collisionLayers = layers;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        float step = speed * Time.fixedDeltaTime;
        Vector3 nextPosition = transform.position + (Vector3)_direction * step;

        if (Vector2.Distance(_startPosition, nextPosition) >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.5f, _direction, step, collisionLayers);
        if (hit.collider != null)
        {
            transform.position = hit.point;
            ApplyHit(hit.collider.gameObject);
            Destroy(gameObject);
            return;
        }

        transform.position = nextPosition;
    }

    private void ApplyHit(GameObject target)
    {
        Health health = target.GetComponent<Health>() ?? target.GetComponentInParent<Health>();
        if (health != null)
            health.ChangeHealth((int)-damage, transform.position);
    }
}
