using UnityEngine;

// This script manages player movement (gravity, jumping).
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // --- Player Movement Variables ---

    // The speed at which the player is pulled towards gravity sources.
    public float moveSpeed = 10f;
    
    // The force applied when the player jumps.
    public float jumpForce = 10f;

    // The tag used to identify "planets" or gravity sources.
    public string gravitySourceTag = "GravitySource";

    // --- Swipe Input Variables ---

    // The minimum distance a touch must move to be considered a swipe.
    private float swipeThreshold = 50f;
    private Vector2 touchStartPos;
    private bool isSwiping = false;

    // --- Private References ---

    private Rigidbody rb;
    private Transform closestGravitySource;

    private void Awake()
    {
        // Get the Rigidbody component from the GameObject.
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }
    
    private void Update()
    {
        // Check for a single touch.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                // When the touch begins, record the starting position.
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = false;
                    break;

                // When the touch is moved, check if it's a valid swipe.
                case TouchPhase.Moved:
                    if (!isSwiping && Vector2.Distance(touchStartPos, touch.position) > swipeThreshold)
                    {
                        isSwiping = true;
                        HandleSwipe(touch);
                    }
                    break;
                
                // When the touch ends, handle it if it was a swipe.
                case TouchPhase.Ended:
                    if (!isSwiping && Vector2.Distance(touchStartPos, touch.position) > swipeThreshold)
                    {
                         HandleSwipe(touch);
                    }
                    isSwiping = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Handles the swipe direction and applies the appropriate forces.
    /// </summary>
    /// <param name="touch">The touch object to analyze.</param>
    private void HandleSwipe(Touch touch)
    {
        Vector2 swipeDelta = touch.position - touchStartPos;
        float angle = Vector2.SignedAngle(Vector2.up, swipeDelta.normalized);

        // Apply a jump force for all upward swipes.
        if (swipeDelta.y > 0)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        // Apply a horizontal force based on the swipe direction.
        if (angle > 45 && angle < 135)
        {
            // Swipe Up-Right
            rb.AddForce(transform.right * jumpForce * 0.5f, ForceMode.Impulse);
        }
        else if (angle > -135 && angle < -45)
        {
            // Swipe Up-Left
            rb.AddForce(-transform.right * jumpForce * 0.5f, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        // Find the closest gravity source for a continuous pull.
        FindClosestGravitySource();

        if (closestGravitySource != null)
        {
            Vector3 gravityDirection = (closestGravitySource.position - transform.position).normalized;
            rb.AddForce(gravityDirection * moveSpeed, ForceMode.Force);
        }
    }

    /// <summary>
    /// Searches for and finds the closest object tagged as a "GravitySource".
    /// </summary>
    private void FindClosestGravitySource()
    {
        GameObject[] gravitySources = GameObject.FindGameObjectsWithTag(gravitySourceTag);
        float closestDistance = Mathf.Infinity;
        Transform newClosestSource = null;

        foreach (GameObject source in gravitySources)
        {
            float distance = Vector3.Distance(transform.position, source.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                newClosestSource = source.transform;
            }
        }

        closestGravitySource = newClosestSource;
    }
}
