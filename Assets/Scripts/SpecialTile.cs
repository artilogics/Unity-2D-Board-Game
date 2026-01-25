using UnityEngine;

public class SpecialTile : MonoBehaviour
{
    public enum TileEffect
    {
        None,
        ExtraRoll,
        Shortcut,
        SkipTurn,
        QuestionTile  // New for trivia system
    }

    public enum QuestionCategory
    {
        Science,
        History,
        Geography,
        Sports,
        Entertainment,
        Literature,
        Art,
        General
    }

    public TileEffect effect = TileEffect.ExtraRoll;
    public System.Collections.Generic.List<Transform> possibleDestinations;
    
    [Header("Question Tile Settings")]
    public QuestionCategory questionCategory = QuestionCategory.General;
    
    // Get category as string for QuestionManager
    public string GetCategoryString()
    {
        return questionCategory.ToString();
    }
}
