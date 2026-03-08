using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class StartGameUICreator : EditorWindow
{
    [MenuItem("Tools/Board Game/Create Start Game Panel")]
    public static void ShowWindow()
    {
        GetWindow<StartGameUICreator>("Start Game UI Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Start Game UI Generator", EditorStyles.boldLabel);
        GUILayout.Label("Generates the Start Game panel and links it.", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("Generate Start Game Panel"))
        {
            CreateUI();
        }
    }

    private void CreateUI()
    {
        GameSetupManager manager = FindFirstObjectByType<GameSetupManager>();
        if (manager == null)
        {
            Debug.LogError("Could not find GameSetupManager! Please generate the main Setup UI first.");
            return;
        }

        if (manager.setupPanel == null)
        {
             Debug.LogError("GameSetupManager does not have a Setup Panel assigned! Please generate the main Setup UI first.");
             return;
        }

        Transform setupPanelTransform = manager.setupPanel.transform;

        // Create Start Game Step
        GameObject startGameStep = CreatePanel("Step_StartGame", setupPanelTransform);
        
        // Put it after Welcome Step if it exists, otherwise just at the top
        if (manager.welcomeStep != null)
        {
            startGameStep.transform.SetSiblingIndex(manager.welcomeStep.transform.GetSiblingIndex() + 1);
        }
        else
        {
            startGameStep.transform.SetAsFirstSibling();
        }
        
        startGameStep.SetActive(false);
        CreateText("StartGame_Title", "Session Type", startGameStep.transform, new Vector2(0, 200), 40);
        
        // Create Date Inputs horizontally (Dropdowns)
        List<string> dayOptions = new List<string> { "DD" };
        for (int i = 1; i <= 31; i++) dayOptions.Add(i.ToString("00"));
        GameObject dayInputObj = CreateDropdown("DayDropdown", startGameStep.transform, new Vector2(-120, 100), dayOptions);
        Dropdown dayInput = dayInputObj.GetComponent<Dropdown>();

        List<string> monthOptions = new List<string> { "MM" };
        for (int i = 1; i <= 12; i++) monthOptions.Add(i.ToString("00"));
        GameObject monthInputObj = CreateDropdown("MonthDropdown", startGameStep.transform, new Vector2(0, 100), monthOptions);
        Dropdown monthInput = monthInputObj.GetComponent<Dropdown>();

        List<string> yearOptions = new List<string> { "YYYY" };
        int currentYear = System.DateTime.Now.Year;
        for (int i = 0; i < 10; i++) yearOptions.Add((currentYear - i).ToString());
        GameObject yearInputObj = CreateDropdown("YearDropdown", startGameStep.transform, new Vector2(120, 100), yearOptions);
        Dropdown yearInput = yearInputObj.GetComponent<Dropdown>();
        
        GameObject codeInputObj = CreateInputField("CodeInput", startGameStep.transform, new Vector2(0, 20));
        codeInputObj.transform.Find("Placeholder").GetComponent<Text>().text = "Game Code";
        InputField codeInput = codeInputObj.GetComponent<InputField>();

        GameObject loggedBtn = CreateButton("Btn_LoggedGame", "Logged Game (Required)", startGameStep.transform, new Vector2(-180, -80));
        loggedBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 50); // Make button wider for text
        GameObject guestBtn = CreateButton("Btn_GuestSession", "Guest Session (Skips)", startGameStep.transform, new Vector2(180, -80));
        guestBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 50);

        GameObject feedbackObj = CreateText("FeedbackText", "", startGameStep.transform, new Vector2(0, -150), 20);
        Text feedbackText = feedbackObj.GetComponent<Text>();
        feedbackText.color = Color.red;

        // Link to existing GameSetupManager
        Undo.RecordObject(manager, "Link Start Game UI");
        
        manager.startGameStep = startGameStep;
        manager.dayInput = dayInput;
        manager.monthInput = monthInput;
        manager.yearInput = yearInput;
        manager.codeInput = codeInput;
        manager.startGameFeedbackText = feedbackText;
        
        // Clear any existing persistent listeners to avoid duplicates if run multiple times
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(loggedBtn.GetComponent<Button>().onClick, manager.OnLoggedGameSelected);
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(guestBtn.GetComponent<Button>().onClick, manager.OnGuestSessionSelected);

        // Link Actions
        UnityEditor.Events.UnityEventTools.AddPersistentListener(loggedBtn.GetComponent<Button>().onClick, manager.OnLoggedGameSelected);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(guestBtn.GetComponent<Button>().onClick, manager.OnGuestSessionSelected);

        Debug.Log("Start Game UI Panel Generated and Linked Successfully!");
    }

    // --- Utility Methods (Copied from SetupUICreator to keep tool self-contained and avoid dependencies) ---

    private GameObject CreateDropdown(string name, Transform parent, Vector2 pos, List<string> options)
    {
        DefaultControls.Resources uiResources = new DefaultControls.Resources();
        uiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        uiResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        uiResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        uiResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        
        GameObject root = DefaultControls.CreateDropdown(uiResources);
        root.name = name;
        root.transform.SetParent(parent, false);
        
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50);
        rt.anchoredPosition = pos;

        Dropdown dropdown = root.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        
        RectTransform labelRT = dropdown.captionText.GetComponent<RectTransform>();
        labelRT.offsetMin = new Vector2(10, 0);
        labelRT.offsetMax = new Vector2(-25, 0);
        dropdown.captionText.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText.fontSize = 20;

        return root;
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
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = fontSize;
        txt.color = Color.white; 
        
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
        
        GameObject placeholder = CreateText("Placeholder", "Enter Value...", root.transform, Vector2.zero, 24);
        placeholder.GetComponent<Text>().color = Color.gray;
        
        GameObject text = CreateText("Text", "", root.transform, Vector2.zero, 24);
        text.GetComponent<Text>().color = Color.black;
        
        input.targetGraphic = bg;
        input.placeholder = placeholder.GetComponent<Text>();
        input.textComponent = text.GetComponent<Text>();
        
        return root;
    }
}
