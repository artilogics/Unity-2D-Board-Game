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
    public CharacterDatabase charDB; // Assign in Inspector
    public Transform colorGrid; // Parent of color buttons

    // Internal State
    private int totalPlayers = 2;
    private int currentPlayerIndex = 0;
    private List<PlayerConfiguration> players = new List<PlayerConfiguration>();
    
    private int selectedModelIndex = -1;
    private int selectedColorIndex = -1;

    void Start()
    {
        // Initial State
        if (setupPanel) setupPanel.SetActive(true);
        if (gamePanel) gamePanel.SetActive(false);
        
        // Try to load DB if not assigned (auto-find)
        if (charDB == null) charDB = Resources.Load<CharacterDatabase>("CharacterDatabase");

        ShowPlayerCountSelection();
    }

    public void OnPlayerCountSelected(int count)
    {
        totalPlayers = Mathf.Clamp(count, 2, 4); // Enforce GDD limit (2-4 players)
        // totalPlayers = count; 
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
        
        selectedModelIndex = -1;
        selectedColorIndex = -1;
        
        if (selectedCharPreview) selectedCharPreview.sprite = null;
        if (selectedCharPreview) selectedCharPreview.color = Color.clear; // Hide until selected
        
        if (feedbackText) feedbackText.text = ""; // Clear feedback
        
        // Reset Grids
        RefreshCharacterGrid();
        if (colorGrid) colorGrid.gameObject.SetActive(false); // Hide colors until model picked
        
        // Update Buttons
        bool isLast = (currentPlayerIndex == totalPlayers - 1);
        if (nextButton) nextButton.gameObject.SetActive(false); // Hide until valid
        if (startGameButton) startGameButton.gameObject.SetActive(false); // Hide until valid
    }

    private void RefreshCharacterGrid()
    {
        if (characterGrid == null || charDB == null) return;

        // 1. Reset all buttons to Interactable
        for(int i=0; i < characterGrid.childCount; i++)
        {
             Button b = characterGrid.GetChild(i).GetComponent<Button>();
             if (b) b.interactable = true;
        }
        
        // 2. Disable buttons for Models already chosen by PREVIOUS players
        foreach(var p in players)
        {
            int usedIndex = p.ModelIndex;
            if (usedIndex >= 0 && usedIndex < characterGrid.childCount)
            {
                 Button b = characterGrid.GetChild(usedIndex).GetComponent<Button>();
                 if (b) b.interactable = false;
            }
        }
    }

    public void OnCharacterSelected(int modelIndex)
    {
        if (charDB == null || modelIndex < 0 || modelIndex >= charDB.characters.Count) return;

        selectedModelIndex = modelIndex;
        selectedColorIndex = -1; // Reset color
        
        // Show Color Grid
        if (colorGrid)
        {
            colorGrid.gameObject.SetActive(true);
            RefreshColorGrid();
        }

        // Preview (Neutral)
        UpdatePreview();
        ValidateCurrentConfig();
    }

    public void OnColorSelected(int colorIndex)
    {
        selectedColorIndex = colorIndex;
        UpdatePreview();
        ValidateCurrentConfig(); // Check if we can proceed
    }

    private void RefreshColorGrid()
    {
        if (colorGrid == null) return;
        
        // 1. Reset all
        for(int i=0; i < colorGrid.childCount; i++)
        {
             Button b = colorGrid.GetChild(i).GetComponent<Button>();
             if (b) b.interactable = true;
        }

        // 2. Disable Colors already chosen by PREVIOUS players
        foreach(var p in players)
        {
            int usedColor = p.ColorIndex;
            if (usedColor >= 0 && usedColor < colorGrid.childCount)
            {
                 Button b = colorGrid.GetChild(usedColor).GetComponent<Button>();
                 if (b) b.interactable = false;
            }
        }
    }

    private void UpdatePreview()
    {
        if (selectedCharPreview == null || charDB == null) return;
        
        if (selectedModelIndex >= 0)
        {
            var character = charDB.characters[selectedModelIndex];
            selectedCharPreview.color = Color.white;
            
            // Determine Sprite
            // Index 0 = Neutral
            // Index 1 = Color 0, Index 2 = Color 1...
            int spriteIndex = 0;
            if (selectedColorIndex >= 0) spriteIndex = selectedColorIndex + 1;
            
            if (character.sprites != null && spriteIndex < character.sprites.Length)
            {
                selectedCharPreview.sprite = character.sprites[spriteIndex];
            }
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
        bool nameOk = !string.IsNullOrEmpty(nameInput.text);
        bool modelOk = selectedModelIndex >= 0;
        bool colorOk = selectedColorIndex >= 0;

        // Update Button Visibility based on validity
        bool isValid = nameOk && modelOk && colorOk;
        
        bool isLast = (currentPlayerIndex == totalPlayers - 1);
        if (nextButton) nextButton.gameObject.SetActive(isValid && !isLast);
        if (startGameButton) startGameButton.gameObject.SetActive(isValid && isLast);

        if (!isValid)
        {
            if (!modelOk) if (feedbackText) feedbackText.text = "Select a Model!";
            else if (!colorOk) if (feedbackText) feedbackText.text = "Select a Color!";
            else if (!nameOk) if (feedbackText) feedbackText.text = "Enter a Name!";
        }
        else
        {
            if (feedbackText) feedbackText.text = "";
        }

        return isValid;
    }

    private void SaveCurrentConfig()
    {
        PlayerConfiguration pc = new PlayerConfiguration();
        pc.PlayerID = currentPlayerIndex;
        pc.PlayerName = nameInput.text;
        
        // Fallback sprite for legacy code/HUD
        if (charDB != null && selectedModelIndex >= 0)
        {
             var c = charDB.characters[selectedModelIndex];
             int sIdx = selectedColorIndex + 1;
             if (sIdx < c.sprites.Length) pc.CharacterSprite = c.sprites[sIdx];
        }

        pc.ModelIndex = selectedModelIndex;
        pc.ColorIndex = selectedColorIndex;
        
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
