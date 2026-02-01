using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectBounceFeedback : MonoBehaviour {

    [Header("Interaction Settings")]
    public float bounceHeight = 0.5f;
    public float bounceDuration = 0.4f;
    public bool enableSquash = true;
    public Vector2 squashScale = new Vector2(1.2f, 0.8f);

    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Start()
    {
        originalScale = transform.localScale;
        
        // Auto-add collider if missing (checks both 2D and 3D)
        Collider2D col2d = GetComponent<Collider2D>();
        Collider col3d = GetComponent<Collider>();

        if (col2d == null && col3d == null)
        {
            Debug.LogWarning($"Object {name} missing Collider (2D or 3D) for interaction! Adding BoxCollider2D default.");
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void Update()
    {
        // Don't detect input while animating
        if (isAnimating) return;

        // Mouse Input
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckInput(Mouse.current.position.ReadValue(), "Mouse");
        }

        // Touch Input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckInput(Touchscreen.current.primaryTouch.position.ReadValue(), "Touch");
        }
    }

    private void CheckInput(Vector2 screenPos, string source)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        bool hitSelf = false;

        // 1. Try 2D Raycast (GetRayIntersection covers Z-depth for 2D objects)
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
        if (hit2D.collider != null && hit2D.collider.gameObject == gameObject)
        {
            hitSelf = true;
        }

        // 2. Try 3D Raycast (if 2D didn't hit)
        if (!hitSelf)
        {
            RaycastHit hit3D;
            if (Physics.Raycast(ray, out hit3D))
            {
                if (hit3D.collider != null && hit3D.collider.gameObject == gameObject)
                {
                    hitSelf = true;
                }
            }
        }

        if (hitSelf)
        {
            StartCoroutine(AnimateBounce());
        }
    }

    private IEnumerator AnimateBounce()
    {
        isAnimating = true;
        originalPosition = transform.position;
        float timer = 0f;

        while (timer < bounceDuration)
        {
            timer += Time.deltaTime;
            float t = timer / bounceDuration;

            // Bounce (Sine Wave)
            float height = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            transform.position = new Vector3(originalPosition.x, originalPosition.y + height, originalPosition.z);

            // Squash (optional)
            if (enableSquash)
            {
                float squashFactor = Mathf.Sin(t * Mathf.PI);
                // Apply squash to X and Y, preserve Z
                // Note: accurate 3D squash might need Volume preservation (scale Z too), 
                // but this 2D-style squash is usually "cute" enough for top-down 3D too.
                Vector3 targetScaleVector = new Vector3(
                    Mathf.Lerp(originalScale.x, originalScale.x * squashScale.x, squashFactor),
                    Mathf.Lerp(originalScale.y, originalScale.y * squashScale.y, squashFactor),
                    originalScale.z
                );
                transform.localScale = targetScaleVector;
            }

            yield return null;
        }

        // Reset to exact originals
        transform.position = originalPosition;
        transform.localScale = originalScale;
        isAnimating = false;
    }
}
