using UnityEngine;

/// <summary>Trigger volume that requests a scene transition once the Player enters it.</summary>
public class DoorScript : MonoBehaviour
{
    [SerializeField, Tooltip("Scene to load when a Player enters. Leave empty if this door has no valid destination yet.")]
    private string targetSceneName = "Level2";
    [SerializeField] private string spawnId = "Spawn_A";

    private bool _isTransitioning;
    private int _playersInside;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isTransitioning || collision == null) return;

        Player player = collision.GetComponentInParent<Player>() ?? collision.GetComponent<Player>();
        if (player == null) return;

        _playersInside++;
        TryTransition();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;

        Player player = collision.GetComponentInParent<Player>() ?? collision.GetComponent<Player>();
        if (player == null) return;

        _playersInside = Mathf.Max(0, _playersInside - 1);
    }

    private void TryTransition()
    {
        if (_isTransitioning || _playersInside <= 0) return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.Log("DoorScript: Player entered door, but no destination scene is configured yet.");
            return;
        }

        _isTransitioning = true;
        RoomTransitionManager.RequestTransition(targetSceneName, spawnId);
    }
}
