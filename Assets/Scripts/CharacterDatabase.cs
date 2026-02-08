using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Board Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [System.Serializable]
    public class CharacterEntry
    {
        public string characterName;
        public GameObject prefab3D; // The 3D Model Prefab
        
        [Tooltip("Index 0 = Neutral/Grey (UI). Index 1..N = Colored variants matching ColorDefinitions.")]
        public Sprite[] sprites; 
    }

    [System.Serializable]
    public class ColorDefinition
    {
        public string colorName;
        public Color uiColor;       // For UI buttons
        public Material material3D; // For 3D Model
    }

    public List<CharacterEntry> characters;
    public List<ColorDefinition> availableColors;
}
