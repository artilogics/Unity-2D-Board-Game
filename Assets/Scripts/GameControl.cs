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
            forwardButton.GetComponent<Button>().onClick.AddListener(() => MovePlayerForward());
            forwardButton.SetActive(false);
        }
        
        if (backwardButton != null) {
            backwardButton.GetComponent<Button>().onClick.AddListener(() => MovePlayerBackward());
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

        // If markers are active, player is choosing direction. Do not update state.
        if (activeMarkers.Count > 0)
        {
            return;
        }

        // Check if player 1 is moving
        if (player1 != null && player1.GetComponent<FollowThePath>().moveAllowed)
        {
             return;
        }
        else if (player1 != null && playerToMove == 1 && player1MoveText.gameObject.activeSelf)
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
             else if (tile != null && tile.effect == SpecialTile.TileEffect.QuestionTile)
             {
                 // Question Tile - Show trivia popup
                 if (TriviaPopup.Instance != null)
                 {
                     playerToMove = 0; // Reset immediately to prevent infinite loop
                     TriviaPopup.Instance.ShowQuestion(tile.GetCategoryString(), 1, (isCorrect) =>
                     {
                         if (isCorrect)
                         {
                             // Correct answer - grant reroll
                             gameStatusText.gameObject.SetActive(true);
                             gameStatusText.GetComponent<Text>().text = "Correct! Extra Roll!";
                             StartCoroutine(HideStatusText(1.5f));
                             dice.GetComponent<Dice>().SetTurn(1);
                             dice.GetComponent<Dice>().ResetDice();
                         }
                         else
                         {
                             // Wrong answer - switch turn
                             SwitchTurn(2);
                             dice.GetComponent<Dice>().ResetDice();
                         }
                     });
                 }
                 else
                 {
                     // No trivia popup - treat as normal tile
                     SwitchTurn(2);
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
        if (player2 != null && player2.GetComponent<FollowThePath>().moveAllowed)
        {
             return;
        }
        else if (player2 != null && playerToMove == 2 && player2MoveText.gameObject.activeSelf)
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
             else if (tile != null && tile.effect == SpecialTile.TileEffect.QuestionTile)
             {
                 // Question Tile - Show trivia popup
                 if (TriviaPopup.Instance != null)
                 {
                     playerToMove = 0; // Reset immediately to prevent infinite loop
                     TriviaPopup.Instance.ShowQuestion(tile.GetCategoryString(), 2, (isCorrect) =>
                     {
                         if (isCorrect)
                         {
                             // Correct answer - grant reroll
                             gameStatusText.gameObject.SetActive(true);
                             gameStatusText.GetComponent<Text>().text = "Correct! Extra Roll!";
                             StartCoroutine(HideStatusText(1.5f));
                             dice.GetComponent<Dice>().SetTurn(-1);
                             dice.GetComponent<Dice>().ResetDice();
                         }
                         else
                         {
                             // Wrong answer - switch turn
                             SwitchTurn(1);
                             dice.GetComponent<Dice>().ResetDice();
                         }
                     });
                 }
                 else
                 {
                     // No trivia popup - treat as normal tile
                     SwitchTurn(1);
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
        // Win condition checks (only if not moving)
        if (player1 != null && 
            !player1.GetComponent<FollowThePath>().isCircular &&
            player1.GetComponent<FollowThePath>().waypointIndex == player1.GetComponent<FollowThePath>().waypoints.Length - 1)
        {
            gameStatusText.gameObject.SetActive(true);
            gameStatusText.GetComponent<Text>().text = "Player 1 Wins";
            gameOver = true;
        }

        if (player2 != null && 
            !player2.GetComponent<FollowThePath>().isCircular &&
            player2.GetComponent<FollowThePath>().waypointIndex == player2.GetComponent<FollowThePath>().waypoints.Length - 1)
        {
            gameStatusText.gameObject.SetActive(true);
            player1MoveText.gameObject.SetActive(false);
            player2MoveText.gameObject.SetActive(false);
            gameStatusText.GetComponent<Text>().text = "Player 2 Wins";
            gameOver = true;
        }


        // Manage Visual Overlap
        if (player1 != null && player2 != null)
        {
            FollowThePath p1Path = player1.GetComponent<FollowThePath>();
            FollowThePath p2Path = player2.GetComponent<FollowThePath>();
            
            // Check for same waypoint position
            if (p1Path.waypointIndex == p2Path.waypointIndex && p1Path.waypointIndex >= 0)
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
    }

    IEnumerator HideStatusText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!gameOver) gameStatusText.SetActive(false);
    }

    [Header("Direction Markers")]
    public GameObject markerPrefab2D; // User defined 2D marker
    public GameObject markerPrefab3D; // User defined 3D marker
    public bool use3DMarkers = false; // Toggle for logic
    public Vector3 markerOffset = new Vector3(0, 1.5f, 0); // Customizable offset
    public Vector3 markerRotation = new Vector3(0, 0, 180); // Default to pointing down

    private static List<GameObject> activeMarkers = new List<GameObject>();

    private static void ClearMarkers()
    {
        foreach (var marker in activeMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        activeMarkers.Clear();
    }

    private static void SpawnMarker(Vector3 position, System.Action onClickAction)
    {
        GameControl instance = GameObject.FindFirstObjectByType<GameControl>();
        if (instance == null) return;

        // Use instance offset & rotation
        Vector3 finalPos = position + instance.markerOffset;
        Quaternion finalRot = Quaternion.Euler(instance.markerRotation);
        
        GameObject prefabToUse = instance.use3DMarkers ? instance.markerPrefab3D : instance.markerPrefab2D;
        GameObject markerObj;

        if (prefabToUse != null)
        {
            // Instantiate user prefab
            markerObj = Instantiate(prefabToUse, finalPos, finalRot);
        }
        else
        {
            // FALLBACK SYSTEM
            Debug.LogWarning("No Marker Prefab assigned! Using fallback arrow.");
            
            Sprite arrowSprite = Resources.Load<Sprite>("arrow_marker");
            if (arrowSprite == null)
            {
                Texture2D arrowTex = Resources.Load<Texture2D>("arrow_marker");
                if (arrowTex != null)
                    arrowSprite = Sprite.Create(arrowTex, new Rect(0, 0, arrowTex.width, arrowTex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            
            markerObj = new GameObject("DirectionMarker_Fallback");
            markerObj.transform.position = finalPos;
            markerObj.transform.rotation = finalRot;
            
            SpriteRenderer sr = markerObj.AddComponent<SpriteRenderer>();
            sr.sprite = arrowSprite;
            sr.sortingOrder = 10;
        }

        // Ensure logic exists
        DirectionMarker dm = markerObj.GetComponent<DirectionMarker>();
        if (dm == null) dm = markerObj.AddComponent<DirectionMarker>();
        
        dm.Setup(onClickAction);
        
        // Track it
        activeMarkers.Add(markerObj);
    }

    public static void ShowDirectionOptions(int player)
    {
        playerToMove = player;
        GameObject activePlayerObj = (player == 1) ? player1 : player2;
        FollowThePath playerPath = activePlayerObj.GetComponent<FollowThePath>();
        
        // Clear old markers (just in case)
        ClearMarkers();

        // Calculate indices
        int currentIdx = playerPath.waypointIndex;
        bool isCircular = playerPath.isCircular;
        bool atStartPosition = (currentIdx == -1);
        
        // Calculate indices using the robust method in FollowThePath
        int forwardIdx = playerPath.CalculateTargetIndex(currentIdx, diceSideThrown);
        int backwardIdx = playerPath.CalculateTargetIndex(currentIdx, -diceSideThrown);

        // Option A: Forward
        if (isCircular || forwardIdx < playerPath.waypoints.Length)
        {
            if (isCircular)
            {
                while (forwardIdx >= playerPath.waypoints.Length) forwardIdx -= playerPath.waypoints.Length;
                while (forwardIdx < 0) forwardIdx += playerPath.waypoints.Length;
            }
            
            // Linear Check: Prevent going backward past 0 (or to -1)
            // CalculateTargetIndex returns < 0 if invalid on linear
            bool valid = true;
            if (!isCircular)
            {
                if (forwardIdx < 0) valid = false;
            }

            if (valid && forwardIdx < playerPath.waypoints.Length) 
            {
                 SpawnMarker(playerPath.waypoints[forwardIdx].transform.position, () => MovePlayerForward(player));
            }
        }

        // Option B: Backward
        if (isCircular || backwardIdx >= 0)
        {
            if (isCircular)
            {
                while (backwardIdx < 0) backwardIdx += playerPath.waypoints.Length;
                 while (backwardIdx >= playerPath.waypoints.Length) backwardIdx -= playerPath.waypoints.Length;
            }
            
            // Linear Check
            bool valid = true;
            if (!isCircular)
            {
                 if (backwardIdx < 0) valid = false;
            }
            
            if (valid && backwardIdx >= 0 && backwardIdx < playerPath.waypoints.Length)
            {
                 SpawnMarker(playerPath.waypoints[backwardIdx].transform.position, () => MovePlayerBackward(player));
            }
        }
    }

    public static void CalculateMove(int direction)
    {
        // Deprecated helper
    }

    private static List<Transform> currentShortcutOptions;

    public static void ShowShortcutOptions(int player, System.Collections.Generic.List<Transform> options)
    {
        playerToMove = player;
        currentShortcutOptions = options;
        
        ClearMarkers();
        
        // Option 0
        if (options.Count > 0)
        {
            SpawnMarker(options[0].position, () => MoveToShortcut(0));
        }

        // Option 1
        if (options.Count > 1)
        {
            SpawnMarker(options[1].position, () => MoveToShortcut(1));
        }
    }

    public static void MoveToShortcut(int optionIndex)
    {
        Debug.Log($"[GameControl] MoveToShortcut called. Index: {optionIndex}, Player: {playerToMove}");
        if (currentShortcutOptions == null || optionIndex >= currentShortcutOptions.Count) 
        {
             Debug.LogError("Shortcut options invalid or index out of range!");
             return;
        }

        ClearMarkers(); // Hide arrows
        Transform target = currentShortcutOptions[optionIndex];
        
        // Hide Buttons (Legacy)
        if (forwardButton != null) forwardButton.SetActive(false);
        if (backwardButton != null) backwardButton.SetActive(false);
        
        // Execute Move
        GameObject activePlayer = (playerToMove == 1) ? player1 : player2;
        activePlayer.GetComponent<FollowThePath>().StartHop(target);
        
        // Show Feedback
        if (gameStatusText != null) 
        {
            gameStatusText.gameObject.SetActive(true);
            gameStatusText.GetComponent<Text>().text = "Shortcut!";
        }

        GameControl instance = GameObject.FindFirstObjectByType<GameControl>();
        if (instance != null) instance.StartCoroutine(instance.HideStatusText(1.5f));

        // Switch Turn
        if (playerToMove == 1) SwitchTurn(2);
        else SwitchTurn(1);
        
        dice.GetComponent<Dice>().ResetDice();
        playerToMove = 0;
    }

    public static void MovePlayerForward(int playerOverride = 0)
    {
        int p = (playerOverride != 0) ? playerOverride : playerToMove;
        
        Debug.Log($"[GameControl] MovePlayerForward called. Player: {p} (Override: {playerOverride}, Static: {playerToMove}), Dice: {diceSideThrown}");
        
        ClearMarkers(); // Hide arrows
        if (forwardButton != null) forwardButton.SetActive(false);
        if (backwardButton != null) backwardButton.SetActive(false);

        if (p == 1)
        {
            player1.GetComponent<FollowThePath>().moveAllowed = true;
            player1.GetComponent<FollowThePath>().StartMove(diceSideThrown);
        }
        else if (p == 2)
        {
            player2.GetComponent<FollowThePath>().moveAllowed = true;
            player2.GetComponent<FollowThePath>().StartMove(diceSideThrown);
        }
        else
        {
            Debug.LogError($"[GameControl] MovePlayerForward failed! Player is 0. (Override: {playerOverride}, Static: {playerToMove})");
        }
        
        // playerToMove = 0; // Reset static <<-- REMOVED: Must persist for Update() to detect turn end
    }

    public static void MovePlayerBackward(int playerOverride = 0)
    {
        int p = (playerOverride != 0) ? playerOverride : playerToMove;
        
        Debug.Log($"[GameControl] MovePlayerBackward called. Player: {p} (Override: {playerOverride}, Static: {playerToMove}), Dice: {diceSideThrown}");
        
        ClearMarkers(); // Hide arrows
        if (forwardButton != null) forwardButton.SetActive(false);
        if (backwardButton != null) backwardButton.SetActive(false);

        if (p == 1)
        {
            player1.GetComponent<FollowThePath>().moveAllowed = true;
            player1.GetComponent<FollowThePath>().StartMove(-diceSideThrown);
        }
        else if (p == 2)
        {
            player2.GetComponent<FollowThePath>().moveAllowed = true;
            player2.GetComponent<FollowThePath>().StartMove(-diceSideThrown);
        }
        
        // playerToMove = 0; // Reset <<-- REMOVED: Must persist for Update() to detect turn end
    }

}
