using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Dice : MonoBehaviour {

    private Sprite[] diceSides;
    
    [Header("Visual References")]
    public SpriteRenderer dice2DSprite;
    public GameObject dice3DObject; // Drag the 3D Dice here

    [Header("Settings")]
    public bool use3DPhysics = true;
    public bool enableDebugInput = true;

    private int currentPlayerIndex = 0;
    private bool coroutineAllowed = true;
    public static int debugRollValue = 0;

	// Use this for initialization
	private void Start () {
        // Fallback for existing setup
        if (dice2DSprite == null) dice2DSprite = GetComponent<SpriteRenderer>();
        
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
        
        // Initialize Visuals
        if (dice2DSprite != null)
        {
            dice2DSprite.sprite = diceSides[5];
            dice2DSprite.enabled = !use3DPhysics; // Hide 2D if using 3D
        }
        
        if (dice3DObject != null)
        {
            dice3DObject.SetActive(use3DPhysics); // Show/Hide 3D
        }
	}

    private void Update()
    {
        // Debug input
        if (enableDebugInput && Keyboard.current != null)
        {
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame) { debugRollValue = 1; Debug.Log("Debug Roll: 1"); }
            if (Keyboard.current[Key.Digit2].wasPressedThisFrame) { debugRollValue = 2; Debug.Log("Debug Roll: 2"); }
            if (Keyboard.current[Key.Digit3].wasPressedThisFrame) { debugRollValue = 3; Debug.Log("Debug Roll: 3"); }
            if (Keyboard.current[Key.Digit4].wasPressedThisFrame) { debugRollValue = 4; Debug.Log("Debug Roll: 4"); }
            if (Keyboard.current[Key.Digit5].wasPressedThisFrame) { debugRollValue = 5; Debug.Log("Debug Roll: 5"); }
            if (Keyboard.current[Key.Digit6].wasPressedThisFrame) { debugRollValue = 6; Debug.Log("Debug Roll: 6"); }
        }

        // Mouse/Pointer input (works in editor and with mouse on device)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            CheckClickAtPosition(mousePosition, "Mouse");
        }

        // Mobile touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            CheckClickAtPosition(touchPosition, "Touch");
        }
    }

    private void CheckClickAtPosition(Vector2 screenPosition, string inputType)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        // Use GetRayIntersectionAll to pierce through blocking player colliders
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);

        foreach(var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                HandleDiceClick();
                break; // Found the dice, stop checking
            }
        }
    }

    private void HandleDiceClick()
    {
        Debug.Log("HandleDiceClick() called");
        Debug.Log($"use3DPhysics: {use3DPhysics}, dice3DObject: {(dice3DObject != null ? "exists" : "null")}");
        Debug.Log($"GameControl.gameOver: {GameControl.gameOver}, coroutineAllowed: {coroutineAllowed}");
        
        // If using 3D physics, the 3D object handles the click via Dice3D.
        // We ignore the click on the parent 2D object to prevent double-activation.
        if (use3DPhysics && dice3DObject != null)
        {
             if (!coroutineAllowed)
             {
                 Debug.LogWarning("Dice click blocked (3D)! coroutineAllowed=false");
                 return;
             }

             Debug.Log("Using 3D physics - activating 3D dice object");
             if (dice3DObject.activeSelf == false) dice3DObject.SetActive(true);
             return; 
        }

        if (!GameControl.gameOver && coroutineAllowed)
        {
            Debug.Log("Starting RollTheDice coroutine!");
            StartCoroutine("RollTheDice");
        }
        else
        {
            Debug.LogWarning($"Dice click blocked! gameOver={GameControl.gameOver}, coroutineAllowed={coroutineAllowed}");
        }
    }

    // Called by Dice3D when it lands
    public void ProcessDiceResult(int resultSide)
    {
        if (GameControl.gameOver) return;

        // Visual Override (Debug)
        if (debugRollValue > 0)
        {
             resultSide = debugRollValue;
             debugRollValue = 0;
             Debug.Log("Debug Override (3D): " + resultSide);
        }

        FinalizeTurn(resultSide);
    }

    private IEnumerator RollTheDice()
    {
        coroutineAllowed = false;
        int resultSide = 0;

        // --- 2D MODE (Legacy) ---
        // Switch Visuals
        if (dice2DSprite != null) dice2DSprite.enabled = true;

        int randomDiceSide = 0;
        for (int i = 0; i <= 20; i++)
        {
            randomDiceSide = Random.Range(0, 6);
            if (dice2DSprite != null) dice2DSprite.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }

        // Debug Override
        if (debugRollValue > 0)
        {
            randomDiceSide = debugRollValue - 1;
            debugRollValue = 0;
            if (dice2DSprite != null) dice2DSprite.sprite = diceSides[randomDiceSide];
        }

        resultSide = randomDiceSide + 1;
        FinalizeTurn(resultSide);
    }

    private void FinalizeTurn(int result)
    {
        coroutineAllowed = false; // Block until reset
        GameControl.diceSideThrown = result;
        
        // Use current index
        GameControl.ShowDirectionOptions(currentPlayerIndex);
    }

    public void ResetDice()
    {
        coroutineAllowed = true;
    }

    public void SetTurn(int playerIndex)
    {
        currentPlayerIndex = playerIndex;
    }

    public bool IsCoroutineAllowed()
    {
        return coroutineAllowed;
    }
}
