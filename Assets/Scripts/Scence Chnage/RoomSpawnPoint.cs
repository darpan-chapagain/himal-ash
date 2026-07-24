using UnityEngine;

public class RoomSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "Spawn_A";
    public string SpawnId => spawnId;
}
