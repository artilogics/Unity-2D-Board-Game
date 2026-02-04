using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance { get; private set; }

    private Dictionary<int, PlayerData> playersData = new Dictionary<int, PlayerData>();

    public class PlayerData
    {
        public int points = 0;
        public HashSet<string> completedCategories = new HashSet<string>();
    }

    [Header("Settings")]
    public int pointsForCorrect = 10;
    public int pointsForWrong = -5;
    public int categoriesToWin = 8;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Helper to get or create data
    private PlayerData GetData(int playerNumber)
    {
        if (!playersData.ContainsKey(playerNumber))
        {
            playersData[playerNumber] = new PlayerData();
        }
        return playersData[playerNumber];
    }

    // Add points for a player
    public void AddPoints(int playerNumber, int points)
    {
        PlayerData data = GetData(playerNumber);
        data.points += points;
        data.points = Mathf.Max(0, data.points); // Don't go negative
        Debug.Log($"Player {playerNumber} points: {data.points}");
    }

    // Mark a category as completed for a player
    public void CompleteCategory(int playerNumber, string category)
    {
        PlayerData data = GetData(playerNumber);
        if (!data.completedCategories.Contains(category))
        {
            data.completedCategories.Add(category);
            Debug.Log($"Player {playerNumber} completed category: {category} ({data.completedCategories.Count}/{categoriesToWin})");
        }
    }

    // Check if player has completed a category
    public bool HasCompletedCategory(int playerNumber, string category)
    {
        if (playersData.ContainsKey(playerNumber))
        {
            return playersData[playerNumber].completedCategories.Contains(category);
        }
        return false;
    }

    // Check if player is ready for final round
    public bool IsReadyForFinal(int playerNumber)
    {
        if (playersData.ContainsKey(playerNumber))
        {
            return playersData[playerNumber].completedCategories.Count >= categoriesToWin;
        }
        return false;
    }

    // Get current points for a player
    public int GetPoints(int playerNumber)
    {
        return playersData.ContainsKey(playerNumber) ? playersData[playerNumber].points : 0;
    }

    // Get completed category count
    public int GetCompletedCategoryCount(int playerNumber)
    {
        return playersData.ContainsKey(playerNumber) ? playersData[playerNumber].completedCategories.Count : 0;
    }
    
    // Get actual Set of categories (for UI Sockets)
    public HashSet<string> GetCompletedCategories(int playerNumber)
    {
        return playersData.ContainsKey(playerNumber) ? playersData[playerNumber].completedCategories : new HashSet<string>();
    }

    // Reset all progress
    public void ResetProgress()
    {
        playersData.Clear();
        Debug.Log("PlayerProgress: All progress reset");
    }

    // Reset specific player
    public void ResetPlayer(int playerNumber)
    {
        if (playersData.ContainsKey(playerNumber))
        {
            playersData[playerNumber] = new PlayerData();
        }
        Debug.Log($"PlayerProgress: Player {playerNumber} reset");
    }
}
