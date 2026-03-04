using System;
using System.Collections;
using UnityEngine;

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

    private void Start()
    {
        // Detect if player spawns inside a battle zone
        var hit = Physics2D.OverlapCircle(transform.position, 0.2f, battleLayer);
        if (hit != null)
        {
            var trigger = hit.GetComponent<EncounterZoneTrigger>();
            if (trigger != null)
            {
                currentMapArea = trigger.GetParentArea();
                currentMapArea?.OnPlayerEnterZone(trigger.GetZoneName());
            }
        }
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
        var hit = Physics2D.OverlapCircle(transform.position, 0.2f, battleLayer);
        if (hit != null)
        {
            var trigger = hit.GetComponent<EncounterZoneTrigger>();
            if (trigger == null) return;

            var mapArea = trigger.GetParentArea();
            if (mapArea == null) return;

            if (currentMapArea != mapArea)
            {
                currentMapArea?.OnPlayerExit();
                currentMapArea = mapArea;
            }

            currentMapArea.OnPlayerEnterZone(trigger.GetZoneName());

            if (currentMapArea.OnPlayerStep())
            {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
        else
        {
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