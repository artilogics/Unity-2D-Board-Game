using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour {

    private static GameObject gameStatusText, player1MoveText, player2MoveText;

    private static GameObject player1, player2;

    public static int diceSideThrown = 0;
    public static int player1StartWaypoint = 0;
    public static int player2StartWaypoint = 0;

    public static bool gameOver = false;

    private static GameObject forwardButton, backwardButton;
    public static int playerToMove;
    private static GameObject dice;
    public static bool player1MissTurn = false;
    public static bool player2MissTurn = false;

    public static void SwitchTurn(int nextPlayer)
    {
        if (nextPlayer == 1)
        {
            if (player1MissTurn)
            {
                player1MissTurn = false;
                gameStatusText.gameObject.SetActive(true);
                gameStatusText.GetComponent<Text>().text = "Player 1 Missed Turn!";
                GameControl instance = GameObject.FindFirstObjectByType<GameControl>();
                if (instance != null) instance.StartCoroutine(instance.HideStatusText(2.0f));
                
                // Return control to Player 2
                // Dice turn should be -1 (Player 2)
                dice.GetComponent<Dice>().SetTurn(-1);
                
                player1MoveText.gameObject.SetActive(false);
                player2MoveText.gameObject.SetActive(true);
            }
            else
            {
                // Normal Switch to Player 1
                dice.GetComponent<Dice>().SetTurn(1);
                player1MoveText.gameObject.SetActive(true);
                player2MoveText.gameObject.SetActive(false);
            }
        }
        else // nextPlayer == 2
        {
            if (player2MissTurn)
            {
                player2MissTurn = false;
                gameStatusText.gameObject.SetActive(true);
                gameStatusText.GetComponent<Text>().text = "Player 2 Missed Turn!";
                GameControl instance = GameObject.FindFirstObjectByType<GameControl>();
                if (instance != null) instance.StartCoroutine(instance.HideStatusText(2.0f));
                
                // Return control to Player 1
                // Dice turn should be 1 (Player 1)
                dice.GetComponent<Dice>().SetTurn(1);
                
                player2MoveText.gameObject.SetActive(false);
                player1MoveText.gameObject.SetActive(true);
            }
            else
            {
                // Normal Switch to Player 2
                dice.GetComponent<Dice>().SetTurn(-1);
                player2MoveText.gameObject.SetActive(true);
                player1MoveText.gameObject.SetActive(false);
            }
        }
    }


    // Use this for initialization
    void Start () {

        dice = GameObject.Find("Dice");
        gameStatusText = GameObject.Find("GameStatusText");
        player1MoveText = GameObject.Find("Player1MoveText");
        player2MoveText = GameObject.Find("Player2MoveText");

        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");

        player1.GetComponent<FollowThePath>().moveAllowed = false;
        player2.GetComponent<FollowThePath>().moveAllowed = false;

        gameStatusText.gameObject.SetActive(false);
        player1MoveText.gameObject.SetActive(true);
        player2MoveText.gameObject.SetActive(false);

        // Find UI Buttons
        forwardButton = GameObject.Find("ForwardButton");
        backwardButton = GameObject.Find("BackwardButton");

        // Auto-create if missing
        if (forwardButton == null || backwardButton == null)
        {
            Transform canvasTransform = player1MoveText.transform.parent;
            
            if (forwardButton == null)
            {
                forwardButton = CreateButton("ForwardButton", "Forward", canvasTransform, new Vector2(100, -200));
            }
            
            if (backwardButton == null)
            {
                backwardButton = CreateButton("BackwardButton", "Backward", canvasTransform, new Vector2(-100, -200));
            }
        }
        
        if (forwardButton != null) {
            forwardButton.GetComponent<Button>().onClick.AddListener(MovePlayerForward);
            forwardButton.SetActive(false);
        }
        
        if (backwardButton != null) {
            backwardButton.GetComponent<Button>().onClick.AddListener(MovePlayerBackward);
            backwardButton.SetActive(false);
        }
    }

    private GameObject CreateButton(string name, string label, Transform parent, Vector2 offset)
    {
        // Create Button Object
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.9f); // Light gray
        img.preserveAspect = true;
        
        Button btn = buttonObj.AddComponent<Button>();
        
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(60, 60);
        rect.anchoredPosition = offset;

        // Create Text Object
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonObj;
    }

    // Update is called once per frame
    void Update()
    {
        // If buttons are active, player is choosing. Do not update state.
        if ((forwardButton != null && forwardButton.activeSelf) || 
            (backwardButton != null && backwardButton.activeSelf))
        {
            return;
        }

        // Check if player 1 is moving
        if (player1.GetComponent<FollowThePath>().moveAllowed)
        {
             return;
        }
        else if (playerToMove == 1 && player1MoveText.gameObject.activeSelf)
        {
             // Player 1 finished moving. Check for special tile.
             FollowThePath path = player1.GetComponent<FollowThePath>();
             SpecialTile tile = path.waypoints[path.waypointIndex].GetComponent<SpecialTile>();
             
             if (tile != null && tile.effect == SpecialTile.TileEffect.ExtraRoll)
             {
                 // Extra Roll! Keep turn.
                 dice.GetComponent<Dice>().SetTurn(1); // Force next roll to be Player 1
                 // Show Feedback
                 gameStatusText.gameObject.SetActive(true);
                 gameStatusText.GetComponent<Text>().text = "Extra Roll!";
                 StartCoroutine(HideStatusText(1.5f));
                 dice.GetComponent<Dice>().ResetDice();
                 playerToMove = 0;
             }
             else if (tile != null && tile.effect == SpecialTile.TileEffect.Shortcut)
             {
                 // Shortcut!
                 if (tile.possibleDestinations != null && tile.possibleDestinations.Count > 0)
                 {
                     // Always show choice (even if only 1)
                     gameStatusText.gameObject.SetActive(true);
                     gameStatusText.GetComponent<Text>().text = "Choose Path";
                     ShowShortcutOptions(1, tile.possibleDestinations);
                     // Do NOT reset playerToMove yet, wait for input.
                 }
                 else
                 {
                     // No destination? Treat as normal.
                     player1MoveText.gameObject.SetActive(false);
                     player2MoveText.gameObject.SetActive(true);
                     dice.GetComponent<Dice>().ResetDice();
                     playerToMove = 0;
                 }
             }
             else if (tile != null && tile.effect == SpecialTile.TileEffect.SkipTurn)
             {
                  player1MissTurn = true;
                 gameStatusText.gameObject.SetActive(true);
                 gameStatusText.GetComponent<Text>().text = "Miss Next Turn!";
                 StartCoroutine(HideStatusText(1.5f));
                 SwitchTurn(2);
                 dice.GetComponent<Dice>().ResetDice();
                 playerToMove = 0;
             }
             else
             {
                 // Normal Turn Switch
                 SwitchTurn(2);
                 dice.GetComponent<Dice>().ResetDice();
                 playerToMove = 0;
             }
        }
        
        // Check if player 2 is moving
        if (player2.GetComponent<FollowThePath>().moveAllowed)
        {
             return;
        }
        else if (playerToMove == 2 && player2MoveText.gameObject.activeSelf)
        {
             // Player 2 finished moving. Check for special tile.
             FollowThePath path = player2.GetComponent<FollowThePath>();
             SpecialTile tile = path.waypoints[path.waypointIndex].GetComponent<SpecialTile>();
             
             if (tile != null && tile.effect == SpecialTile.TileEffect.ExtraRoll)
             {
                 // Extra Roll! Keep turn.
                 dice.GetComponent<Dice>().SetTurn(-1); // Force next roll to be Player 2 (-1)
                 // Show Feedback
                 gameStatusText.gameObject.SetActive(true);
                 gameStatusText.GetComponent<Text>().text = "Extra Roll!";
                 StartCoroutine(HideStatusText(1.5f));
                 dice.GetComponent<Dice>().ResetDice();
                 playerToMove = 0;
             }
             else if (tile != null && tile.effect == SpecialTile.TileEffect.Shortcut)
             {
                 // Shortcut!
                 if (tile.possibleDestinations != null && tile.possibleDestinations.Count > 0)
                 {
                     // Always show choice
                     gameStatusText.gameObject.SetActive(true);
                     gameStatusText.GetComponent<Text>().text = "Choose Path";
                     ShowShortcutOptions(2, tile.possibleDestinations);
                 }
                 else
                 {
                     // No destination? Treat as normal.
                     player2MoveText.gameObject.SetActive(false);
                     player1MoveText.gameObject.SetActive(true);
                     dice.GetComponent<Dice>().ResetDice();
                     playerToMove = 0;
                 }
             }
             else if (tile != null && tile.effect == SpecialTile.TileEffect.SkipTurn)
             {
                 player2MissTurn = true;
                 gameStatusText.gameObject.SetActive(true);
                 gameStatusText.GetComponent<Text>().text = "Miss Next Turn!";
                 StartCoroutine(HideStatusText(1.5f));
                 SwitchTurn(1);
                 dice.GetComponent<Dice>().ResetDice();
                 playerToMove = 0;
             }
             else
             {
                 // Normal Turn Switch
                 SwitchTurn(1);
                 dice.GetComponent<Dice>().ResetDice();
                 playerToMove = 0;
             }
        }
        
        // Win condition checks (only if not moving)
        if (player1.GetComponent<FollowThePath>().waypointIndex == 
            player1.GetComponent<FollowThePath>().waypoints.Length - 1)
        {
            gameStatusText.gameObject.SetActive(true);
            gameStatusText.GetComponent<Text>().text = "Player 1 Wins";
            gameOver = true;
        }

        if (player2.GetComponent<FollowThePath>().waypointIndex ==
            player2.GetComponent<FollowThePath>().waypoints.Length - 1)
        {
            gameStatusText.gameObject.SetActive(true);
            player1MoveText.gameObject.SetActive(false);
            player2MoveText.gameObject.SetActive(false);
            gameStatusText.GetComponent<Text>().text = "Player 2 Wins";
            gameOver = true;
        }


        // Manage Visual Overlap
        FollowThePath p1Path = player1.GetComponent<FollowThePath>();
        FollowThePath p2Path = player2.GetComponent<FollowThePath>();
        
        // Check for same waypoint position
        if (p1Path.waypointIndex == p2Path.waypointIndex)
        {
            p1Path.isOverlapping = true;
            p2Path.isOverlapping = true;
        }
        else
        {
            p1Path.isOverlapping = false;
            p2Path.isOverlapping = false;
        }
    }

    IEnumerator HideStatusText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!gameOver) gameStatusText.SetActive(false);
    }

    public static void ShowDirectionOptions(int player)
    {
        playerToMove = player;
        GameObject activePlayerObj = (player == 1) ? player1 : player2;
        FollowThePath playerPath = activePlayerObj.GetComponent<FollowThePath>();
        Sprite playerSprite = activePlayerObj.GetComponent<SpriteRenderer>().sprite;

        // Calculate potential indices
        int currentIdx = playerPath.waypointIndex;
        int forwardIdx = currentIdx + diceSideThrown;
        int backwardIdx = currentIdx - diceSideThrown;

        // Setup Forward Button
        if (forwardIdx < playerPath.waypoints.Length)
        {
            if (forwardButton != null)
            {
                SetupSilhouetteButton(forwardButton, playerPath.waypoints[forwardIdx].transform.position, playerSprite);
                forwardButton.SetActive(true);
            }
        }
        else
        {
            if (forwardButton != null) forwardButton.SetActive(false);
        }

        // Setup Backward Button
        if (backwardIdx >= 0)
        {
            if (backwardButton != null)
            {
                SetupSilhouetteButton(backwardButton, playerPath.waypoints[backwardIdx].transform.position, playerSprite);
                backwardButton.SetActive(true);
            }
        }
        else
        {
            if (backwardButton != null) backwardButton.SetActive(false);
        }
    }

    private static void SetupSilhouetteButton(GameObject button, Vector3 worldPos, Sprite sprite)
    {
        // Position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        screenPos.y += 20; // Slight upward offset
        button.transform.position = screenPos;

        // Visuals
        Image btnImage = button.GetComponent<Image>();
        btnImage.sprite = sprite;
        btnImage.color = new Color(0, 0, 0, 0.8f); // Darker Silhouette style

        // Hide Text
        Text btnText = button.GetComponentInChildren<Text>();
        if (btnText != null) btnText.text = "";
    }

    public static void CalculateMove(int direction)
    {
        // Helper to avoid duplicate code if needed, but keeping existing structure for now.
    }

    private static List<Transform> currentShortcutOptions;

    public static void ShowShortcutOptions(int player, System.Collections.Generic.List<Transform> options)
    {
        playerToMove = player;
        currentShortcutOptions = options;
        
        GameObject activePlayerObj = (player == 1) ? player1 : player2;
        Sprite playerSprite = activePlayerObj.GetComponent<SpriteRenderer>().sprite;

        // Button 1 (Left/Back position mostly? Or just reuse Forward/Back)
        // Let's map Option 0 to ForwardButton (Right side usually) and Option 1 to BackwardButton (Left side)
        // Or just map based on screen position relative to player? 
        // For simplicity: Option 0 -> ForwardButton, Option 1 -> BackwardButton (if exists)
        
        // Clear previous listeners (Critical!)
        forwardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        backwardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        
        // Option 0
        if (options.Count > 0)
        {
            SetupSilhouetteButton(forwardButton, options[0].position, playerSprite);
            forwardButton.GetComponent<Button>().onClick.AddListener(() => MoveToShortcut(0));
            forwardButton.SetActive(true);
        }
        else
        {
            forwardButton.SetActive(false);
        }

        // Option 1
        if (options.Count > 1)
        {
            SetupSilhouetteButton(backwardButton, options[1].position, playerSprite);
            backwardButton.GetComponent<Button>().onClick.AddListener(() => MoveToShortcut(1));
            backwardButton.SetActive(true);
        }
        else
        {
            backwardButton.SetActive(false);
        }
    }

    public static void MoveToShortcut(int optionIndex)
    {
        if (currentShortcutOptions == null || optionIndex >= currentShortcutOptions.Count) return;

        Transform target = currentShortcutOptions[optionIndex];
        
        // Hide Buttons
        forwardButton.SetActive(false);
        backwardButton.SetActive(false);
        
        // Restore default listeners for next turn
        forwardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        backwardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        forwardButton.GetComponent<Button>().onClick.AddListener(MovePlayerForward);
        backwardButton.GetComponent<Button>().onClick.AddListener(MovePlayerBackward);

        // Execute Move
        GameObject activePlayer = (playerToMove == 1) ? player1 : player2;
        activePlayer.GetComponent<FollowThePath>().StartHop(target);
        
        // Show Feedback
        gameStatusText.gameObject.SetActive(true);
        gameStatusText.GetComponent<Text>().text = "Shortcut!";
        // We can't start coroutine from static easily without instance reference, 
        // but existing StartCoroutine calls were inside non-static methods or instance was used?
        // Ah, GameControl is MonoBehaviour but methods are static. 
        // The existing code uses `StartCoroutine` which implies `this`.
        // Wait, `StartCoroutine` is used locally in Update (instance method).
        // But `MovePlayerForward` is static? It doesn't use `StartCoroutine`.
        // `StartCoroutine` is only used in Update.
        // We need a reference to the instance to run Coroutine.
        // Let's find the instance.
        GameControl instance = GameObject.FindFirstObjectByType<GameControl>();
        if (instance != null) instance.StartCoroutine(instance.HideStatusText(1.5f));

        // Switch Turn
        if (playerToMove == 1)
        {
            SwitchTurn(2);
        }
        else
        {
            SwitchTurn(1);
        }
        
        dice.GetComponent<Dice>().ResetDice();
        playerToMove = 0;
    }

    public static void MovePlayerForward()
    {
        if (forwardButton != null) forwardButton.SetActive(false);
        if (backwardButton != null) backwardButton.SetActive(false);

        if (playerToMove == 1)
        {
             player1.GetComponent<FollowThePath>().StartMove(diceSideThrown);
        }
        else if (playerToMove == 2)
        {
             player2.GetComponent<FollowThePath>().StartMove(diceSideThrown);
        }
    }

    public static void MovePlayerBackward()
    {
        if (forwardButton != null) forwardButton.SetActive(false);
        if (backwardButton != null) backwardButton.SetActive(false);

        if (playerToMove == 1)
        {
             player1.GetComponent<FollowThePath>().StartMove(-diceSideThrown);
        }
        else if (playerToMove == 2)
        {
             player2.GetComponent<FollowThePath>().StartMove(-diceSideThrown);
        }
    }
}
