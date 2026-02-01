using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dice : MonoBehaviour {

    private Sprite[] diceSides;
    
    [Header("Visual References")]
    public SpriteRenderer dice2DSprite;
    public GameObject dice3DObject; // Drag the 3D Dice here

    [Header("Settings")]
    public bool use3DPhysics = true;
    public bool enableDebugInput = true;

    private int whosTurn = 1;
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
        if (enableDebugInput && Keyboard.current != null)
        {
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame) { debugRollValue = 1; Debug.Log("Debug Roll: 1"); }
            if (Keyboard.current[Key.Digit2].wasPressedThisFrame) { debugRollValue = 2; Debug.Log("Debug Roll: 2"); }
            if (Keyboard.current[Key.Digit3].wasPressedThisFrame) { debugRollValue = 3; Debug.Log("Debug Roll: 3"); }
            if (Keyboard.current[Key.Digit4].wasPressedThisFrame) { debugRollValue = 4; Debug.Log("Debug Roll: 4"); }
            if (Keyboard.current[Key.Digit5].wasPressedThisFrame) { debugRollValue = 5; Debug.Log("Debug Roll: 5"); }
            if (Keyboard.current[Key.Digit6].wasPressedThisFrame) { debugRollValue = 6; Debug.Log("Debug Roll: 6"); }
        }
    }

    private void OnMouseDown()
    {
        // If using 3D physics, the 3D object handles the click via Dice3D.
        // We ignore the click on the parent 2D object to prevent double-activation.
        if (use3DPhysics && dice3DObject != null)
        {
             if (dice3DObject.activeSelf == false) dice3DObject.SetActive(true);
             return; 
        }

        if (!GameControl.gameOver && coroutineAllowed)
        {
            StartCoroutine("RollTheDice");
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
        
        if (whosTurn == 1)
        {
            GameControl.ShowDirectionOptions(1);
        } else if (whosTurn == -1)
        {
            GameControl.ShowDirectionOptions(2);
        }
        whosTurn *= -1;
    }

    public void ResetDice()
    {
        coroutineAllowed = true;
    }

    public void SetTurn(int turn)
    {
        whosTurn = turn;
    }

    public bool IsCoroutineAllowed()
    {
        return coroutineAllowed;
    }
}
