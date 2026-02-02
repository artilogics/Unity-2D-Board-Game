using UnityEngine;
using UnityEngine.UI;

public class TurnIndicatorUI : MonoBehaviour
{
    [Header("UI References")]
    public Image playerPortrait;
    public Text playerNameText;
    
    // Optional: Animation or extra flair
    
    public void UpdateTurn(string playerName, Sprite playerSprite)
    {
        if (playerNameText) playerNameText.text = $"{playerName}'s Turn";
        
        if (playerPortrait)
        {
            if (playerSprite != null)
            {
                playerPortrait.sprite = playerSprite;
                playerPortrait.enabled = true;
            }
            else
            {
                playerPortrait.enabled = false;
            }
        }
    }
}
