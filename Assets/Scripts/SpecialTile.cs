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
        General,
        Random
    }

    public TileEffect effect = TileEffect.ExtraRoll;
    public System.Collections.Generic.List<Transform> possibleDestinations;
    
    [Header("Question Tile Settings")]
    public QuestionCategory questionCategory = QuestionCategory.General;
    
    // Get category as string for QuestionManager
    public string GetCategoryString()
    {
        if (questionCategory == QuestionCategory.Random)
        {
            QuestionCategory[] categories = (QuestionCategory[])System.Enum.GetValues(typeof(QuestionCategory));
            int randomIndex = UnityEngine.Random.Range(0, categories.Length - 1);
            return categories[randomIndex].ToString();
        }
        return questionCategory.ToString();
    }
}
