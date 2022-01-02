using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

// TO-DO NOTES
// - Currently, there is not really a min-jump-height applied. The min jump height is the jump amount when a button press takes only one frame.
//   To achieve a true min-height for a jump, recommend adding a min jump press time that will control whether jump dampening is applied.
// - Currently, the player jumps again if the jump button is pressed and held while the player is in mid-air. The early jump
//   mechanic should probably be overhauled. Recommend simply using a raycast and setting an isEarlyJumpPressed bool.

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Control")]
    [Space]
    
    [Tooltip("How fast the player moves horizontally")]
    [SerializeField] float moveSpeed = 8f;

    [Tooltip("Amount of force added when the player jumps")]
    [SerializeField] private float jumpSpeed = 15.5f;

    [Tooltip("Multiplier applied after the player reaches the maximum jump height")]
    [SerializeField] private float jumpFallMultiplier = 2f;

    [Tooltip("Multiplier applied to achieve a shorter jump height - applied when player is rising vertically and lets go of the jump button before max height reached")]
    [SerializeField] private float jumpShortMultiplier = 1.5f;

    [Tooltip("How early the player can jump before touching the ground again")]
    [Range(0, 0.5f)] [SerializeField] private float jumpEarlyTime = 0.05f;

    [Tooltip("How late the player can jump after leaving a surface")]
    [Range(0, 0.2f)] [SerializeField] private float jumpLateTime = 0.05f;

    [Tooltip("How long the jump button is held by default for short jumps")]
    [Range(0, 0.5f)] [SerializeField] private float jumpShortTime = 0.2f;

    [Tooltip("Whether or not a player can steer while jumping")]
    [SerializeField] private bool canAirControl = true;

    [Header("Events")]
    [Space]

    public UnityEvent OnJumpEvent;
    public UnityEvent OnLandEvent;

    // STATE
    Vector2 moveInput;
    bool isRunning = false;
    bool isJumping = false;
    bool isGrounded = false;
    bool isJumpPressed = false;
    float jumpEarlyTimeElapsed = 0f;
    float jumpLateTimeElapsed = 0f;
    float jumpShortTimeElapsed = 0f;

    // COMPONENTS
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    int groundLayerMask;

    // ANIMATION STATES - can also use an ENUM
    const string ANIM_PLAYER_IDLE = "PlayerIdle";
    const string ANIM_PLAYER_RUNNING = "PlayerRunning";
    string currentAnimState;
    string nextAnimState;

    void Initialize()
    {
        jumpEarlyTimeElapsed = jumpEarlyTime + 1f;
        jumpLateTimeElapsed = jumpLateTime + 1f;
    }

    void Awake() {
        if (OnLandEvent == null) OnLandEvent = new UnityEvent();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        AppIntegrity.AssertPresent(rb);

        anim = GetComponent<Animator>();
        AppIntegrity.AssertPresent(anim);

        col = GetComponent<Collider2D>();
        AppIntegrity.AssertPresent(col);

        groundLayerMask = LayerMask.GetMask("Ground");

        Initialize();
    }

    void Update()
    {
        Animate();
    }

    void FixedUpdate() {
        HandleMove();
        HandleJump();
        HandleFall();
    }

    void HandleMove()
    {
        if (CanMoveX())
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);

            // Mathf.Epsilon is better than using a hard-coded zero value
            // see: https://docs.unity3d.com/ScriptReference/Mathf.Epsilon.html
            isRunning = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;

            // flip sprite
            if (isRunning) {
                transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
            }
        }
    }

    bool CanMoveX() {
        if (isGrounded) return true;
        if (canAirControl) return true;
        return false;
    }

    void HandleJump() {
        // implement coyote (hang) time before disallowing jump
        if (checkIsGrounded())
        {
            isGrounded = true;
            jumpLateTimeElapsed = 0;
        }
        else if (jumpLateTimeElapsed > jumpLateTime)
        {
            isGrounded = false;
        }
        else
        {
            jumpLateTimeElapsed = Mathf.Min(jumpLateTimeElapsed + Time.fixedDeltaTime, jumpLateTime + 1f);
        }

        // handle early jump button press
        if (isJumpPressed && !isGrounded && !isJumping) {
            jumpEarlyTimeElapsed = 0f;
        } else {
            jumpEarlyTimeElapsed = Mathf.Min(jumpEarlyTimeElapsed + Time.fixedDeltaTime, jumpEarlyTime + 1f);
        }

        // increment jump press time elapsed
        jumpShortTimeElapsed = Mathf.Min(jumpShortTimeElapsed + Time.fixedDeltaTime, jumpShortTime + 1f);

        // handle early jump button release (shorter jump)
        if (!isJumpPressed) {
            isJumping = false;
        }

        // handle initial jump impulse
        if (shouldJump()) {
            isJumping = true;
            jumpShortTimeElapsed = 0f;
            jumpEarlyTimeElapsed = jumpEarlyTime + 1f;
            // rb.velocity += new Vector2(0f, jumpSpeed);
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    bool checkIsGrounded()
    {
        return col.IsTouchingLayers(groundLayerMask);
    }

    bool shouldJump() {
        if (!isGrounded) return false;
        // disallow double-jumps
        if (isJumping) return false;
        if (jumpEarlyTimeElapsed < jumpEarlyTime) return true;
        if (isJumpPressed) return true;
        return false;
    }

    void HandleFall() {
        // rising
        if (rb.velocity.y > 0 && !isJumping && jumpShortTimeElapsed >= jumpShortTime)
        {
            rb.velocity += Vector2.up
                * Physics2D.gravity.y
                // grav accel constant is per second; multiply by the time slice to get the accel for this frame
                * Time.fixedDeltaTime
                // 1 "gravity unit" is already accounted for; subtract one here to make our multiplier easier to reason about => 0 would mean normal gravity
                * (jumpShortMultiplier - 1f);
        }
        // falling
        else if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up
                * Physics2D.gravity.y
                // grav accel constant is per second; multiply by the time slice to get the accel for this frame
                * Time.fixedDeltaTime
                // 1 "gravity unit" is already accounted for; subtract one here to make our multiplier easier to reason about => 0 would mean normal gravity
                * (jumpFallMultiplier - 1f);

        }
    }

    void Animate()
    {
        if (isRunning) {
            nextAnimState = ANIM_PLAYER_RUNNING;
        } else {
            nextAnimState = ANIM_PLAYER_IDLE;
        }

        if (currentAnimState == nextAnimState) return;

        currentAnimState = nextAnimState;
        anim.Play(currentAnimState);
    }


    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        isJumpPressed = value.isPressed;
    }
}
