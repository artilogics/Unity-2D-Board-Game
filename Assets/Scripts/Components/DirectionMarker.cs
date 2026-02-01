using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DirectionMarker : MonoBehaviour
{
    private Action onClickCallback;
    private Vector3 startPos;
    private float floatSpeed = 2.0f;
    private float floatHeight = 0.25f;

    public void Setup(Action callback)
    {
        this.onClickCallback = callback;
    }

    void Start()
    {
        startPos = transform.position;
        
        // Ensure we have a collider for raycasts (check 2D then 3D)
        if (GetComponent<Collider2D>() == null && GetComponent<Collider>() == null)
        {
            // Default to BoxCollider2D if nothing exists (assuming 2D context default, strict 3D users should add collider)
             // But let's check if we are 2D or 3D based on SpriteRenderer vs MeshRenderer?
             // Simple fallback: Add BoxCollider2D.
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f); 
        }
    }

    void Update()
    {
        // 1. Floating Animation (Bob up and down)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // 2. Input Detection
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckInput(Mouse.current.position.ReadValue());
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckInput(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void CheckInput(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        bool hitSelf = false;

        // 1. 2D Raycast
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
        if (hit2D.collider != null && hit2D.collider.gameObject == gameObject)
        {
            hitSelf = true;
        }

        // 2. 3D Raycast
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
            // Clicked!
            Debug.Log($"[DirectionMarker] Clicked! Invoking callback. Callback exists: {onClickCallback != null}");
            
            if (onClickCallback != null)
            {
                onClickCallback.Invoke();
            }
            
            // Destroy self to prevent double clicks (GameControl will clear others)
            Destroy(gameObject); 
        }
    }
}
