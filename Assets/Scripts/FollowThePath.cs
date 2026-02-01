using UnityEngine;
using UnityEngine.InputSystem;

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

    private Vector2 jumpStartPosition; // Renamed to avoid conflict with circular board startPosition
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
        if (isCircular && startPosition != null)
        {
            // Circular board - start at separate start position
            waypointIndex = -1; // Special index meaning "at start"
            transform.position = startPosition.position;
        }
        else
        {
            // Linear board or circular without startPosition - start at waypoint 0
            transform.position = waypoints[waypointIndex].transform.position;
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
        
        // Check if we're at the start position (waypointIndex = -1 for circular start)
        bool atStartPosition = (waypointIndex == -1);
        
        if (isCircular && atStartPosition)
        {
            // First roll from start - map dice value to 0-indexed waypoints
            // Roll 1 forward → waypoints[0] (1st tile counting forward)
            // Roll 1 backward → waypoints[31] (1st tile counting backward, assuming 32 waypoints)
            // Roll 3 forward → waypoints[2] (3rd tile)
            // Roll 3 backward → waypoints[29] (3rd tile from end)
            
            targetIndex = (stepDirection > 0) ? (steps - 1) : (waypoints.Length + steps);
            
            // Ensure in bounds
            while (targetIndex < 0) targetIndex += waypoints.Length;
            targetIndex = targetIndex % waypoints.Length;
        }
        else
        {
            // Normal movement from a waypoint
            targetIndex = waypointIndex + steps;
            
            if (isCircular)
            {
                // Wrap around for circular boards
                while (targetIndex < 0) targetIndex += waypoints.Length;
                targetIndex = targetIndex % waypoints.Length;
            }
            else
            {
                // Clamp for linear boards
                if (targetIndex < 0) targetIndex = 0;
                if (targetIndex >= waypoints.Length) targetIndex = waypoints.Length - 1;
            }
        }
        
        Debug.Log($"StartMove: waypointIndex={waypointIndex}, steps={steps}, targetIndex={targetIndex}");
        moveAllowed = true;
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
                
                // Handle circular wrapping
                if (isCircular)
                {
                    if (nextStepIndex < 0) nextStepIndex = waypoints.Length - 1;
                    if (nextStepIndex >= waypoints.Length) nextStepIndex = 0;
                    
                    // Skip StartWaypoint in circular mode
                    if (waypoints[nextStepIndex].GetComponent<StartWaypoint>() != null)
                    {
                        Debug.Log($"Skipping StartWaypoint at index {nextStepIndex}");
                        nextStepIndex += stepDirection;
                        
                        // Wrap again if needed
                        if (nextStepIndex < 0) nextStepIndex = waypoints.Length - 1;
                        if (nextStepIndex >= waypoints.Length) nextStepIndex = 0;
                    }
                }
                
                Vector2 endPos = waypoints[nextStepIndex].transform.position;
                
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
                    
                    // Skip StartWaypoint
                    if (waypoints[waypointIndex].GetComponent<StartWaypoint>() != null)
                    {
                        Debug.Log($"Skipping StartWaypoint at index {waypointIndex}, jumping to next");
                        waypointIndex += stepDirection;
                        
                        // Wrap again if needed
                        if (waypointIndex < 0) waypointIndex = waypoints.Length - 1;
                        if (waypointIndex >= waypoints.Length) waypointIndex = 0;
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
                
                Vector2 endPosition = waypoints[nextStepIndex].transform.position;
                
                Vector2 linearPos = Vector2.Lerp(jumpStartPosition, endPosition, t);
                
                // Add jump height (Sine wave 0->1->0)
                float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                
                transform.position = new Vector2(linearPos.x, linearPos.y + height);
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
