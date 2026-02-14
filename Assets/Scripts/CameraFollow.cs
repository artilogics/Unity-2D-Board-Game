using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -10); // Standard ISO-like view
    public float zoomLevel = 10f;
    
    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomSpeed = 2f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target == null)
        {
            // Try to find a player if none assigned
            GameObject p1 = GameObject.Find("Player1");
            if (p1) target = p1.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Optional LookAt (if we want to keep target centered)
        // transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
