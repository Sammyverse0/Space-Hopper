using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerSwipeRunner : MonoBehaviour
{
    public GameObject touchToPlayText;
    [Header("Lane Settings")]
    public float laneOffset = 50f;
    public int laneCount = 3;
    private int currentLane = 1;

    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float laneChangeSpeed = 20f;

    [Header("Jump Settings")]
    public float jumpDistance = 200f;
    public float jumpHeight = 50f;
    public float jumpDuration = 1f;

    private Rigidbody rb;
    private bool isJumping = false;
    private Vector3 jumpStart, jumpEnd;
    private float jumpProgress = 0f;

    private Vector2 startTouch;
    private bool isTouching = false;

    private Animator animator;

    // Game state
    private bool gameStarted = false;  // player is idle at start

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) {
        Debug.LogError("ERROR: Animator not found on player!");
    } else {
        Debug.Log("Animator found successfully!");
    }
        animator.SetBool("isRunning", false);
    }

    void Update()
    {
        if (!gameStarted)
        {
            HandleGameStart();
            return;
        }

        HandleSwipe();
    }

    void FixedUpdate()
    {
        if (!gameStarted) return;

        if (isJumping)
        {
            PerformJump();
        }
        else
        {
            RunForward();
        }
    }

    // ---------------- START GAME ON TAP ----------------
    void HandleGameStart()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            gameStarted = true;
            animator.SetBool("isRunning", true); // switch to running anim
            
            Debug.Log("GAME STARTED! Set isRunning to true.");
            if (touchToPlayText != null)
            {
                touchToPlayText.SetActive(false);
            }
        }
    }

    // ---------------- SWIPE INPUT ----------------
    void HandleSwipe()
    {
        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.isPressed)
        {
            Vector2 currentPos = touch.position.ReadValue();

            if (!isTouching && touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                startTouch = currentPos;
                isTouching = true;
            }
            else if (isTouching)
            {
                Vector2 delta = currentPos - startTouch;

                if (delta.magnitude > 100f) // minimum swipe distance
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        // Horizontal swipe
                        if (delta.x > 0 && currentLane < laneCount - 1) currentLane++;
                        if (delta.x < 0 && currentLane > 0) currentLane--;
                        isTouching = false; // Reset after a swipe is registered
                    }
                    else
                    {
                        // Jump swipe
                        if (delta.y > 0 && !isJumping)
                        {
                            StartJump();
                            isTouching = false; // Reset after a swipe is registered
                        }
                    }
                }
            }
        }
        else
        {
            isTouching = false;
        }
    }

    // ---------------- RUN FORWARD (CORRECTED) ----------------
    void RunForward()
    {
        // 1. Calculate the target X position for the current lane.
        float targetX = GetLaneX();

        // 2. Smoothly interpolate the current X position towards the target X.
        float newX = Mathf.Lerp(rb.position.x, targetX, Time.fixedDeltaTime * laneChangeSpeed);

        // 3. Create a new position vector. The forward movement (Z axis) is constant and not smoothed.
        Vector3 newPos = new Vector3(newX, rb.position.y, rb.position.z + forwardSpeed * Time.fixedDeltaTime);

        // 4. Apply the new position to the Rigidbody.
        rb.MovePosition(newPos);
    }


    // ---------------- JUMP LOGIC ----------------
    private void StartJump()
    {
        isJumping = true;
        jumpProgress = 0f;

        jumpStart = rb.position;
        jumpEnd = new Vector3(GetLaneX(), rb.position.y, rb.position.z + jumpDistance);

        animator.SetBool("isJumping", true);
        Debug.Log("JUMP STARTED! Set isJumping to true.");
    }

    private void PerformJump()
    {
        jumpProgress += Time.fixedDeltaTime / jumpDuration;

        // Use a sine wave to create a smooth arc for the jump height
        float height = Mathf.Sin(Mathf.PI * jumpProgress) * jumpHeight;
        // Linearly interpolate the position from the start to the end of the jump
        Vector3 pos = Vector3.Lerp(jumpStart, jumpEnd, jumpProgress);
        pos.y += height;

        rb.MovePosition(pos);

        if (jumpProgress >= 1f)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);

            Debug.Log("JUMP ENDED! Set isJumping to false.");
        }
    }

    // ---------------- LANE UTILITY ----------------
    float GetLaneX()
    {
        // This formula calculates the X position for any lane, centered around X=0.
        // For 3 lanes (0, 1, 2), (laneCount / 2) is 1.
        // Lane 0: (0 - 1) * offset = -offset
        // Lane 1: (1 - 1) * offset = 0
        // Lane 2: (2 - 1) * offset = offset
        return (currentLane - (laneCount / 2)) * laneOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameOver"))
        {
            SceneManager.LoadScene("GameOver"); // replace with your Game Over scene name
        }
        else if (other.CompareTag("WinTrigger"))
        {
            SceneManager.LoadScene("WinScene"); // replace with your Win scene name
        }
    }
}