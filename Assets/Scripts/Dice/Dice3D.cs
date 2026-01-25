using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice3D : MonoBehaviour {

	static Rigidbody rb;
	public static Vector3 diceVelocity;

    public Dice parentDice; // Reference to main controller
    public bool isRolling = false;

    [Header("Gravity Settings")]
    public bool useCustomGravity = true;
    public Vector3 customGravity = new Vector3(0, 0, 9.81f); // Default to Z-forward for 2D games

    [Header("Anti-Cocked Settings")]
    public GameObject[] wallsToIgnore;
    private bool touchingWall = false;
    private int cockedRetryCount = 0;
    private bool isRetrying = false;
    private float rollStartTime = 0f;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody> ();
        rb.useGravity = !useCustomGravity; 
        rb.isKinematic = true; // Wait for click
        
        if (parentDice == null) parentDice = FindObjectOfType<Dice>();
    }
    
    void FixedUpdate () {
        if (rb != null && !rb.isKinematic)
        {
            diceVelocity = rb.linearVelocity;
            
            if (useCustomGravity)
            {
                rb.AddForce(customGravity * rb.mass);
            }
        }
    }

    void Update() {
        // Only check for stuck AFTER dice has had time to roll (not immediately after throw)
        float timeSinceRoll = Time.time - rollStartTime;
        
        if (isRolling && !rb.isKinematic && timeSinceRoll > 1.0f && rb.linearVelocity.magnitude < 0.05f) {
            if ((IsCocked() || touchingWall) && !isRetrying) {
                Debug.LogWarning("Dice stuck - disabling walls to free it.");
                DisableWalls();
                isRetrying = true; // Prevent spam
            }
        }
    }
    
    private void DisableWalls() {
        if (wallsToIgnore != null && wallsToIgnore.Length > 0) {
            foreach (var wall in wallsToIgnore) {
                if (wall != null) {
                    Collider wallCollider = wall.GetComponent<Collider>();
                    if (wallCollider != null) wallCollider.enabled = false;
                }
            }
        }
    }
    
    public void EnableWalls() {
        if (wallsToIgnore != null && wallsToIgnore.Length > 0) {
            Debug.Log("Re-enabling walls.");
            foreach (var wall in wallsToIgnore) {
                if (wall != null) {
                    Collider wallCollider = wall.GetComponent<Collider>();
                    if (wallCollider != null) wallCollider.enabled = true;
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (parentDice != null && !parentDice.IsCoroutineAllowed()) return;
        
        // Grab the dice - move to spawn and freeze it there
        Debug.Log("Grabbed dice - moving to spawn point");
        
        rb.isKinematic = true; // Freeze it
        
        // Move to spawn
        if (spawnPoint != null) transform.position = spawnPoint.position;
        else transform.position = new Vector3 (0, 2, 0);
        
        transform.rotation = Random.rotation; // Random rotation!
        
        // Re-enable walls NOW (while frozen, safe)
        EnableWalls();
        
        // Reset state
        cockedRetryCount = 0;
        isRetrying = false;
    }
    
    private void OnMouseUp()
    {
        if (parentDice != null && !parentDice.IsCoroutineAllowed()) return;
        
        // Throw the dice
        Debug.Log("Released dice - throwing!");
        RollDice();
    }

    [Header("Toss Settings")]
    public Transform spawnPoint; 
    public float upForce = 10f; 
    public float torqueAmount = 50f; 

    public void RollDice() {
        // Dice is already positioned at spawn by OnMouseDown
        // Just need to enable physics and apply forces
        
        isRolling = true;
        rollStartTime = Time.time; // Record when throw started
        
        // Enable physics
        rb.isKinematic = false;
        
        // Clear velocities
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Apply forces
        float dirX = Random.Range (0, torqueAmount);
        float dirY = Random.Range (0, torqueAmount);
        float dirZ = Random.Range (0, torqueAmount);
        
        rb.AddForce (transform.up * upForce, ForceMode.Impulse); 
        rb.AddTorque (dirX, dirY, dirZ, ForceMode.Impulse);
        
        Debug.Log("Dice thrown!");
    }

    // --- Anti-Cocked Helpers ---

    private bool IsCocked() {
        Vector3 upDir = -customGravity.normalized; // "Up" is opposite of gravity
        if (!useCustomGravity) upDir = Vector3.up;

        float bestDot = -1f;
        Vector3[] sideDirections = new Vector3[] {
            Vector3.up, Vector3.forward, Vector3.left, Vector3.right, Vector3.back, Vector3.down
        };
        
        for (int i = 0; i < 6; i++) {
            Vector3 worldDir = transform.TransformDirection(sideDirections[i]);
            float dot = Vector3.Dot(worldDir, upDir);
            if (dot > bestDot) bestDot = dot;
        }
        return bestDot < 0.8f; 
    }

    private void OnCollisionEnter(Collision collision) {
        if (wallsToIgnore != null) {
            foreach (var wall in wallsToIgnore) {
                if (wall != null && collision.gameObject == wall) touchingWall = true;
            }
        }
    }
    
    private void OnCollisionExit(Collision collision) {
         if (wallsToIgnore != null) {
            foreach (var wall in wallsToIgnore) {
                if (wall != null && collision.gameObject == wall) touchingWall = false;
            }
        }
    }
}
