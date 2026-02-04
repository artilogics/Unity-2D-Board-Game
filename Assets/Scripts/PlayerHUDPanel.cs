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
    // You might want 2 rows of 4
    
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
    
    public void UpdateSockets(System.Collections.Generic.HashSet<string> completedCategories)
    {
        // Loop through sockets and color them if category collected
        // Needs mapping logic. For now, just fill count?
        // User said "sockets change to the color of the category"
        // We need a shared definition of Category -> Index/Color
        // Let's assume TurnIndicatorUI passes the color or we fill sequentially
        
        // Placeholder: Fill N sockets with generic color or loop
        int count = 0;
        foreach (var cat in completedCategories)
        {
            if (count < sockets.Length)
            {
                 sockets[count].color = Color.cyan; // Placeholder color
                 // Ideally: sockets[count].color = CategoryColors[cat];
                 count++;
            }
        }
        
        // Reset remaining
        while (count < sockets.Length)
        {
            sockets[count].color = Color.gray; // Empty
            count++;
        }
    }
}
