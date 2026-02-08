using UnityEngine;

[System.Serializable]
public class PlayerConfiguration
{
    public int PlayerID; // 0, 1, 2...
    public string PlayerName;
    public Sprite CharacterSprite; // Kept for legacy/fallback
    
    // New Customization
    public int ModelIndex;
    public int ColorIndex;
}
