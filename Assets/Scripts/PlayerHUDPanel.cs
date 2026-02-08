using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDPanel : MonoBehaviour
{
    public Text nameText;
    public Text scoreText;
    public Image iconImage;
    public Image activeHighlight;
    
    [Header("Sockets")]
    public Image[] sockets; // 8 sockets (hypothetically)

    public void SetInfo(string name, Sprite icon, int score)
    {
        if (nameText) nameText.text = name;
        if (scoreText) scoreText.text = $"{score} Pts";
        if (iconImage)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }
    }
    
    public void SetActive(bool isActive)
    {
        if (activeHighlight) activeHighlight.color = isActive ? Color.green : Color.clear;
        // Or handle animation
    }

    public void UpdateSockets(System.Collections.Generic.HashSet<string> completedCategories, System.Collections.Generic.List<TurnIndicatorUI.CategoryMapping> mappings)
    {
        // 1. Reset all sockets to default (or gray)
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket) socket.color = Color.gray; // Default empty color
            }
        }

        // 2. Iterate through completed categories and color mapped sockets
        if (mappings != null && completedCategories != null)
        {
            foreach (var mapping in mappings)
            {
                // Check if this player has completed this category
                // Convert enum to string for HashSet comparison since Set stores strings
                string catName = mapping.category.ToString();
                
                Debug.Log($"HUD: Checking {catName}. Completed: {completedCategories.Contains(catName)}");

                if (completedCategories.Contains(catName))
                {
                     // Valid socket index?
                     if (sockets != null && mapping.socketIndex >= 0 && mapping.socketIndex < sockets.Length)
                     {
                         if (sockets[mapping.socketIndex])
                         {
                             sockets[mapping.socketIndex].color = mapping.activeColor;
                             // Debug.Log($"HUD Setting Socket {mapping.socketIndex} to {mapping.activeColor}");
                         }
                     }
                }
            }
        }
    }
}
