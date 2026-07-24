using UnityEngine;

/// <summary>Marks a position where an enemy was meant to be placed. Purely a record of intent -
/// the reference project's runtime spawner (BasicSpawner) that consumed these markers isn't part
/// of this single-player port, so enemies are placed statically instead of spawned from this marker.
public class LevelEnemySpawnMarker : MonoBehaviour
{
    public GameObject enemyPrefab;
}
