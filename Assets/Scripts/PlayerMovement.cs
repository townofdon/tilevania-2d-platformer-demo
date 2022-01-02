using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

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
    [SerializeField] private float jumpShortMultiplier = 2f;

    [Tooltip("How early the player can jump before touching the ground again")]
    [Range(0, 0.5f)] [SerializeField] private float jumpEarlyTime = 0.2f;

    [Tooltip("How late the player can jump after leaving a surface")]
    [Range(0, 0.2f)] [SerializeField] private float jumpLateTime = 0.05f;

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
    bool isGroundedRaw = false; // the actual grounded state
    bool isGrounded = false; // what we will use to determine "grounded" or not
    bool isJumpPressed = false;
    float jumpEarlyTimeElapsed = 0f;
    float jumpLateTimeElapsed = 0f;

    // COMPONENTS
    Rigidbody2D rb;
    Animator anim;

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
        Vector2 vel = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        rb.velocity = vel;

        // Mathf.Epsilon is better than using a hard-coded zero value
        // see: https://docs.unity3d.com/ScriptReference/Mathf.Epsilon.html
        isRunning = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;

        // flip sprite
        if (isRunning) {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
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

        // handle early jump button release (shorter jump)
        if (!isJumpPressed) {
            isJumping = false;
        }

        // handle initial jump impulse
        if (shouldJump()) {
            isJumping = true;
            jumpEarlyTimeElapsed = jumpEarlyTime + 1f;
            // rb.velocity += new Vector2(0f, jumpSpeed);
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            Debug.Log("JUMPING!");
            Debug.Log("Current vel is: " + rb.velocity.y);
        }
    }

    bool checkIsGrounded()
    {
        // TODO: ADD IS GROUNDED CHECK
        // bool wasGrounded = isGrounded;
        // // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        // Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        // for (int i = 0; i < colliders.Length; i++)
        // {
        //     if (colliders[i].gameObject != gameObject)
        //     {
        //         isGrounded = true;
        //         if (!wasGrounded)
        //         OnLandEvent.Invoke();
        //         return true;
        //     }
        // }
        return true;
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
        if (rb.velocity.y > 0)
        {

        }
        // falling
        else
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
