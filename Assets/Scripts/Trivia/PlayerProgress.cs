using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance { get; private set; }

    [Header("Player 1")]
    public int player1Points = 0;
    public HashSet<string> player1CompletedCategories = new HashSet<string>();

    [Header("Player 2")]
    public int player2Points = 0;
    public HashSet<string> player2CompletedCategories = new HashSet<string>();

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

    // Add points for a player
    public void AddPoints(int playerNumber, int points)
    {
        if (playerNumber == 1)
        {
            player1Points += points;
            player1Points = Mathf.Max(0, player1Points); // Don't go negative
            Debug.Log($"Player 1 points: {player1Points}");
        }
        else if (playerNumber == 2)
        {
            player2Points += points;
            player2Points = Mathf.Max(0, player2Points);
            Debug.Log($"Player 2 points: {player2Points}");
        }
    }

    // Mark a category as completed for a player
    public void CompleteCategory(int playerNumber, string category)
    {
        if (playerNumber == 1)
        {
            player1CompletedCategories.Add(category);
            Debug.Log($"Player 1 completed category: {category} ({player1CompletedCategories.Count}/{categoriesToWin})");
        }
        else if (playerNumber == 2)
        {
            player2CompletedCategories.Add(category);
            Debug.Log($"Player 2 completed category: {category} ({player2CompletedCategories.Count}/{categoriesToWin})");
        }
    }

    // Check if player has completed a category
    public bool HasCompletedCategory(int playerNumber, string category)
    {
        if (playerNumber == 1)
        {
            return player1CompletedCategories.Contains(category);
        }
        else if (playerNumber == 2)
        {
            return player2CompletedCategories.Contains(category);
        }
        return false;
    }

    // Check if player is ready for final round
    public bool IsReadyForFinal(int playerNumber)
    {
        if (playerNumber == 1)
        {
            return player1CompletedCategories.Count >= categoriesToWin;
        }
        else if (playerNumber == 2)
        {
            return player2CompletedCategories.Count >= categoriesToWin;
        }
        return false;
    }

    // Get current points for a player
    public int GetPoints(int playerNumber)
    {
        return playerNumber == 1 ? player1Points : player2Points;
    }

    // Get completed category count
    public int GetCompletedCategoryCount(int playerNumber)
    {
        return playerNumber == 1 ? player1CompletedCategories.Count : player2CompletedCategories.Count;
    }

    // Reset all progress
    public void ResetProgress()
    {
        player1Points = 0;
        player2Points = 0;
        player1CompletedCategories.Clear();
        player2CompletedCategories.Clear();
        Debug.Log("PlayerProgress: All progress reset");
    }

    // Reset specific player
    public void ResetPlayer(int playerNumber)
    {
        if (playerNumber == 1)
        {
            player1Points = 0;
            player1CompletedCategories.Clear();
        }
        else if (playerNumber == 2)
        {
            player2Points = 0;
            player2CompletedCategories.Clear();
        }
        Debug.Log($"PlayerProgress: Player {playerNumber} reset");
    }
}
