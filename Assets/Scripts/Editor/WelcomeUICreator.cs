using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class WelcomeUICreator : EditorWindow
{
    [MenuItem("Tools/Board Game/Create Welcome Panel")]
    public static void ShowWindow()
    {
        GetWindow<WelcomeUICreator>("Welcome UI Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Welcome UI Generator", EditorStyles.boldLabel);
        GUILayout.Label("Generates the Welcome panel.", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("Generate Welcome Panel"))
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

        // 1. Create Welcome Step
        GameObject welcomeStep = CreatePanel("Step_Welcome", setupPanelTransform);
        // Ensure it's the first child so it draws on top/first in order of operations conceptually,
        // but typically GameSetupManager handles activation. Standard UI puts first steps at top of hierarchy.
        welcomeStep.transform.SetAsFirstSibling(); 
        
        CreateText("Welcome_Logo", "BOARD GAME LOGO", welcomeStep.transform, new Vector2(0, 100), 60);
        GameObject proceedBtn = CreateButton("Btn_Proceed", "Proceed", welcomeStep.transform, new Vector2(0, -50));

        // 2. Link to existing GameSetupManager
        Undo.RecordObject(manager, "Link Welcome UI");
        
        manager.welcomeStep = welcomeStep;
        
        // Clear any existing persistent listeners to avoid duplicates if run multiple times
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(proceedBtn.GetComponent<Button>().onClick, manager.OnWelcomeFinished);

        // Link Actions
        UnityEditor.Events.UnityEventTools.AddPersistentListener(proceedBtn.GetComponent<Button>().onClick, manager.OnWelcomeFinished);

        Debug.Log("Welcome UI Panel Generated and Linked Successfully!");
    }

    // --- Utility Methods (Copied from SetupUICreator to keep tool self-contained and avoid dependencies) ---

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
