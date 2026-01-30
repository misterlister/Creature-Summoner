using System;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask battleLayer;
    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;
    private Animator animator;
    private MapArea currentMapArea;

    const float DEADZONE = 0.19f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");

            // Apply dead zone to both x and y input
            if (Mathf.Abs(input.x) < DEADZONE) input.x = 0f;
            if (Mathf.Abs(input.y) < DEADZONE) input.y = 0f;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                // Calculate target position as Vector2
                var targetPos = (Vector2)transform.position + input;
                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()
    {
        // Check if we're in a battle zone
        var battleZone = Physics2D.OverlapCircle(transform.position, 0.2f, battleLayer);
        if (battleZone != null)
        {
            // Get or update current map area
            var mapArea = battleZone.GetComponent<MapArea>();
            if (mapArea != null)
            {
                if (currentMapArea != mapArea)
                {
                    // Entered a new map area
                    currentMapArea = mapArea;
                    currentMapArea.OnPlayerEnter();
                }

                // Notify map area of step and check if encounter triggers
                if (currentMapArea.OnPlayerStep())
                {
                    animator.SetBool("isMoving", false);
                    OnEncountered();
                }
            }
        }
        else
        {
            // Left the map area
            if (currentMapArea != null)
            {
                currentMapArea.OnPlayerExit();
                currentMapArea = null;
            }
        }
    }

    /// <summary>
    /// Get the current map area the player is in
    /// </summary>
    public MapArea GetCurrentMapArea()
    {
        return currentMapArea;
    }
}