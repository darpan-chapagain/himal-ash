using SQLite;

[Table("PlayerSaveData")]
public class PlayerSaveData
{
    [PrimaryKey]
    public string PlayerId { get; set; } = "Player_One";

    public string SceneName { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public int Health { get; set; }
    public long SavedAtTicks { get; set; }
}
