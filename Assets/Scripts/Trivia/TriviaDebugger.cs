using UnityEngine;

// Add this script to any GameObject in your scene to debug trivia system
public class TriviaDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== TRIVIA SYSTEM DEBUG ===");
        
        // Check QuestionManager
        if (QuestionManager.Instance == null)
        {
            Debug.LogError("❌ QuestionManager.Instance is NULL! You need to:");
            Debug.LogError("   1. Create GameObject named 'QuestionManager'");
            Debug.LogError("   2. Add QuestionManager script");
            Debug.LogError("   3. Assign CSV file in Inspector");
        }
        else
        {
            Debug.Log("✅ QuestionManager found!");
            var categories = QuestionManager.Instance.GetAllCategories();
            Debug.Log($"   Categories loaded: {categories.Count}");
            foreach (var cat in categories)
            {
                int count = QuestionManager.Instance.GetQuestionCount(cat);
                Debug.Log($"   - {cat}: {count} questions");
            }
        }
        
        // Check TriviaPopup
        if (TriviaPopup.Instance == null)
        {
            Debug.LogError("❌ TriviaPopup.Instance is NULL! You need to:");
            Debug.LogError("   1. Run Tools → Create Trivia Popup UI");
            Debug.LogError("   2. OR manually create popup with TriviaPopup script");
        }
        else
        {
            Debug.Log("✅ TriviaPopup found!");
            if (TriviaPopup.Instance.popupPanel == null)
            {
                Debug.LogWarning("⚠️ TriviaPopup panel not assigned in Inspector!");
            }
        }
        
        // Check PlayerProgress
        if (PlayerProgress.Instance == null)
        {
            Debug.LogWarning("⚠️ PlayerProgress.Instance is NULL (optional for basic testing)");
        }
        else
        {
            Debug.Log("✅ PlayerProgress found!");
        }
        
        Debug.Log("=== END DEBUG ===");
    }
    
    void Update()
    {
        // Press T to test popup
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Testing popup with Science category for Player 1...");
            if (TriviaPopup.Instance != null)
            {
                TriviaPopup.Instance.ShowQuestion("Science", 1, (correct) => 
                {
                    Debug.Log($"Test answer was: {(correct ? "CORRECT" : "WRONG")}");
                });
            }
            else
            {
                Debug.LogError("Can't test - TriviaPopup.Instance is null!");
            }
        }
    }
}
