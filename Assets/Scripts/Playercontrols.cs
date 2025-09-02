using UnityEngine;
using UnityEngine.SceneManagement;

// This script manages player movement (gravity, jumping) and handles all physics interactions.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // --- Public Variables to Adjust in Unity Editor ---

    // The speed at which the player is pulled towards gravity sources.
    public float gravityForce = 20f;
    
    // The force applied when the player jumps.
    public float jumpForce = 15f;

    // The speed at which the player rotates to align with gravity.
    public float rotationSpeed = 10f;

    // The speed at which the player runs forward on a planet.
    public float runSpeed = 8f;

    // The maximum distance a player can be from a planet to be affected by its gravity.
    public float gravityActivationDistance = 30f;

    // The tag used to identify "planets" or gravity sources.
    public string gravitySourceTag = "GravitySource";
    
    // The tag for the game over trigger.
    public string gameOverTriggerTag = "GameOver";

    // --- Private References ---

    private Rigidbody rb;
    private Transform closestGravitySource;
    private bool isGrounded;
    private Vector2 touchStartPos;
    private bool isSwiping;
    
    // The minimum distance a touch must move to be considered a swipe.
    private const float SwipeThreshold = 50f;

    private void Awake()
    {
        // Get the Rigidbody component from the GameObject.
        rb = GetComponent<Rigidbody>();
        
        // Freeze rotation on the X and Z axes to prevent the player from toppling over.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        // On game start, immediately snap the player to the closest planet.
        FindClosestGravitySource();

        if (closestGravitySource != null)
        {
            // Position the player on the planet's surface.
            Vector3 planetSurfacePosition = closestGravitySource.position + (transform.position - closestGravitySource.position).normalized * (closestGravitySource.GetComponent<Collider>().bounds.extents.y + 0.1f);
            transform.position = planetSurfacePosition;
            
            // Set the player's rotation to align with the planet's gravity.
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -Vector3.Normalize(closestGravitySource.position - transform.position)) * transform.rotation;
            transform.rotation = targetRotation;

            // Mark the player as grounded.
            isGrounded = true;
            rb.isKinematic = true;
        }
    }
    
    private void Update()
    {
        HandleTouchInput();
    }

    private void FixedUpdate()
    {
        // Find the closest gravity source for a continuous pull.
        FindClosestGravitySource();

        if (closestGravitySource != null)
        {
            float distance = Vector3.Distance(transform.position, closestGravitySource.position);

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
                    rb.AddForce(gravityDirection * gravityForce, ForceMode.Force);
                }
                
                // Always align the player's rotation with the gravity source.
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -Vector3.Normalize(closestGravitySource.position - transform.position)) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Handles all touch-based swipe input.
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = false;
                    break;
                case TouchPhase.Ended:
                    if (Vector2.Distance(touchStartPos, touch.position) > SwipeThreshold)
                    {
                        HandleSwipe(touch);
                    }
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

        // Re-enable Rigidbody physics when a swipe (jump) occurs.
        rb.isKinematic = false;

        // Apply jump force.
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // Apply a horizontal force based on the swipe direction.
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y) * 0.5f)
        {
            if (swipeDelta.x > 0)
            {
                // Swipe right
                rb.AddForce(transform.right * jumpForce * 0.5f, ForceMode.Impulse);
            }
            else
            {
                // Swipe left
                rb.AddForce(-transform.right * jumpForce * 0.5f, ForceMode.Impulse);
            }
        }
    }

    /// <summary>
    /// Checks for collisions to determine if the player is grounded.
    /// This is where we make the player "stick" to the planet.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(gravitySourceTag))
        {
            isGrounded = true;
            rb.isKinematic = true;

            // Instantly snap the player's rotation to align with the planet's gravity.
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -Vector3.Normalize(closestGravitySource.position - transform.position)) * transform.rotation;
            transform.rotation = targetRotation;
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
    
    /// <summary>
    /// Handles the game over trigger.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(gameOverTriggerTag))
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
