using SQLite;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

/// <summary>Owns the SQLite connection and does raw CRUD against PlayerSaveData. No scene/networking knowledge.</summary>
public class LocalSaveManager
{
    private SQLiteConnection _connection;
    private string _databasePath;

    public LocalSaveManager()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Windows: C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[ProductName]\PlayerSaveData.db
        _databasePath = Path.Combine(Application.persistentDataPath, "PlayerSaveData.db");
        _connection = new SQLiteConnection(_databasePath);
        _connection.CreateTable<PlayerSaveData>();

        Debug.Log($"Database initialized at: {_databasePath}");
    }

    public void SavePlayerData(PlayerSaveData data)
    {
        try
        {
            data.SavedAtTicks = DateTime.UtcNow.Ticks;

            var existing = _connection.Table<PlayerSaveData>().Where(p => p.PlayerId == data.PlayerId).FirstOrDefault();
            if (existing != null)
            {
                _connection.Update(data);
                Debug.Log($"Updated save data for {data.PlayerId}");
            }
            else
            {
                _connection.Insert(data);
                Debug.Log($"Inserted new save data for {data.PlayerId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save player data: {ex.Message}");
        }
    }

    public PlayerSaveData LoadPlayerData(string playerId = "Player_One")
    {
        try
        {
            var data = _connection.Table<PlayerSaveData>().Where(p => p.PlayerId == playerId).FirstOrDefault();
            if (data != null)
            {
                Debug.Log($"Loaded save data for {playerId}");
                return data;
            }

            Debug.Log($"No save data found for {playerId}, returning defaults");
            return new PlayerSaveData { PlayerId = playerId };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load player data: {ex.Message}");
            return new PlayerSaveData { PlayerId = playerId };
        }
    }

    public bool HasSaveData(string playerId = "Player_One")
    {
        try
        {
            return _connection.Table<PlayerSaveData>().Any(p => p.PlayerId == playerId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to check save data: {ex.Message}");
            return false;
        }
    }

    public void DeletePlayerData(string playerId = "Player_One")
    {
        try
        {
            var existing = _connection.Table<PlayerSaveData>().Where(p => p.PlayerId == playerId).FirstOrDefault();
            if (existing != null)
            {
                _connection.Delete(existing);
                Debug.Log($"Deleted save data for {playerId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete player data: {ex.Message}");
        }
    }

    // Call this when the application quits.
    public void CloseDatabase()
    {
        if (_connection != null)
        {
            _connection.Close();
            Debug.Log("Database connection closed");
        }
    }
}
