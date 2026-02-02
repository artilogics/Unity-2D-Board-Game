using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetupUICreator : EditorWindow
{
    [MenuItem("Tools/Board Game/Create Setup UI")]
    public static void ShowWindow()
    {
        GetWindow<SetupUICreator>("UI Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Setup UI Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Setup UI"))
        {
            CreateUI();
        }
    }

    private void CreateUI()
    {
        // 1. Find or Create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Create Main Setup Panel
        GameObject setupPanel = CreatePanel("SetupPanel", canvas.transform);
        setupPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.95f); // Dark Blue background

        // 3. Create Player Count Step
        GameObject countStep = CreatePanel("Step_PlayerCount", setupPanel.transform);
        CreateText("Title", "Select Number of Players", countStep.transform, new Vector2(0, 100), 40);
        
        GameObject d2 = CreateButton("Btn_2Players", "2 Players", countStep.transform, new Vector2(-150, 0));
        GameObject d3 = CreateButton("Btn_3Players", "3 Players", countStep.transform, new Vector2(150, 0));

        // 4. Create Player Config Step
        GameObject configStep = CreatePanel("Step_PlayerConfig", setupPanel.transform);
        configStep.SetActive(false);
        
        Text configTitle = CreateText("Title", "Setup Player 1", configStep.transform, new Vector2(0, 150), 36).GetComponent<Text>();
        
        // Name Input
        GameObject inputObj = CreateInputField("NameInput", configStep.transform, new Vector2(0, 80));
        InputField nameInput = inputObj.GetComponent<InputField>();

        // Character Grid
        GameObject charGrid = new GameObject("CharacterGrid");
        charGrid.transform.SetParent(configStep.transform, false);
        RectTransform gridRect = charGrid.AddComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(500, 200);
        gridRect.anchoredPosition = new Vector2(0, -50);
        GridLayoutGroup grid = charGrid.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // Preview Image
        GameObject previewObj = new GameObject("CharPreview");
        previewObj.transform.SetParent(configStep.transform, false);
        Image previewImg = previewObj.AddComponent<Image>();
        previewObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);
        previewObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

        // Feedback Text
        GameObject feedbackObj = CreateText("FeedbackText", "", configStep.transform, new Vector2(0, -130), 20);
        Text feedbackText = feedbackObj.GetComponent<Text>();
        feedbackText.color = Color.red;

        // Navigation Buttons
        GameObject nextBtn = CreateButton("Btn_Next", "Next Player", configStep.transform, new Vector2(200, -200));
        GameObject startBtn = CreateButton("Btn_Start", "Start Game", configStep.transform, new Vector2(200, -200));
        startBtn.SetActive(false);

        // 5. Try to Link to Manager
        GameSetupManager manager = FindFirstObjectByType<GameSetupManager>();
        if (manager == null)
        {
            GameObject manObj = new GameObject("GameSetupManager");
            manager = manObj.AddComponent<GameSetupManager>();
        }

        Undo.RecordObject(manager, "Link Setup UI");
        manager.setupPanel = setupPanel;
        // Assume Game Panel exists or user assigns it, or create a placeholder
        // manager.gamePanel = ... 
        
        manager.playerCountStep = countStep;
        manager.playerConfigStep = configStep;
        
        // Link Actions - Count Step
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(d2.GetComponent<Button>().onClick, manager.OnPlayerCountSelected, 2);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(d3.GetComponent<Button>().onClick, manager.OnPlayerCountSelected, 3);

        // Link Actions - Config Step
        manager.configTitle = configTitle;
        manager.nameInput = nameInput;
        manager.characterGrid = charGrid.transform;
        manager.startGameButton = startBtn.GetComponent<Button>();
        manager.nextButton = nextBtn.GetComponent<Button>();
        manager.selectedCharPreview = previewImg;
        manager.feedbackText = feedbackText; // Link feedback text
        
        // Link Actions - Nav Buttons
        // Note: UnityEvents in Editor don't persist perfectly for runtime if target is scene object, but it usually works.
        // Better to set them up via script connection or cleaner serialized events.
        // For this generated tool, we try our best.
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, manager.OnNextPlayerButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(startBtn.GetComponent<Button>().onClick, manager.OnStartGameButton);
        
        // 6. Try to populate characters
        Sprite[] sprites = LoadSprites();
        if (sprites != null && sprites.Length > 0)
        {
            manager.availableCharacters = sprites;
            // Create buttons for them in grid
            for(int i=0; i<sprites.Length; i++)
            {
                CreateCharSelectButton(i, sprites[i], charGrid.transform, manager);
            }
        }
        else
        {
            Debug.LogWarning("Could not auto-load sprites. Please assign them manually.");
        }
        
        // 7. Create Turn Indicator Panel (New Request)
        CreateTurnIndicatorPanel(canvas.transform);

        Debug.Log("UI Generated Successfully!");
    }
    
    // Helper to find sprites
    private Sprite[] LoadSprites()
    {
        // Try to load 'characters' from Resources if possible? 
        // Or AssetDatabase
        string path = "Assets/Sprites/characters.png";
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> sprites = new List<Sprite>();
        foreach(Object o in objs)
        {
            if (o is Sprite s) sprites.Add(s);
        }
        return sprites.ToArray();
    }

    private void CreateCharSelectButton(int index, Sprite sprite, Transform parent, GameSetupManager manager)
    {
        GameObject btnObj = new GameObject("CharBtn_" + index);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        
        Button btn = btnObj.AddComponent<Button>();
        
        // Add Listener
        // We need a persistent listener for an int argument, which is tricky in Editor scripts sometimes.
        // We'll wrap it in a simple helper script if needed, or rely on the user to check checks.
        // Actually, let's create a lambda wrapper component? No, that's messy.
        // Let's attach a small script to the button that talks to the manager.
        CharSelectButton csb = btnObj.AddComponent<CharSelectButton>();
        csb.index = index;
        csb.manager = manager;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, csb.OnClick);
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        // Transparent BG
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0,0,0,0);
        return panel;
    }

    private GameObject CreateButton(string name, string text, Transform parent, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        
        Button btn = btnObj.AddComponent<Button>();
        
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 50);
        rt.anchoredPosition = pos;

        GameObject textObj = CreateText("Text", text, btnObj.transform, Vector2.zero, 24);
        textObj.GetComponent<Text>().color = Color.black;
        
        return btnObj;
    }

    private GameObject CreateText(string name, string content, Transform parent, Vector2 pos, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Default Arial usually
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = fontSize;
        txt.color = Color.white; // Default for dark bg
        
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
        rt.anchoredPosition = pos;
        
        return textObj;
    }

    private GameObject CreateInputField(string name, Transform parent, Vector2 pos)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        Image bg = root.AddComponent<Image>();
        bg.color = Color.white;
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 50);
        rt.anchoredPosition = pos;

        InputField input = root.AddComponent<InputField>();
        
        // Placeholder
        GameObject placeholder = CreateText("Placeholder", "Enter Name...", root.transform, Vector2.zero, 24);
        placeholder.GetComponent<Text>().color = Color.gray;
        
        // Text
        GameObject text = CreateText("Text", "", root.transform, Vector2.zero, 24);
        text.GetComponent<Text>().color = Color.black;
        
        input.targetGraphic = bg;
        input.placeholder = placeholder.GetComponent<Text>();
        input.textComponent = text.GetComponent<Text>();
        
        return root;
    }

    private void CreateTurnIndicatorPanel(Transform parent)
    {
        // Setup a movable panel
        GameObject turnPanel = new GameObject("TurnIndicatorPanel");
        turnPanel.transform.SetParent(parent, false);
        
        // Background (optional)
        Image bg = turnPanel.AddComponent<Image>();
        bg.color = new Color(0,0,0, 0.5f);
        
        RectTransform rt = turnPanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 80);
        rt.anchoredPosition = new Vector2(0, 200); // Near dice?
        
        // Content
        GameObject faceObj = new GameObject("Portrait");
        faceObj.transform.SetParent(turnPanel.transform, false);
        Image faceImg = faceObj.AddComponent<Image>();
        RectTransform faceRT = faceObj.GetComponent<RectTransform>();
        faceRT.sizeDelta = new Vector2(60, 60);
        faceRT.anchoredPosition = new Vector2(-80, 0);
        
        GameObject textObj = CreateText("NameText", "Player 1's Turn", turnPanel.transform, new Vector2(40, 0), 20);
        Text nameText = textObj.GetComponent<Text>();
        nameText.alignment = TextAnchor.MiddleLeft;
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 60);
        
        // Script
        TurnIndicatorUI ti = turnPanel.AddComponent<TurnIndicatorUI>();
        ti.playerPortrait = faceImg;
        ti.playerNameText = nameText;
        
        // Link to GameControl
        GameControl gc = FindFirstObjectByType<GameControl>();
        if (gc)
        {
             Undo.RecordObject(gc, "Link Turn UI");
             gc.turnIndicator = ti;
        }
        else
        {
            Debug.LogWarning("Could not find GameControl to link Turn Indicator!");
        }
        
        // Hide by default (GameControl will enable it)
        turnPanel.SetActive(false);
    }
}
