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
    public float laneChangeSpeed = 10f;

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
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        animator = GetComponent<Animator>();
        animator.SetBool("isRunning", false); // idle at start
    }

    void Update()
    {
        if (!gameStarted)
        {
            HandleGameStart(); // wait for tap
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
            touchToPlayText.SetActive(false);
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

                        isTouching = false;
                    }
                    else
                    {
                        // Jump swipe
                        if (delta.y > 0 && !isJumping)
                        {
                            StartJump();
                            isTouching = false;
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

    // ---------------- RUN FORWARD ----------------
    void RunForward()
    {
        float targetX = GetLaneX();
        Vector3 targetPos = new Vector3(targetX, rb.position.y, rb.position.z + forwardSpeed * Time.fixedDeltaTime);

        Vector3 newPos = Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * laneChangeSpeed);
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
    }

    private void PerformJump()
    {
        jumpProgress += Time.fixedDeltaTime / jumpDuration;

        float height = Mathf.Sin(Mathf.PI * jumpProgress) * jumpHeight;
        Vector3 pos = Vector3.Lerp(jumpStart, jumpEnd, jumpProgress);
        pos.y += height;

        rb.MovePosition(pos);

        if (jumpProgress >= 1f)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }

    // ---------------- LANE UTILITY ----------------
    float GetLaneX()
    {
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


