using UnityEngine;

/// <summary>Scales a sprite up from startScale to targetScale over duration, then fades it out and destroys it.</summary>
public class ExpandingEffect : MonoBehaviour
{
    [Header("Expansion Settings")]
    public float startScale = 0.1f;
    public float targetScale = 5f;
    public float duration = 0.5f;
    public float fadeDuration = 0.2f;

    private float _timer;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;
        _spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float t = _timer / duration;

        if (t <= 1f)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);
            return;
        }

        float fadeT = (_timer - duration) / fadeDuration;
        if (fadeT <= 1f)
        {
            if (_spriteRenderer != null)
            {
                Color color = _spriteRenderer.color;
                color.a = 1f - fadeT;
                _spriteRenderer.color = color;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
