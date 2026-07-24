using UnityEngine;

/// <summary>Trigger volume that requests a scene transition once the Player enters it.</summary>
public class DoorScript : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Level2";
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

        _isTransitioning = true;
        Debug.Log($"DoorScript: Player entered door -> would transition to '{targetSceneName}' at spawn '{spawnId}'.");

        // No RoomTransitionManager exists in this project yet (the reference's scene-transition
        // coordinator). Wire this up to it once that system is built; for now this only logs.
    }
}
