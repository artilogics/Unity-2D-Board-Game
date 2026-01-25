using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class TriviaUICreator : EditorWindow
{
    [MenuItem("Tools/Create Trivia Popup UI")]
    static void CreateTriviaPopup()
    {
        // Find Canvas or create one
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create parent controller GameObject (STAYS ENABLED - holds script)
        GameObject controller = new GameObject("TriviaPopupController");
        controller.transform.SetParent(canvas.transform, false);
        
        // Add TriviaPopup script to controller
        TriviaPopup popup = controller.AddComponent<TriviaPopup>();

        // Create Popup Panel as child (this is what gets shown/hidden)
        GameObject popupPanel = new GameObject("PopupPanel");
        popupPanel.transform.SetParent(controller.transform, false);
        
        RectTransform popupRect = popupPanel.AddComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.sizeDelta = new Vector2(600, 500);
        
        Image popupBg = popupPanel.AddComponent<Image>();
        popupBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Category Text
        GameObject categoryObj = CreateText("CategoryText", popupPanel.transform, new Vector2(0, 200), "Category: Science");
        Text categoryText = categoryObj.GetComponent<Text>();
        categoryText.fontSize = 20;
        categoryText.fontStyle = FontStyle.Bold;
        categoryText.color = Color.yellow;

        // Question Text
        GameObject questionObj = CreateText("QuestionText", popupPanel.transform, new Vector2(0, 140), "What is the question?");
        Text questionText = questionObj.GetComponent<Text>();
        questionText.fontSize = 24;
        questionText.fontStyle = FontStyle.Bold;
        questionText.alignment = TextAnchor.MiddleCenter;
        questionObj.GetComponent<RectTransform>().sizeDelta = new Vector2(550, 100);

        // Option Buttons
        Button btnA = CreateButton("OptionA_Button", popupPanel.transform, new Vector2(0, 50), "A) Option A");
        Button btnB = CreateButton("OptionB_Button", popupPanel.transform, new Vector2(0, 0), "B) Option B");
        Button btnC = CreateButton("OptionC_Button", popupPanel.transform, new Vector2(0, -50), "C) Option C");
        Button btnD = CreateButton("OptionD_Button", popupPanel.transform, new Vector2(0, -100), "D) Option D");

        // Confirm Button
        Button confirmBtn = CreateButton("ConfirmButton", popupPanel.transform, new Vector2(0, -160), "CONFIRM");
        confirmBtn.GetComponent<Image>().color = new Color(0, 0.8f, 0, 1);
        confirmBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

        // Feedback Text
        GameObject feedbackObj = CreateText("FeedbackText", popupPanel.transform, new Vector2(0, -210), "");
        Text feedbackText = feedbackObj.GetComponent<Text>();
        feedbackText.fontSize = 20;
        feedbackText.fontStyle = FontStyle.Bold;

        // Confetti Particle System
        GameObject confettiObj = new GameObject("ConfettiEffect");
        confettiObj.transform.SetParent(popupPanel.transform, false);
        RectTransform confettiRect = confettiObj.AddComponent<RectTransform>();
        confettiRect.anchoredPosition = new Vector2(0, -250);
        
        ParticleSystem confetti = confettiObj.AddComponent<ParticleSystem>();
        var main = confetti.main;
        main.startLifetime = 2f;
        main.startSpeed = 10f;
        main.startSize = 0.5f;
        main.maxParticles = 100;
        main.loop = false;
        
        var emission = confetti.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 50) });
        
        var shape = confetti.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.rotation = new Vector3(-90, 0, 0);
        
        var colorOverLifetime = confetti.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        confetti.Stop();

        // Link everything to popup script
        popup.popupPanel = popupPanel;
        popup.questionText = questionText;
        popup.categoryText = categoryText;
        popup.optionA_Button = btnA;
        popup.optionB_Button = btnB;
        popup.optionC_Button = btnC;
        popup.optionD_Button = btnD;
        popup.confirmButton = confirmBtn;
        popup.feedbackText = feedbackText;
        popup.confettiEffect = confetti;
        // Note: player points text will be added separately

        // Initially hide the panel (NOT the controller!)
        popupPanel.SetActive(false);

        // Select the created popup
        Selection.activeGameObject = controller;

        Debug.Log("✅ Trivia Popup UI created successfully!");
        EditorUtility.DisplayDialog("Success!", 
            "Trivia Popup UI created!\n\n" +
            "✅ Controller GameObject is ENABLED\n" +
            "✅ Popup panel starts hidden\n" +
            "✅ All components linked", 
            "OK");
    }

    static GameObject CreateText(string name, Transform parent, Vector2 position, string defaultText)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 30);

        Text text = textObj.AddComponent<Text>();
        text.text = defaultText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return textObj;
    }

    static Button CreateButton(string name, Transform parent, Vector2 position, string buttonText)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(500, 40);

        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;

        Button btn = btnObj.AddComponent<Button>();

        // Add Text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;

        return btn;
    }
}
