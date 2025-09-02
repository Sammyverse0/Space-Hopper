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

    // The speed at which the player rotates to align with gravity.
    public float rotationSpeed = 5f;

    // The speed at which the player runs forward on a planet.
    public float runSpeed = 5f;

    // The maximum distance a player can be from a planet to be affected by its gravity.
    public float gravityActivationDistance = 20f;

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

    private bool isGrounded;

    // --- Game Over Trigger ---

    // The tag for the game over trigger.
    public string gameOverTriggerTag = "GameOver";

    private void Awake()
    {
        // Get the Rigidbody component from the GameObject.
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }

    private void Start()
    {
        // On game start, immediately snap the player to the closest planet.
        FindClosestGravitySource();

        if (closestGravitySource != null)
        {
            // Position the player on the planet's surface.
            Vector3 planetSurfacePosition = closestGravitySource.position + (transform.position - closestGravitySource.position).normalized * closestGravitySource.GetComponent<Collider>().bounds.extents.y;
            transform.position = planetSurfacePosition;
            
            // Set the player's rotation to align with the planet's gravity.
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -Vector3.Normalize(closestGravitySource.position - transform.position)) * transform.rotation;
            transform.rotation = targetRotation;

            // Mark the player as grounded and make the rigidbody kinematic to prevent sliding.
            isGrounded = true;
            rb.isKinematic = true;
        }
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
        // Only allow a jump if the player is grounded.
        if (!isGrounded)
        {
            return;
        }

        Vector2 swipeDelta = touch.position - touchStartPos;
        float angle = Vector2.SignedAngle(Vector2.up, swipeDelta.normalized);

        // When the player jumps, re-enable the Rigidbody.
        rb.isKinematic = false;

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
            float distance = Vector3.Distance(transform.position, closestGravitySource.position);

            // Only apply gravity if the player is within the activation distance.
            if (distance < gravityActivationDistance)
            {
                // Apply a continuous forward movement when grounded.
                if (isGrounded)
                {
                    transform.position += transform.forward * runSpeed * Time.deltaTime;
                }
                
                // Gravity is applied only when the player is not grounded.
                if (!isGrounded)
                {
                    Vector3 gravityDirection = (closestGravitySource.position - transform.position).normalized;
                    rb.AddForce(gravityDirection * moveSpeed, ForceMode.Force);
                }
                
                // Always align the player's rotation with the gravity source.
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -Vector3.Normalize(closestGravitySource.position - transform.position)) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Checks for collisions to determine if the player is grounded.
    /// This is where we make the player "stick" to the planet.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        // Check if the collision object has the GravitySource tag.
        if (collision.gameObject.CompareTag(gravitySourceTag))
        {
            // The player is grounded.
            isGrounded = true;
            
            // Set the rigidbody to kinematic to prevent sliding.
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// When the player leaves a planet, they are no longer grounded.
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(gravitySourceTag))
        {
            isGrounded = false;
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
    
    // New code to handle the game over trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has the GameOver tag.
        if (other.CompareTag(gameOverTriggerTag))
        {
            // Load the "GameOver" scene.
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }
}
