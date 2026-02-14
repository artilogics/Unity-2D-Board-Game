using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TriviaPopup : MonoBehaviour
{
    public static TriviaPopup Instance { get; private set; }

    [Header("UI References")]
    public GameObject popupPanel;
    public Text questionText;
    public Button optionA_Button;
    public Button optionB_Button;
    public Button optionC_Button;
    public Button optionD_Button;
    public Button confirmButton;
    public Text feedbackText;

    [Header("Optional: Category Display")]
    public Text categoryText;

    [Header("Optional: Points Display")]
    public Text player1PointsText;
    public Text player2PointsText;

    [Header("Optional: Confetti Effect")]
    public ParticleSystem confettiEffect;

    private TriviaQuestion currentQuestion;
    private string selectedAnswer = "";
    private int currentPlayer = 1;
    private System.Action<bool> onAnswerCallback;

    void Awake()
    {
        Debug.Log("TriviaPopup: Awake() called");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("TriviaPopup: Instance set successfully");
        }
        else
        {
            Debug.LogWarning("TriviaPopup: Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }

        // Hide popup on start
        if (popupPanel != null) popupPanel.SetActive(false);

        // Setup button listeners
        if (optionA_Button != null) optionA_Button.onClick.AddListener(() => SelectAnswer("A"));
        if (optionB_Button != null) optionB_Button.onClick.AddListener(() => SelectAnswer("B"));
        if (optionC_Button != null) optionC_Button.onClick.AddListener(() => SelectAnswer("C"));
        if (optionD_Button != null) optionD_Button.onClick.AddListener(() => SelectAnswer("D"));
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmAnswer);
    }

    public void ShowQuestion(string category, int playerNumber, System.Action<bool> callback)
    {
        currentPlayer = playerNumber;
        onAnswerCallback = callback;

        // Get question from manager
        if (QuestionManager.Instance == null)
        {
            Debug.LogError("TriviaPopup: QuestionManager not found!");
            return;
        }

        currentQuestion = QuestionManager.Instance.GetQuestion(category);
        if (currentQuestion == null)
        {
            Debug.LogWarning($"TriviaPopup: No question available for category {category}");
            return;
        }

        // Display question
        if (questionText != null) questionText.text = currentQuestion.question;
        if (categoryText != null) categoryText.text = $"Category: {currentQuestion.category}";

        // Set option texts
        if (optionA_Button != null) optionA_Button.GetComponentInChildren<Text>().text = $"A) {currentQuestion.optionA}";
        if (optionB_Button != null) optionB_Button.GetComponentInChildren<Text>().text = $"B) {currentQuestion.optionB}";
        if (optionC_Button != null) optionC_Button.GetComponentInChildren<Text>().text = $"C) {currentQuestion.optionC}";
        if (optionD_Button != null) optionD_Button.GetComponentInChildren<Text>().text = $"D) {currentQuestion.optionD}";

        // Reset selection
        selectedAnswer = "";
        ResetButtonColors();
        if (confirmButton != null) confirmButton.interactable = false;
        if (feedbackText != null) feedbackText.text = "";

        // Update points display
        UpdatePointsDisplay();

        // Show popup
        if (popupPanel != null) popupPanel.SetActive(true);
    }

    void SelectAnswer(string answer)
    {
        selectedAnswer = answer;
        HighlightSelectedButton(answer);
        if (confirmButton != null) confirmButton.interactable = true;
    }

    void ConfirmAnswer()
    {
        if (string.IsNullOrEmpty(selectedAnswer)) return;

        bool isCorrect = selectedAnswer == currentQuestion.correctAnswer;

        // Mark question as answered
        if (QuestionManager.Instance != null)
        {
            QuestionManager.Instance.MarkAnswered(currentQuestion.id, isCorrect);
        }

        // Award/subtract points
        if (PlayerProgress.Instance != null)
        {
            int points = isCorrect ? PlayerProgress.Instance.pointsForCorrect : PlayerProgress.Instance.pointsForWrong;
            PlayerProgress.Instance.AddPoints(currentPlayer, points);

            // Complete category if correct
            if (isCorrect && !PlayerProgress.Instance.HasCompletedCategory(currentPlayer, currentQuestion.category))
            {
                PlayerProgress.Instance.CompleteCategory(currentPlayer, currentQuestion.category);
            }
            
            // Update points display immediately after changes
            UpdatePointsDisplay();
        }

        // Show feedback
        ShowFeedback(isCorrect);

        // Close after delay and callback
        StartCoroutine(CloseAfterDelay(isCorrect));
    }

    void ShowFeedback(bool correct)
    {
        Debug.Log($"ShowFeedback called: {(correct ? "CORRECT" : "WRONG")}");
        
        if (feedbackText != null)
        {
            if (correct)
            {
                feedbackText.text = "✓ CORRECT!";
                feedbackText.color = Color.green;
                
                // Audio Feedback
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrectAnswer();

                // Trigger confetti!
                if (confettiEffect != null)
                {
                    confettiEffect.Play();
                    Debug.Log("Confetti played!");
                }
            }
            else
            {
                feedbackText.text = $"✗ WRONG! Correct answer was {currentQuestion.correctAnswer}";
                feedbackText.color = Color.red;

                // Audio Feedback
                if (AudioManager.Instance != null) AudioManager.Instance.PlayWrongAnswer();
            }
        }
        else
        {
            Debug.LogWarning("FeedbackText is null!");
        }

        // Disable all buttons
        if (optionA_Button != null) optionA_Button.interactable = false;
        if (optionB_Button != null) optionB_Button.interactable = false;
        if (optionC_Button != null) optionC_Button.interactable = false;
        if (optionD_Button != null) optionD_Button.interactable = false;
        if (confirmButton != null) confirmButton.interactable = false;
        
        Debug.Log("All buttons disabled");
    }

    IEnumerator CloseAfterDelay(bool wasCorrect)
    {
        Debug.Log("CloseAfterDelay: Waiting 2.5 seconds...");
        yield return new WaitForSeconds(2.5f);
        Debug.Log("CloseAfterDelay: Closing popup now");
        ClosePopup();
        Debug.Log($"CloseAfterDelay: Invoking callback with {wasCorrect}");
        onAnswerCallback?.Invoke(wasCorrect);
    }

    void ClosePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        // Re-enable buttons
        if (optionA_Button != null) optionA_Button.interactable = true;
        if (optionB_Button != null) optionB_Button.interactable = true;
        if (optionC_Button != null) optionC_Button.interactable = true;
        if (optionD_Button != null) optionD_Button.interactable = true;
    }

    void HighlightSelectedButton(string answer)
    {
        ResetButtonColors();

        Button selectedButton = answer switch
        {
            "A" => optionA_Button,
            "B" => optionB_Button,
            "C" => optionC_Button,
            "D" => optionD_Button,
            _ => null
        };

        if (selectedButton != null)
        {
            // Change the Image color directly for immediate visual feedback
            Image btnImage = selectedButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = new Color(1f, 0.92f, 0.016f); // Bright yellow
            }
        }
    }

    void ResetButtonColors()
    {
        Button[] buttons = { optionA_Button, optionB_Button, optionC_Button, optionD_Button };
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                // Reset Image color to white
                Image btnImage = btn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = Color.white;
                }
            }
        }
    }

    void UpdatePointsDisplay()
    {
        if (PlayerProgress.Instance == null) return;

        if (player1PointsText != null)
        {
            int p1Points = PlayerProgress.Instance.GetPoints(1);
            int p1Categories = PlayerProgress.Instance.GetCompletedCategoryCount(1);
            player1PointsText.text = $"P1: {p1Points} pts | {p1Categories}/8";
        }

        if (player2PointsText != null)
        {
            int p2Points = PlayerProgress.Instance.GetPoints(2);
            int p2Categories = PlayerProgress.Instance.GetCompletedCategoryCount(2);
            player2PointsText.text = $"P2: {p2Points} pts | {p2Categories}/8";
        }
    }
}
