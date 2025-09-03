using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerSwipeRunner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float laneOffset = 50f;      // Distance between lanes
    public int laneCount = 3;           // Number of lanes
    private int currentLane = 1;        // Start in middle lane

    [Header("Movement Settings")]
    public float forwardSpeed = 10f;     // Running speed when on planet
    public float laneChangeSpeed = 10f;  // How fast we move sideways

    [Header("Jump Settings")]
    public float jumpDistance = 200f;    // Forward distance covered in one jump
    public float jumpHeight = 50f;       // Maximum jump height
    public float jumpDuration = 1f;      // Time taken for one jump

    private Rigidbody rb;
    private bool isJumping = false;
    private Vector3 jumpStart, jumpEnd;
    private float jumpProgress = 0f;

    private Vector2 startTouch;
    private bool isTouching = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        HandleSwipe();
    }

    void FixedUpdate()
    {
        if (isJumping)
        {
            PerformJump();
        }
        else
        {
            RunForward();
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
                        // Horizontal swipe (trigger instantly)
                        if (delta.x > 0 && currentLane < laneCount - 1) currentLane++; // right
                        if (delta.x < 0 && currentLane > 0) currentLane--;             // left

                        isTouching = false; // reset so multiple swipes can be detected
                    }
                    else
                    {
                        // Vertical swipe (jump)
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
        }
    }

    // ---------------- LANE UTILITY ----------------
    float GetLaneX()
    {
        return (currentLane - (laneCount / 2)) * laneOffset;
    }
}
