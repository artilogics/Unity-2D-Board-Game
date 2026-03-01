using UnityEngine;
using System.Collections;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance;

    [Header("Spin Settings")]
    [Tooltip("How long the initial 360 spin takes")]
    public float initialSpinDuration = 3f;
    [Tooltip("How long rotating to face a player takes")]
    public float focusSpinDuration = 1.5f;

    [Header("References")]
    [Tooltip("The main camera (or camera pivot) to align the player with")]
    public Transform cameraReference;
    
    // Internal state
    public bool IsSpinning { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (cameraReference == null && Camera.main != null)
        {
            cameraReference = Camera.main.transform;
        }
    }

    /// <summary>
    /// Spins the board 360 degrees and slows down.
    /// </summary>
    public void SpinToStart(System.Action onComplete = null)
    {
        StartCoroutine(SpinRoutine(360f, initialSpinDuration, onComplete));
    }

    /// <summary>
    /// Rotates the board so the player is aligned with the camera's forward.
    /// </summary>
    public void RotateToFacePlayer(Transform player, System.Action onComplete = null)
    {
        if (player == null || cameraReference == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Calculate vector from board center to the player
        Vector3 centerToPlayer = player.position - transform.position;
        centerToPlayer.y = 0; // Ignore height for Y rotation
        
        // Calculate the target direction we want the player to face towards
        // Usually, we want the player closest to the camera. 
        // So the vector from center to player should point matching the camera's backward vector (towards the camera),
        // or just align with World Forward (0,0,-1) if the camera is looking down the Z axis.
        
        // Let's align the player to point towards the camera in the XZ plane
        Vector3 cameraForwardXZ = cameraReference.forward;
        cameraForwardXZ.y = 0;
        
        // We actually want the player between the camera and the board, so the vector from center to player 
        // should be the OPPOSITE of the camera's forward.
        Vector3 targetDirection = -cameraForwardXZ.normalized;
        
        if (targetDirection == Vector3.zero) 
        {
            // Fallback if camera is perfectly top-down
            targetDirection = -cameraReference.up;
            targetDirection.y = 0;
        }

        // What's the current angle between our player direction and the target direction?
        float angleToRotate = Vector3.SignedAngle(centerToPlayer, targetDirection, Vector3.up);

        StartCoroutine(SpinRoutine(angleToRotate, focusSpinDuration, onComplete));
    }

    public void RotateToFacePlayerWithExtraSpin(Transform player, int extraSpins, System.Action onComplete = null)
    {
        if (player == null || cameraReference == null)
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 centerToPlayer = player.position - transform.position;
        centerToPlayer.y = 0; 
        
        Vector3 cameraForwardXZ = cameraReference.forward;
        cameraForwardXZ.y = 0;
        
        Vector3 targetDirection = -cameraForwardXZ.normalized;
        if (targetDirection == Vector3.zero) 
        {
            targetDirection = -cameraReference.up;
            targetDirection.y = 0;
        }

        float angleToRotate = Vector3.SignedAngle(centerToPlayer, targetDirection, Vector3.up);
        
        // Ensure it always spins consistently (e.g. clockwise)
        if (angleToRotate < 0) angleToRotate += 360f;
        
        angleToRotate += 360f * extraSpins;

        StartCoroutine(SpinRoutine(angleToRotate, initialSpinDuration, onComplete));
    }

    private IEnumerator SpinRoutine(float angleOffset, float duration, System.Action onComplete)
    {
        IsSpinning = true;
        
        Vector3 startEuler = transform.eulerAngles;
        float startY = startEuler.y;
        float targetY = startY + angleOffset;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // Ease out cubic: f(t) = 1 - (1 - t)^3
            float t = elapsed / duration;
            if (t > 1f) t = 1f;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            float currentY = Mathf.Lerp(startY, targetY, easeT);
            transform.rotation = Quaternion.Euler(startEuler.x, currentY, startEuler.z);
            
            yield return null;
        }
        
        transform.rotation = Quaternion.Euler(startEuler.x, targetY, startEuler.z); 
        IsSpinning = false;
        
        onComplete?.Invoke();
    }
}
