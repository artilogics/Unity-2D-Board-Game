using UnityEngine;
using UnityEngine.UI;

public class TurnIndicatorUI : MonoBehaviour
{
    [Header("Player Panels")]
    public System.Collections.Generic.List<PlayerHUDPanel> playerPanels; // Assign 5 panels in Inspector
    
    [Header("Shared")]
    public Text turnStatusText;
    
    [System.Serializable]
    public struct CategoryMapping
    {
        public SpecialTile.QuestionCategory category;
        public int socketIndex;
        public Color activeColor;
    }

    [Header("Category Configuration")]
    public System.Collections.Generic.List<CategoryMapping> categoryMappings;

    [Header("Category Colors")]
    public Color defaultSocketColor = Color.gray;
    // Simple mapping for now
    
    public void UpdateHUD(int activePlayerIndex, System.Collections.Generic.List<GameObject> players)
    {
         if (PlayerProgress.Instance == null) { Debug.LogError("TurnIndicatorUI: No PlayerProgress!"); return; }
         // Debug.Log($"TurnIndicatorUI: Updating for {players.Count} players. Active: {activePlayerIndex}");
         
         // Loop through all available panels
         for (int i = 0; i < playerPanels.Count; i++)
         {
             if (i < players.Count)
             {
                 // Active Player Logic
                 playerPanels[i].gameObject.SetActive(true);
                 
                 GameObject p = players[i];
                 string pName = p.name;
                 
                 Sprite pSprite = null;
                 SpriteRenderer sr = p.GetComponentInChildren<SpriteRenderer>();
                 if (sr) pSprite = sr.sprite;
                 
                 // 1-based index for PlayerProgress
                 int pNum = i + 1;
                 int score = PlayerProgress.Instance.GetPoints(pNum);
                 
                 // Get completed categories
                 var cats = PlayerProgress.Instance.GetCompletedCategories(pNum);
                 
                 // Debug.Log($"TurnIndicatorUI: Player {pNum} ({pName}) has {cats.Count} categories complete.");
                 
                 playerPanels[i].SetInfo(pName, pSprite, score);
                 playerPanels[i].SetActive(i == activePlayerIndex);
                 
                 // Pass mappings
                 if (categoryMappings != null && categoryMappings.Count > 0)
                 {
                     playerPanels[i].UpdateSockets(cats, categoryMappings);
                 }
                 else
                 {
                     Debug.LogWarning("TurnIndicatorUI: No Category Mappings found!");
                 }
             }
             else
             {
                 // Disable unused panels
                 playerPanels[i].gameObject.SetActive(false);
             }
         }
         
         if (turnStatusText && activePlayerIndex < players.Count)
         {
             turnStatusText.text = $"{players[activePlayerIndex].name}'s Turn";
         }
    }
    
    // Deprecated
    public void UpdateTurn(string n, Sprite s) {}
    public void UpdateHUD(int a, string n1, Sprite s1, string n2, Sprite s2) {} // Legacy override
    

}
