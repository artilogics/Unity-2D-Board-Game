using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour {

    public static GameControl Instance;

    [Header("Game Settings")]
    public GameObject playerPrefab; // Prefab to spawn
    public Transform startPoint; // Where to spawn
    public FollowThePath mapReference; // To copy waypoints from
    
    [Header("UI References")]
    public Text statusText;
    public TurnIndicatorUI turnIndicator; // New Reference

    // Dynamic State
    public static List<GameObject> players = new List<GameObject>();
    public static int currentPlayerIndex = 0;
    public static bool gameOver = false;
    public static int diceSideThrown = 0;
    
    // Internal
    private static bool[] playerMissTurn;
    private static GameObject dice;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        dice = GameObject.Find("Dice");
    }

    // Initialize Game from Setup Manager
    public void InitializeGame(List<PlayerConfiguration> configs)
    {
        players.Clear();
        activeMarkers.Clear();
        
        playerMissTurn = new bool[configs.Count];
        
        // Find Waypoints reference if missing
        if (mapReference == null)
        {
             GameObject p1 = GameObject.Find("Player1");
             if (p1) mapReference = p1.GetComponent<FollowThePath>();
        }

        // Spawn Players
        int i = 0;
        foreach (var cfg in configs)
        {
            // Spawn
            Vector3 spawnPos = (startPoint != null) ? startPoint.position : Vector3.zero;
            if (mapReference != null && mapReference.waypoints.Length > 0 && startPoint == null)
                 spawnPos = mapReference.waypoints[0].position;

            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            newPlayer.name = cfg.PlayerName;
            
            // Setup Visuals
            SpriteRenderer sr = newPlayer.GetComponent<SpriteRenderer>();
            if (sr) sr.sprite = cfg.CharacterSprite;
            
            // Setup Path
            FollowThePath path = newPlayer.GetComponent<FollowThePath>();
            if (path && mapReference)
            {
                path.waypoints = mapReference.waypoints;
                path.isCircular = mapReference.isCircular;
                // Fix: explicit start position if assigned
                if (startPoint != null) path.startPosition = startPoint;
            }
            
            // Offset for visibility
            // Centered offset logic: (i - (Total-1)/2) * spacing
            // Using Diagonal offset (X and Z) as requested
            float spacing = 0.6f;
            float centerOffset = (i - (configs.Count - 1) / 2.0f) * spacing;
            path.playerOffset = new Vector3(centerOffset, 0, centerOffset); 

            players.Add(newPlayer);
            
            // Setup UI
            // Setup UI - Removed legacy list logic
            i++;
        }
        
        // Remove existing hardcoded objects if any exist and use them as reference
        GameObject existingP1 = GameObject.Find("Player1");
        if (existingP1 && !players.Contains(existingP1)) existingP1.SetActive(false);
        GameObject existingP2 = GameObject.Find("Player2");
        if (existingP2 && !players.Contains(existingP2)) existingP2.SetActive(false);
        
        // Start Game
        currentPlayerIndex = 0;
        dice.GetComponent<Dice>().SetTurn(0); // Set turn to index 0
        gameOver = false;
        
        UpdateUI();
    }

    private static void UpdateUI()
    {
        if (Instance == null) return;
        
        if (Instance.turnIndicator && players.Count > currentPlayerIndex)
        {
             // Ensure it's visible
             if (!Instance.turnIndicator.gameObject.activeSelf) 
                 Instance.turnIndicator.gameObject.SetActive(true);

             GameObject p = players[currentPlayerIndex];
             if (p)
             {
                 Sprite s = p.GetComponent<SpriteRenderer>().sprite;
                 Instance.turnIndicator.UpdateTurn(p.name, s);
             }
        }
        
        // Status Text is shared
    }

    public static void ReportTurnEnd()
    {
       // Called when movement finishes
       // Check Win
       // Check Special Tile
       // Then Switch Turn
       if (Instance) Instance.HandleTurnEnd();
    }

    void HandleTurnEnd()
    {
        GameObject activePlayer = players[currentPlayerIndex];
        FollowThePath path = activePlayer.GetComponent<FollowThePath>();
        
        // Win Check
        if (!path.isCircular && path.waypointIndex == path.waypoints.Length - 1)
        {
            gameOver = true;
            statusText.gameObject.SetActive(true);
            statusText.text = $"{activePlayer.name} Wins!";
            return;
        }
        
        // Loop Prevention
        if (justHopped)
        {
            justHopped = false;
            // Skip tile effects on landing from a shortcut?
            // Yes, to prevent infinite loops if destination is also shortcut
            SwitchTurn();
            dice.GetComponent<Dice>().ResetDice();
            return;
        }
        
        // Tile Check
        SpecialTile tile = null;
        if (path.waypointIndex >= 0 && path.waypointIndex < path.waypoints.Length)
             tile = path.waypoints[path.waypointIndex].GetComponent<SpecialTile>();

        if (tile != null)
        {
            // Simplified Tile Logic for Refactor
            if (tile.effect == SpecialTile.TileEffect.ExtraRoll)
            {
                ShowStatus("Extra Roll!", 1.5f);
                dice.GetComponent<Dice>().ResetDice();
                // Turn stays same
                return;
            }
            else if (tile.effect == SpecialTile.TileEffect.SkipTurn)
            {
                // Next time this player plays, they miss it.
                // But for now, we just switch.
                // Logic: playerMissTurn[currentPlayerIndex] = true;
                ShowStatus("Miss Next Turn!", 1.5f);
                playerMissTurn[currentPlayerIndex] = true;
                SwitchTurn();
                dice.GetComponent<Dice>().ResetDice();
                return;
            }
            else if (tile.effect == SpecialTile.TileEffect.Shortcut)
            {
                 if (tile.possibleDestinations != null && tile.possibleDestinations.Count > 0)
                 {
                     ShowStatus("Choose Path", 1f);
                     ShowShortcutOptions(currentPlayerIndex, tile.possibleDestinations);
                     return; // Wait for input
                 }
            }
        }
        
        // Normal Switch
        SwitchTurn();
        dice.GetComponent<Dice>().ResetDice();
    }

    public static void SwitchTurn()
    {
        if (gameOver) return;
        
        int attempts = 0;
        int nextPlayer = currentPlayerIndex;
        
        // Find next valid player
        do {
            nextPlayer = (nextPlayer + 1) % players.Count;
            attempts++;
            
            // Check Miss Turn
            if (playerMissTurn[nextPlayer])
            {
                if (Instance) Instance.ShowStatus($"Player {nextPlayer+1} Missed Turn!", 1.5f);
                playerMissTurn[nextPlayer] = false; // Consumed
                // Loop again to skip
            }
            else
            {
                break; // Found valid player
            }
            
        } while (attempts < players.Count);
        
        currentPlayerIndex = nextPlayer;
        dice.GetComponent<Dice>().SetTurn(currentPlayerIndex);
        UpdateUI();
    }
    
    public void ShowStatus(string text, float duration)
    {
        if (statusText)
        {
            statusText.text = text;
            statusText.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideStatus(duration));
        }
    }
    
    IEnumerator HideStatus(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (statusText) statusText.gameObject.SetActive(false);
    }

    // --- Static Forwarders & Logic ---

    [Header("Direction Markers")]
    public GameObject markerPrefab2D;
    public GameObject markerPrefab3D;
    public bool use3DMarkers = false;
    public Vector3 markerOffset = new Vector3(0, 1.5f, 0); 
    public Vector3 markerRotation = new Vector3(0, 0, 180);
    private static List<GameObject> activeMarkers = new List<GameObject>();

    private static void ClearMarkers()
    {
        foreach (var marker in activeMarkers) if (marker) Destroy(marker);
        activeMarkers.Clear();
    }

    private static void SpawnMarker(Vector3 position, System.Action onClickAction)
    {
        if (Instance == null) return;
        
        Vector3 finalPos = position + Instance.markerOffset;
        Quaternion finalRot = Quaternion.Euler(Instance.markerRotation);
         GameObject prefab = Instance.use3DMarkers ? Instance.markerPrefab3D : Instance.markerPrefab2D;
         
         GameObject marker = null;
         if (prefab) marker = Instantiate(prefab, finalPos, finalRot);
         else { /* Fallback omitted for brevity, stick to prefab */ }
         
         if (marker)
         {
             DirectionMarker dm = marker.GetComponent<DirectionMarker>();
             if (!dm) dm = marker.AddComponent<DirectionMarker>();
             dm.Setup(onClickAction);
             activeMarkers.Add(marker);
         }
    }

    public static void ShowDirectionOptions(int playerIndex)
    {
        // New signature: explicitly index
        if (playerIndex != currentPlayerIndex) return; // Safety
        
        GameObject p = players[playerIndex];
        FollowThePath path = p.GetComponent<FollowThePath>();
        
        ClearMarkers();
        
        int current = path.waypointIndex;
        int fwd = path.CalculateTargetIndex(current, diceSideThrown);
        int bwd = path.CalculateTargetIndex(current, -diceSideThrown);
        
        // Reuse logic from before but simpler
        // Forward
        SpawnMarker(path.waypoints[fwd].transform.position, () => MovePlayer(playerIndex, diceSideThrown));
        
        // Backward
        // Only if valid
        if (path.isCircular || bwd >= 0)
        {
             SpawnMarker(path.waypoints[bwd].transform.position, () => MovePlayer(playerIndex, -diceSideThrown));
        }
    }
    
    public static void MovePlayer(int playerIndex, int steps)
    {
        ClearMarkers();
        if (playerIndex >= 0 && playerIndex < players.Count)
        {
            players[playerIndex].GetComponent<FollowThePath>().StartMove(steps);
        }
    }
    
    // Legacy mapping if needed? No, updating Dice.cs directly.
    
    // Shortcut Logic
    private static List<Transform> shortcutOptions;
    private bool justHopped = false; // Flag to prevent shortcut loops

    public static void ShowShortcutOptions(int playerIndex, List<Transform> options)
    {
        shortcutOptions = options;
        ClearMarkers();
        for(int i=0; i<options.Count; i++)
        {
            int idx = i; // Closure capture
            SpawnMarker(options[i].position, () => {
                // Hop logic
                 ClearMarkers();
                 if (Instance) Instance.justHopped = true;
                 players[playerIndex].GetComponent<FollowThePath>().StartHop(options[idx]);
            });
        }
    }
    
    // Update Loop
    void Update()
    {
        if (gameOver || players.Count == 0) return;
        
        // Check active player movement
        if (activeMarkers.Count > 0) return; // Waiting for input
        
        UpdateOverlaps();
        
        GameObject p = players[currentPlayerIndex];

        FollowThePath path = p.GetComponent<FollowThePath>();
        
        // If player is NOT moving but Was moving? 
        // We need a proper state machine. 
        // Simple check: if !moveAllowed, we are idle.
        // We need to know if we JUST finished moving.
        // FollowThePath sets moveAllowed = false when done.
        
        // Let's rely on Dice to block input unless ready.
        // Issue: How do we trigger "Turn End"?
        // Quick Fix: FollowThePath can have a callback or we poll.
        // Let's Poll.
        // We need a flag "isTurnInProgress".
    }
    
    // To fix the "Turn End" detection, we can add a method called by FollowThePath?
    // Or just poll in Update: if (wasMoving && !isMoving) -> ReportTurnEnd()
    // Since I can't easily change FollowThePath entirely right now, I'll poll.
    
    bool wasMoving = false;
    void LateUpdate()
    {
        if (players.Count == 0) return;
        GameObject p = players[currentPlayerIndex];
        bool isMoving = p.GetComponent<FollowThePath>().moveAllowed;
        
        if (wasMoving && !isMoving)
        {
            // Just finished
            ReportTurnEnd();
        }
        
        wasMoving = isMoving;
    }

    private void UpdateOverlaps()
    {
        foreach(var p in players) 
        {
             if(p) p.GetComponent<FollowThePath>().isOverlapping = false;
        }

        for (int i = 0; i < players.Count; i++)
        {
            for (int j = i + 1; j < players.Count; j++)
            {
                 if (players[i] == null || players[j] == null) continue;
                 FollowThePath p1 = players[i].GetComponent<FollowThePath>();
                 FollowThePath p2 = players[j].GetComponent<FollowThePath>();
                 
                 if (p1.waypointIndex == p2.waypointIndex)
                 {
                     p1.isOverlapping = true;
                     p2.isOverlapping = true;
                 }
            }
        }
    }
}
