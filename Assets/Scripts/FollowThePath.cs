using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FollowThePath : MonoBehaviour {

    public Transform[] waypoints;

    [Header("Juice Settings")]
    public float wobbleDuration = 0.2f;
    public Vector2 wobbleScale = new Vector2(1.2f, 0.8f); // Squash
    public float bounceHeight = 0.5f;
    public float bounceDuration = 0.5f;

    [Header("Movement Settings")]
    public float moveDuration = 0.5f;
    public float jumpHeight = 2.0f;

    [Header("Visual Settings")]
    public Vector2 playerOffset;
    public bool isOverlapping = false;

    [Header("Board Type")]
    public bool isCircular = false; // Enable for circular boards (trivia mode)
    public Transform startPosition; // For circular boards - separate from waypoints array

    [Header("Interaction Settings")]
    public float interactionBounceHeight = 0.5f;
    public float interactionDuration = 0.4f;

    [HideInInspector]
    public int waypointIndex = 0;

    public bool moveAllowed = false;

    private Vector3 jumpStartPosition; // Renamed to avoid conflict with circular board startPosition
    private float moveTimer;
    private SpriteRenderer sr;
    private Vector3 originalScale; // Store original scale

    private int targetIndex;
    private int stepDirection;
    
    // Hop Animation Flag
    private bool hopping = false;
    private bool isAnimatingJuice = false; // Flag to block movement during juice

    // Use this for initialization
    private void Start () {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Initialize position
        // Initialize position
        // Universal Logic: Always try to start at the dedicated StartPosition (Index -1)
        if (startPosition != null)
        {
            waypointIndex = -1; // Special index meaning "at start"
            transform.position = startPosition.position;
        }
        else
        {
            // Fallback: If no StartPosition assigned, snapping to first waypoint (Index 0)
            Debug.LogWarning("No StartPosition assigned! Defaulting to Waypoint 0.");
            waypointIndex = 0;
            if (waypoints.Length > 0)
            {
               transform.position = waypoints[waypointIndex].transform.position;
            }
        }

        // Ensure we have a collider for OnMouseDown
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }
    
    // Update is called once per frame
    private void Update () {
        if (moveAllowed && !hopping && !isAnimatingJuice)
        {
            Move();
        }
        
        // Apply Offset Logic when Idle
        if (!moveAllowed && !hopping && !isAnimatingJuice && waypointIndex >= 0)
        {
            Vector3 targetPos = waypoints[waypointIndex].transform.position;
            if (isOverlapping)
            {
                targetPos += (Vector3)playerOffset;
            }
            
            // Smoothly move to target or snap? 
            // Snapping ensures they don't drift weirdly, but Lerp is nicer.
            // Let's simple MoveTowards for now to fix small drifts
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);
        }
    }

    public void StartMove(int steps)
    {
        stepDirection = (steps > 0) ? 1 : -1;
        
        // Use the centralized target calculator to ensure we go to the exact same place the animation will
        targetIndex = CalculateTargetIndex(waypointIndex, steps);
        
        Debug.Log($"StartMove: waypointIndex={waypointIndex}, steps={steps}, targetIndex={targetIndex}");
        moveAllowed = true;
    }

    /// <summary>
    /// Calculates the final waypoint index given a start index and number of steps.
    /// Pure index arithmetic. Returns < 0 if destination is invalid (Start or before).
    /// </summary>
    public int CalculateTargetIndex(int currentIdx, int steps)
    {
        int absSteps = Mathf.Abs(steps);
        int direction = (steps > 0) ? 1 : -1;
        int simulatedIdx = currentIdx;
        int currentSimDir = direction;

        // Special case: Move from Start Position (-1)
        if (simulatedIdx < 0)
        {
            // First step takes us to 0 (if forward) or further back (if backward)
            simulatedIdx += currentSimDir;
            absSteps--; 
        }

        // Simulate remaining steps
        for (int i = 0; i < absSteps; i++)
        {
            simulatedIdx += currentSimDir;

            if (isCircular)
            {
                if (simulatedIdx < 0) simulatedIdx = waypoints.Length - 1;
                else if (simulatedIdx >= waypoints.Length) simulatedIdx = 0;
            }
            else
            {
                // Linear Simple Win Logic
                if (simulatedIdx >= waypoints.Length)
                {
                    // Overshot the end. Clamp to Last Waypoint.
                    simulatedIdx = waypoints.Length - 1;
                    // We assume we don't want to count any further steps (just stop at end)
                    break; 
                }
                
                // Note: We allow simulatedIdx to go < 0 here. 
                // This indicates "Back to Start" or "Past Start".
            }
        }

        return simulatedIdx;
    }

    private void Move()
    {
        // Normal movement (step by step through waypoints)
        if (waypointIndex != targetIndex)
        {
            // Initialize jump start for the next single step
            if (moveTimer == 0f)
            {
                // Play jump sound at start of each hop
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayJumpSound();
                }
                
                jumpStartPosition = transform.position;
                
                // Determine next immediate waypoint based on direction
                int nextStepIndex = waypointIndex + stepDirection;
                
                if (isCircular)
                {
                    if (nextStepIndex < 0) nextStepIndex = waypoints.Length - 1;
                    if (nextStepIndex >= waypoints.Length) nextStepIndex = 0;
                }
                else
                {
                     // Linear Simple Win Check
                     if (nextStepIndex >= waypoints.Length)
                     {
                         // Hit end or past end. Clamp to last.
                         nextStepIndex = waypoints.Length - 1;
                     }
                     // Use visual clamping for safety
                     if (nextStepIndex < 0) nextStepIndex = 0;
                }
                
                Vector3 endPos = waypoints[nextStepIndex].transform.position;
                
                if (endPos.x < jumpStartPosition.x)
                {
                    sr.flipX = true;
                }
                else
                {
                    sr.flipX = false;
                }
            }

            moveTimer += Time.deltaTime;
            float t = moveTimer / moveDuration;

            if (t >= 1f)
            {
                // Finish jump (single step)
                waypointIndex += stepDirection;
                
                // Handle circular wrapping
                if (isCircular)
                {
                    if (waypointIndex < 0) waypointIndex = waypoints.Length - 1;
                    if (waypointIndex >= waypoints.Length) waypointIndex = 0;
                }
                else
                {
                     // Linear Simple Win Update
                     if (waypointIndex >= waypoints.Length)
                     {
                         waypointIndex = waypoints.Length - 1;
                     }
                     if (waypointIndex < 0) waypointIndex = 0;
                }
                
                // Check Skip (Universal)
                // Note: Logic above already handled skip direction for the next hop, 
                // but we need to ensure our *current* resting index is valid if we landed on a StartWaypoint
                // (This creates a multi-hop effect if multiple start waypoints existed, though unlikely)
                if (waypoints[waypointIndex].GetComponent<StartWaypoint>() != null)
                {
                    Debug.Log($"Skipping StartWaypoint at index {waypointIndex}, jumping to next");
                    waypointIndex += stepDirection;
                    
                    // Wrap/Clamp again
                    if (isCircular)
                    {
                        if (waypointIndex < 0) waypointIndex = waypoints.Length - 1;
                        if (waypointIndex >= waypoints.Length) waypointIndex = 0;
                    }
                    else
                    {
                        // Bounce check again for safety (visual only)
                        if (waypointIndex >= waypoints.Length) 
                        {
                            waypointIndex = waypoints.Length - 2;
                            stepDirection = -1; 
                        }
                        if (waypointIndex < 0) waypointIndex = 0;
                    }
                }
                
                transform.position = waypoints[waypointIndex].transform.position;
                moveTimer = 0f; // Reset for next jump

                // Check if we reached the final target
                if (waypointIndex == targetIndex)
                {
                    StartCoroutine(FinalBounce());
                }
                else
                {
                    StartCoroutine(LandWobble());
                }
            }
            else
            {
                // Parabolic interpolation towards the next immediate waypoint
                int nextStepIndex = waypointIndex + stepDirection;
                
                // Wrap for circular boards
                if (isCircular)
                {
                    if (nextStepIndex < 0) nextStepIndex = waypoints.Length - 1;
                    if (nextStepIndex >= waypoints.Length) nextStepIndex = 0;
                }
                else
                {
                    // Linear clamping for interpolation
                    if (nextStepIndex >= waypoints.Length) nextStepIndex = waypoints.Length - 1;
                    if (nextStepIndex < 0) nextStepIndex = 0;
                }
                
                Vector3 endPosition = waypoints[nextStepIndex].transform.position;
                
                Vector3 linearPos = Vector3.Lerp(jumpStartPosition, endPosition, t);
                
                // Add jump height (Sine wave 0->1->0)
                float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                
                transform.position = new Vector3(linearPos.x, linearPos.y + height, linearPos.z);
            }
        }
    }

    private System.Collections.IEnumerator LandWobble()
    {
        isAnimatingJuice = true;
        float timer = 0f;
        
        // Squash
        while (timer < wobbleDuration)
        {
            timer += Time.deltaTime;
            float t = timer / wobbleDuration;
            
            // Sine wave for scale: 0 -> 1 -> 0 (applied to difference)
            // Or simpler: Go to Squash target then back to Original
            // Let's use a nice sine curve for smooth in/out
            float scaleFactor = Mathf.Sin(t * Mathf.PI); 
            
            // Lerp between original and wobbleScale based on scaleFactor
            Vector3 targetScale = new Vector3(
                Mathf.Lerp(originalScale.x, originalScale.x * wobbleScale.x, scaleFactor),
                Mathf.Lerp(originalScale.y, originalScale.y * wobbleScale.y, scaleFactor),
                originalScale.z
            );
            
            transform.localScale = targetScale;
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimatingJuice = false;
    }

    private System.Collections.IEnumerator FinalBounce()
    {
        isAnimatingJuice = true; // Still blocking main Move loop
        // NOTE: We do NOT set moveAllowed = false yet, so GameControl waits.
        
        Vector3 groundPos = transform.position;
        float timer = 0f;

        while (timer < bounceDuration)
        {
            timer += Time.deltaTime;
            float t = timer / bounceDuration;
            
            // Small Hop
            float height = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            transform.position = new Vector3(groundPos.x, groundPos.y + height, groundPos.z);
            
            yield return null;
        }

        transform.position = groundPos;
        isAnimatingJuice = false;
        moveAllowed = false; // Now we are truly done
    }

    public void StartHop(Transform target)
    {
        // Find target index first
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == target)
            {
                // Play jump sound when hopping to shortcut
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayJumpSound();
                }
                
                hopping = true;
                moveAllowed = true; // Ensure Update doesn't block other things if needed, but we used !hopping check
                StartCoroutine(HopTo(i, target.position));
                return;
            }
        }
    }

    private System.Collections.IEnumerator HopTo(int newIndex, Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float timer = 0f;
        
        // Use moveDuration or a custom hop duration (e.g. 1 second for long jumps)
        float duration = 1.0f; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Parabolic Jump
            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight * 2; // Higher jump for shortcuts?
            transform.position = new Vector3(linearPos.x, linearPos.y + height, linearPos.z);

            yield return null;
        }

        transform.position = targetPos;
        waypointIndex = newIndex;
        hopping = false;
        moveAllowed = false; // Stop movement state
    }

}
