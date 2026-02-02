using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameSetupManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject setupPanel; // The entire setup UI
    public GameObject gamePanel;  // The main game UI (to be enabled later)
    
    [Header("Setup Steps")]
    public GameObject playerCountStep;
    public GameObject playerConfigStep;
    
    [Header("Config UI Elements")]
    public Text configTitle; // "Setup Player 1"
    public InputField nameInput;
    public Transform characterGrid; // Parent of character buttons
    public Button startGameButton; // Only on last step
    public Button nextButton;
    public Image selectedCharPreview;
    public Text feedbackText;

    [Header("Resources")]
    public Sprite[] availableCharacters; // Assign in Inspector

    // Internal State
    private int totalPlayers = 2;
    private int currentPlayerIndex = 0;
    private List<PlayerConfiguration> players = new List<PlayerConfiguration>();
    
    private Sprite currentSelectedSprite;

    void Start()
    {
        // Initial State
        if (setupPanel) setupPanel.SetActive(true);
        if (gamePanel) gamePanel.SetActive(false);
        
        ShowPlayerCountSelection();
    }

    public void OnPlayerCountSelected(int count)
    {
        totalPlayers = count;
        players.Clear();
        currentPlayerIndex = 0;
        
        // Move to Config Step
        ShowPlayerConfigStep();
    }

    private void ShowPlayerCountSelection()
    {
        if (playerCountStep) playerCountStep.SetActive(true);
        if (playerConfigStep) playerConfigStep.SetActive(false);
    }

    private void ShowPlayerConfigStep()
    {
        if (playerCountStep) playerCountStep.SetActive(false);
        if (playerConfigStep) playerConfigStep.SetActive(true);

        // Reset UI for current player
        configTitle.text = $"Setup Player {currentPlayerIndex + 1}";
        nameInput.text = "";
        currentSelectedSprite = null;
        if (selectedCharPreview) selectedCharPreview.sprite = null;
        if (feedbackText) feedbackText.text = ""; // Clear feedback
        
        // Update Buttons
        bool isLast = (currentPlayerIndex == totalPlayers - 1);
        if (nextButton) nextButton.gameObject.SetActive(!isLast);
        if (startGameButton) startGameButton.gameObject.SetActive(isLast);
        
        RefreshCharacterGrid();
    }

    private void RefreshCharacterGrid()
    {
        if (characterGrid == null) return;

        // 1. Reset all buttons to Interactable
        foreach(Transform t in characterGrid)
        {
             Button b = t.GetComponent<Button>();
             if (b) b.interactable = true;
        }
        
        // 2. Disable buttons for characters already chosen
        if (availableCharacters != null)
        {
            foreach(var p in players)
            {
                Sprite used = p.CharacterSprite;
                int idx = System.Array.IndexOf(availableCharacters, used);
                
                // Assuming child index corresponds to array index (as per generator)
                if (idx >= 0 && idx < characterGrid.childCount)
                {
                     Button b = characterGrid.GetChild(idx).GetComponent<Button>();
                     if (b) b.interactable = false;
                }
            }
        }
    }

    // Called by UI Buttons
    public void OnCharacterSelected(int spriteIndex)
    {
        if (spriteIndex >= 0 && spriteIndex < availableCharacters.Length)
        {
            currentSelectedSprite = availableCharacters[spriteIndex];
            if (selectedCharPreview) selectedCharPreview.sprite = currentSelectedSprite;
        }
    }

    public void OnNextPlayerButton()
    {
        if (ValidateCurrentConfig())
        {
            SaveCurrentConfig();
            currentPlayerIndex++;
            ShowPlayerConfigStep();
        }
    }

    public void OnStartGameButton()
    {
        if (ValidateCurrentConfig())
        {
            SaveCurrentConfig();
            FinishSetup();
        }
    }

    private bool ValidateCurrentConfig()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.Log("Enter a name!");
            if (feedbackText) feedbackText.text = "Please enter a name!";
            return false;
        }
        if (currentSelectedSprite == null)
        {
            Debug.Log("Select a character!");
            if (feedbackText) feedbackText.text = "Please select a character!";
            return false;
        }
        return true;
    }

    private void SaveCurrentConfig()
    {
        PlayerConfiguration pc = new PlayerConfiguration();
        pc.PlayerID = currentPlayerIndex;
        pc.PlayerName = nameInput.text;
        pc.CharacterSprite = currentSelectedSprite;
        players.Add(pc);
    }

    private void FinishSetup()
    {
        // 1. Hide Setup
        if (setupPanel) setupPanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(true);

        // 2. Initialize GameControl
        GameControl.Instance.InitializeGame(players);
    }
}
