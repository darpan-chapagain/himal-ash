using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Single-player scene-transition coordinator. Loads the target scene, then moves the
/// Player already present in that scene to the RoomSpawnPoint matching the requested spawn id.
public class RoomTransitionManager : MonoBehaviour
{
    public static string PendingSpawnId { get; private set; }
    private static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoadedHook()
    {
        if (_sceneHookRegistered) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _sceneHookRegistered = true;
    }

    public static void RequestTransition(string targetSceneName, string spawnId)
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("RoomTransitionManager: target scene name is empty.");
            return;
        }

        PendingSpawnId = spawnId;
        SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrWhiteSpace(PendingSpawnId))
            return;

        RoomSpawnPoint target = FindTargetSpawnPoint(scene.name);
        if (target == null)
            return;

        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning($"RoomTransitionManager: no Player found in scene '{scene.name}' to place at spawn '{PendingSpawnId}'.");
            return;
        }

        PlacePlayerAtSpawn(player, target.transform.position);
        PendingSpawnId = null;
    }

    private static RoomSpawnPoint FindTargetSpawnPoint(string sceneName)
    {
        RoomSpawnPoint[] points = Object.FindObjectsByType<RoomSpawnPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].SpawnId == PendingSpawnId)
                return points[i];
        }

        Debug.LogWarning($"RoomTransitionManager: no RoomSpawnPoint found for id '{PendingSpawnId}' in scene '{sceneName}'.");
        return null;
    }

    private static void PlacePlayerAtSpawn(Player player, Vector3 spawnPosition)
    {
        player.transform.position = spawnPosition;
        if (player.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.position = spawnPosition;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
