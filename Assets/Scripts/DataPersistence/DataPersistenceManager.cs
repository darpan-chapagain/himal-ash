using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Bridges LocalSaveManager to the live Player. Save/load are manual triggers only (no autosave),
/// matching the reference project's ButtonController-driven save/load flow.
/// </summary>
public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance;

    private const string PlayerId = "Player_One";

    [Header("Test-only keybinds — temporary scaffolding until real save/load UI exists")]
    [SerializeField] private bool enableTestKeybinds = true;

    private LocalSaveManager _saveManager;

    private void Awake()
    {
        Instance = this;
        _saveManager = new LocalSaveManager();
    }

    private void Update()
    {
        if (!enableTestKeybinds || Keyboard.current == null) return;

        if (Keyboard.current.f5Key.wasPressedThisFrame)
            SaveGame();
        else if (Keyboard.current.f9Key.wasPressedThisFrame)
            LoadGame();
    }

    public void SaveGame()
    {
        Player player = Player.Instance;
        if (player == null)
        {
            Debug.LogWarning("SaveGame: no Player instance found.");
            return;
        }

        Health health = player.GetComponent<Health>() ?? player.GetComponentInChildren<Health>();
        int savedHealth = health != null ? Mathf.RoundToInt(health.GetHealth()) : 0;

        Magic magic = player.magic;
        string equippedSpellId = magic != null ? BuildSpellId(magic.CurrentSpell) : null;

        PlayerSaveData data = new PlayerSaveData
        {
            PlayerId = PlayerId,
            SceneName = SceneManager.GetActiveScene().name,
            X = player.transform.position.x,
            Y = player.transform.position.y,
            Health = savedHealth,
            EquippedSpellId = equippedSpellId
        };

        _saveManager.SavePlayerData(data);
        Debug.Log($"Game saved: pos=({data.X}, {data.Y}) health={data.Health} scene={data.SceneName} spell={data.EquippedSpellId}");
    }

    public void LoadGame()
    {
        // Without this guard, a missing save returns default data (X=0, Y=0, Health=0),
        // which would teleport the player to the origin and kill them on "load".
        if (!_saveManager.HasSaveData(PlayerId))
        {
            Debug.Log("LoadGame: no save data found, leaving player at spawn.");
            return;
        }

        Player player = Player.Instance;
        if (player == null)
        {
            Debug.LogWarning("LoadGame: no Player instance found.");
            return;
        }

        PlayerSaveData data = _saveManager.LoadPlayerData(PlayerId);

        Vector3 loadedPosition = new Vector3(data.X, data.Y, player.transform.position.z);
        player.transform.position = loadedPosition;

        Health health = player.GetComponent<Health>() ?? player.GetComponentInChildren<Health>();
        if (health != null)
            health.SetHealth(data.Health);

        Magic magic = player.magic;
        if (magic != null && !string.IsNullOrWhiteSpace(data.EquippedSpellId))
        {
            var spells = magic.AvailableSpells;
            for (int i = 0; i < spells.Count; i++)
            {
                if (BuildSpellId(spells[i]) == data.EquippedSpellId)
                {
                    magic.SetCurrentSpellIndex(i);
                    break;
                }
            }
        }

        Debug.Log($"Game loaded: pos=({data.X}, {data.Y}) health={data.Health} spell={data.EquippedSpellId}");
    }

    private static string BuildSpellId(SpellSO spell)
    {
        if (spell == null) return null;

        string rawId = string.IsNullOrWhiteSpace(spell.spellName) ? spell.name : spell.spellName;
        if (string.IsNullOrWhiteSpace(rawId))
            return null;

        return rawId.Trim().ToLowerInvariant().Replace(" ", "_");
    }

    private void OnDestroy()
    {
        _saveManager?.CloseDatabase();
    }
}
