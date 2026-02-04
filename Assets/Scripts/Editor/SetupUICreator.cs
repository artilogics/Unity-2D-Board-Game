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
        // Setup a HUD Panel (Bottom Banner)
        GameObject hudPanel = new GameObject("HUD_Panel");
        hudPanel.transform.SetParent(parent, false);

        // Background
        Image bg = hudPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        RectTransform rt = hudPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 160); // Height 160 for sockets
        rt.anchoredPosition = new Vector2(0, 0); // Bottom

        // Layout Group for Even Distribution
        HorizontalLayoutGroup hlg = hudPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        // Script
        TurnIndicatorUI ti = hudPanel.AddComponent<TurnIndicatorUI>();
        ti.playerPanels = new List<PlayerHUDPanel>();

        // Link to GameControl
        GameControl gc = FindFirstObjectByType<GameControl>();
        if (gc)
        {
             Undo.RecordObject(gc, "Link Turn UI");
             gc.turnIndicator = ti;
        }

        // Loop to create 5 Player Panels
        for (int i = 0; i < 5; i++)
        {
            GameObject pPanel = new GameObject($"PlayerPanel_{i+1}");
            pPanel.transform.SetParent(hudPanel.transform, false);
            
            // Background & Script
            Image pBg = pPanel.AddComponent<Image>();
            pBg.color = new Color(1, 1, 1, 0.1f);
            
            PlayerHUDPanel php = pPanel.AddComponent<PlayerHUDPanel>();
            
            // Highlight
            GameObject hlObj = new GameObject("Highlight");
            hlObj.transform.SetParent(pPanel.transform, false);
            RectTransform hlRT = hlObj.AddComponent<RectTransform>();
            hlRT.anchorMin = Vector2.zero; hlRT.anchorMax = Vector2.one; 
            hlRT.offsetMin = Vector2.zero; hlRT.offsetMax = Vector2.zero;
            Image hlImg = hlObj.AddComponent<Image>();
            hlImg.color = new Color(0, 1, 0, 0); // Transparent by default
            php.activeHighlight = hlImg;

            // Content Container (Vertical Layout) inside Panel? 
            // Layout: Row 1 (Icon | Name | Score), Row 2 (Sockets)
            // Let's manually place for simplicity or use Layouts. Manual is risky with resizing.
            // Let's use Vertical Layout Group for the panel.
            VerticalLayoutGroup vlg = pPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.spacing = 5;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            // -- Top Info (Horizontal) --
            GameObject topInfo = new GameObject("TopInfo");
            topInfo.transform.SetParent(pPanel.transform, false);
            LayoutElement topLe = topInfo.AddComponent<LayoutElement>();
            topLe.minHeight = 60; topLe.preferredHeight = 60; topLe.flexibleHeight = 0;
            
            HorizontalLayoutGroup infoHlg = topInfo.AddComponent<HorizontalLayoutGroup>();
            infoHlg.childControlWidth = false; infoHlg.childForceExpandWidth = false;
            infoHlg.spacing = 10;
            
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(topInfo.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            LayoutElement iconLe = iconObj.AddComponent<LayoutElement>();
            iconLe.minWidth = 50; iconLe.minHeight = 50;
            php.iconImage = iconImg;
            
            // Name
            GameObject nameObj = CreateText("Name", $"P{i+1}", topInfo.transform, Vector2.zero, 18);
            LayoutElement nameLe = nameObj.AddComponent<LayoutElement>();
            nameLe.minWidth = 80;
            nameLe.flexibleWidth = 1;
            php.nameText = nameObj.GetComponent<Text>();
            php.nameText.alignment = TextAnchor.MiddleLeft;
            
            // Score
            GameObject scoreObj = CreateText("Score", "0", topInfo.transform, Vector2.zero, 20);
            LayoutElement scoreLe = scoreObj.AddComponent<LayoutElement>();
            scoreLe.minWidth = 50;
            php.scoreText = scoreObj.GetComponent<Text>();
            php.scoreText.alignment = TextAnchor.MiddleRight;

            // -- Sockets (Grid) --
            GameObject socketGrid = new GameObject("Sockets");
            socketGrid.transform.SetParent(pPanel.transform, false);
            LayoutElement gridLe = socketGrid.AddComponent<LayoutElement>();
            gridLe.minHeight = 60; gridLe.preferredHeight = 60; gridLe.flexibleHeight = 1;
            
            GridLayoutGroup glg = socketGrid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(25, 25);
            glg.spacing = new Vector2(5, 5);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4; // 4 columns
            glg.childAlignment = TextAnchor.MiddleCenter;

            // Create 8 Sockets
            php.sockets = new Image[8];
            for (int s = 0; s < 8; s++)
            {
                GameObject socket = new GameObject($"Socket_{s}");
                socket.transform.SetParent(socketGrid.transform, false);
                Image sImg = socket.AddComponent<Image>();
                sImg.color = Color.gray;
                php.sockets[s] = sImg;
            }

            ti.playerPanels.Add(php);
        }

        // Status Text (Floating above?)
        GameObject statusObj = CreateText("GlobalStatus", "Start Game!", hudPanel.transform, new Vector2(0, 100), 24);
        ti.turnStatusText = statusObj.GetComponent<Text>();
        // Status needs to ignore layout or be separate. 
        // Adding LayoutElement ignore
        LayoutElement statLe = statusObj.AddComponent<LayoutElement>();
        statLe.ignoreLayout = true;
        statusObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
        statusObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30); // Above panel

        hudPanel.SetActive(false);
    }
}
