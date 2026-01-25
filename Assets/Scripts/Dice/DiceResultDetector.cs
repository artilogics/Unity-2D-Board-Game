using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceResultDetector : MonoBehaviour {

    public Dice parentDice;
    private bool resultReported = false;
    private float stillTimer = 0f;
    private float requiredStillTime = 0.3f; // Must be still for 0.3 seconds
    private int pendingResult = 0;

    void Start() {
        parentDice = FindObjectOfType<Dice>();
    }

	// Update is called once per frame
	void FixedUpdate () {
        // Only reset if moving significantly (actual toss/bounce), not just jitter
        if (Dice3D.diceVelocity.magnitude > 1.0f)
        {
            resultReported = false;
            stillTimer = 0f;
            pendingResult = 0;
        }
	}

	void OnTriggerStay(Collider col)
	{
		// Check if nearly stopped
		if (Dice3D.diceVelocity.magnitude <= 0.1f && !resultReported)
		{
            int result = 0;
            
            // Uncomment to debug which collider is touching:
            // Debug.Log("Detecting side: " + col.gameObject.name);

			switch (col.gameObject.name) {
			case "Side1": result = 6; break;
			case "Side2": result = 5; break;
			case "Side3": result = 4; break;
			case "Side4": result = 3; break;
			case "Side5": result = 2; break;
			case "Side6": result = 1; break;
			}
            
            if (result != 0)
            {
                // Track if same result is stable
                if (pendingResult == result)
                {
                    stillTimer += Time.fixedDeltaTime;
                    
                    // Only report if been still with same result for required time
                    if (stillTimer >= requiredStillTime)
                    {
                        if (parentDice != null)
                        {
                            if (!parentDice.IsCoroutineAllowed()) return; 

                            Debug.Log("Dice Result Final: " + result + " (verified after " + stillTimer.ToString("F2") + "s)");
                            parentDice.ProcessDiceResult(result);
                            resultReported = true;
                            
                            // Re-enable walls now that dice has settled and reported
                            Dice3D diceScript = FindObjectOfType<Dice3D>();
                            if (diceScript != null) diceScript.EnableWalls();
                        }
                    }
                }
                else
                {
                    // Result changed, reset timer
                    pendingResult = result;
                    stillTimer = 0f;
                }
            }
		}
        else
        {
            // Moving too fast, reset
            stillTimer = 0f;
            pendingResult = 0;
        }
	}
}
