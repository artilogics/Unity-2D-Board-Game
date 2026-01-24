using UnityEngine;

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

    [Header("Interaction Settings")]
    public float interactionBounceHeight = 0.5f;
    public float interactionDuration = 0.4f;

    [HideInInspector]
    public int waypointIndex = 0;

    public bool moveAllowed = false;

    private Vector2 startPosition;
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
        transform.position = waypoints[waypointIndex].transform.position; // Start centered, update loop will fix if overlapping

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
        if (!moveAllowed && !hopping && !isAnimatingJuice)
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
        targetIndex = waypointIndex + steps;
        // Clamp logic can be added here if needed to prevent going out of bounds
        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex >= waypoints.Length) targetIndex = waypoints.Length - 1;
        
        moveAllowed = true;
    }

    private void Move()
    {
        if (waypointIndex != targetIndex)
        {
            // Initialize jump start for the next single step
            if (moveTimer == 0f)
            {
                startPosition = transform.position;
                
                // Determine next immediate waypoint based on direction
                int nextStepIndex = waypointIndex + stepDirection;
                Vector2 endPos = waypoints[nextStepIndex].transform.position;
                
                if (endPos.x < startPosition.x)
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
                Vector2 endPosition = waypoints[nextStepIndex].transform.position;
                
                Vector2 linearPos = Vector2.Lerp(startPosition, endPosition, t);
                
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
    // Interaction Logic
    private void OnMouseDown()
    {
        // Only allow interaction if idle
        if (!moveAllowed && !isAnimatingJuice && !hopping)
        {
            StartCoroutine(InteractionBounce());
        }
    }

    private System.Collections.IEnumerator InteractionBounce()
    {
        isAnimatingJuice = true;
        Vector3 groundPos = transform.position;
        float timer = 0f;

        while (timer < interactionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / interactionDuration;

            // Simple Hop
            float height = Mathf.Sin(t * Mathf.PI) * interactionBounceHeight;
            transform.position = new Vector3(groundPos.x, groundPos.y + height, groundPos.z);

            yield return null;
        }

        transform.position = groundPos;
        isAnimatingJuice = false;
    }
}
