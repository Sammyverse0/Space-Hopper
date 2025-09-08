using UnityEngine;
using UnityEngine.InputSystem;
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
    public float laneChangeSpeed = 100f;

    [Header("Jump Settings")]
    public float jumpDistance = 200f;
    public float jumpHeight = 50f;
    public float jumpDuration = 1f;

    // --- Private Variables ---
    private Rigidbody rb;
    private bool isJumping = false;
    private Vector3 jumpStart, jumpEnd;
    private float jumpProgress = 0f;
    private Vector2 startTouch;
    private bool isTouching = false;
    private Animator animator;
    private bool gameStarted = false;

    // A simple way to check for game over
    private float fallThreshold = -20f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetBool("isRunning", false);
    }

    void Update()
    {
        if (!gameStarted)
        {
            HandleGameStart();
            return;
        }

        HandleSwipe();

        // Check if the player has fallen off the level
        if (transform.position.y < fallThreshold)
        {
            ScoreManager.instance.OnGameOver();
            SceneManager.LoadScene("GameOver"); // Or your game over scene name
        }
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

    void HandleGameStart()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            gameStarted = true;
            if (animator != null) animator.SetBool("isRunning", true);
            if (touchToPlayText != null) touchToPlayText.SetActive(false);
        }
    }

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

                if (delta.magnitude > 100f)
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0 && currentLane < laneCount - 1) currentLane++;
                        if (delta.x < 0 && currentLane > 0) currentLane--;
                        isTouching = false;
                    }
                    else
                    {
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

    void RunForward()
    {
        float targetX = GetLaneX();
        float newX = Mathf.Lerp(rb.position.x, targetX, Time.fixedDeltaTime * laneChangeSpeed);
        Vector3 newPos = new Vector3(newX, rb.position.y, rb.position.z + forwardSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    private void StartJump()
    {
        isJumping = true;
        jumpProgress = 0f;
        jumpStart = rb.position;
        jumpEnd = new Vector3(GetLaneX(), rb.position.y, rb.position.z + jumpDistance);
        if (animator != null) animator.SetBool("isJumping", true);
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
            if (animator != null) animator.SetBool("isJumping", false);
        }
    }

    float GetLaneX()
    {
        return (currentLane - (laneCount / 2)) * laneOffset;
    }
}
