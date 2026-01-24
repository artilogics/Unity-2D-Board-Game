using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour {

    private Sprite[] diceSides;
    private SpriteRenderer rend;
    private int whosTurn = 1;
    private bool coroutineAllowed = true;
    
    public bool enableDebugInput = true;
    public static int debugRollValue = 0;

	// Use this for initialization
	private void Start () {
        rend = GetComponent<SpriteRenderer>();
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
        rend.sprite = diceSides[5];
	}

    private void Update()
    {
        if (enableDebugInput)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) { debugRollValue = 1; Debug.Log("Debug Roll: 1"); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { debugRollValue = 2; Debug.Log("Debug Roll: 2"); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { debugRollValue = 3; Debug.Log("Debug Roll: 3"); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { debugRollValue = 4; Debug.Log("Debug Roll: 4"); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { debugRollValue = 5; Debug.Log("Debug Roll: 5"); }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { debugRollValue = 6; Debug.Log("Debug Roll: 6"); }
        }
    }

    private void OnMouseDown()
    {
        if (!GameControl.gameOver && coroutineAllowed)
            StartCoroutine("RollTheDice");
    }

    private IEnumerator RollTheDice()
    {
        coroutineAllowed = false;
        int randomDiceSide = 0;
        for (int i = 0; i <= 20; i++)
        {
            randomDiceSide = Random.Range(0, 6);
            rend.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }

        // DEBUG OVERRIDE
        if (debugRollValue > 0)
        {
            randomDiceSide = debugRollValue - 1;
            debugRollValue = 0; // Reset after usage
            rend.sprite = diceSides[randomDiceSide]; // Ensure visual matches
        }

        GameControl.diceSideThrown = randomDiceSide + 1;
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
}
