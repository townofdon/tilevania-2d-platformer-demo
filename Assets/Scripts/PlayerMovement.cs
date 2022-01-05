using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Cinemachine;

// TO-DO NOTES
// - Currently, the player jumps again if the jump button is pressed and held while the player is in mid-air. The early jump
//   mechanic should probably be overhauled. Recommend simply using a raycast and setting an isEarlyJumpPressed bool.

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Control")]
    [Space]
    
    [Tooltip("How fast the player moves horizontally")]
    [SerializeField] float moveSpeed = 8f;

    [Tooltip("How fast the player climbs vertically")]
    [SerializeField] float climbSpeed = 4f;

    [Tooltip("Amount of force added when the player jumps")]
    [SerializeField] private float jumpSpeed = 15.5f;

    [Tooltip("Max speed at which the player falls (i.e. their terminal velocity) - 1 = 1s of normal grav")]
    [SerializeField] private float maxFallSpeed = 2.5f;

    [Tooltip("Multiplier applied after the player reaches the maximum jump height")]
    [SerializeField] private float jumpFallMultiplier = 2f;

    [Tooltip("Multiplier applied to achieve a shorter jump height - applied when player is rising vertically and lets go of the jump button before max height reached")]
    [SerializeField] private float jumpShortMultiplier = 1.5f;

    [Tooltip("How late the player can jump after leaving a surface")]
    [Range(0, 0.2f)] [SerializeField] private float jumpLateTime = 0.05f;

    [Tooltip("How long the jump button is held by default for short jumps")]
    [Range(0, 0.5f)] [SerializeField] private float jumpShortTime = 0.2f;

    [Tooltip("Whether or not a player can steer while jumping")]
    [SerializeField] private bool canAirControl = true;

    [Header("Cameras")]
    [Space]

    [SerializeField] private CinemachineVirtualCamera followCam;

    [Header("Events")]
    [Space]

    public UnityEvent OnJumpEvent;
    public UnityEvent OnLandEvent;

    // STATE - MOVEMENT
    int orientationX = 1; // 1 => right, -1 => left
    Vector2 moveInput;
    bool isRunning = false;
    bool isGrounded = false;

    // STATE - CLIMBING
    bool canClimb = false;
    bool isClimbing = false;
    bool isClimbIdle = false;

    // STATE - JUMPING
    bool isJumpPressed = false;
    bool isJumpPressedEarly = false;
    bool isJumping = false;
    float jumpLateTimeElapsed = 0f;
    float jumpShortTimeElapsed = 0f;

    // COMPONENTS
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    GameObject groundCheck;
    GameObject cameraLeft;
    GameObject cameraRight;
    GameObject cameraCenter;
    int playerLayerMask;
    int groundLayerMask;
    int laddersLayerMask;
    float gravityScale;

    // ANIMATION STATES - can also use an ENUM
    const string ANIM_PLAYER_IDLE = "PlayerIdle";
    const string ANIM_PLAYER_RUNNING = "PlayerRunning";
    const string ANIM_PLAYER_CLIMBING = "PlayerClimbing";
    const string ANIM_PLAYER_CLIMB_IDLE = "PlayerClimbIdle";
    string currentAnimState;
    string nextAnimState;

    // PUBLIC METHODS

    public bool IsJumpPressed() {
        return isJumpPressed || isJumpPressedEarly;
    }

    public bool IsClimbing() {
        return isClimbing;
    }

    // PRIVATE METHODS

    void Initialize()
    {
        jumpLateTimeElapsed = jumpLateTime + 1f;
    }

    void Awake() {
        if (OnJumpEvent == null) OnJumpEvent = new UnityEvent();
        if (OnLandEvent == null) OnLandEvent = new UnityEvent();
    }

    void Start()
    {
        AppIntegrity.AssertPresent(followCam);

        rb = GetComponent<Rigidbody2D>();
        AppIntegrity.AssertPresent(rb);

        anim = GetComponent<Animator>();
        AppIntegrity.AssertPresent(anim);

        col = GetComponent<Collider2D>();
        AppIntegrity.AssertPresent(col);

        playerLayerMask = LayerMask.GetMask("Player");
        AppIntegrity.AssertPresent(playerLayerMask);

        groundLayerMask = LayerMask.GetMask("Ground");
        AppIntegrity.AssertPresent(groundLayerMask);

        laddersLayerMask = LayerMask.GetMask("Ladders");
        AppIntegrity.AssertPresent(laddersLayerMask);

        groundCheck = FindChildGameObject(this.gameObject, "GroundCheck");
        AppIntegrity.AssertPresent(groundCheck);

        cameraLeft = FindChildGameObject(this.gameObject, "CameraLeft");
        AppIntegrity.AssertPresent(cameraLeft);

        cameraRight = FindChildGameObject(this.gameObject, "CameraRight");
        AppIntegrity.AssertPresent(cameraRight);

        cameraCenter = FindChildGameObject(this.gameObject, "CameraCenter");
        AppIntegrity.AssertPresent(cameraCenter);

        gravityScale = rb.gravityScale;

        Initialize();
    }

    void Update()
    {
        Animate();
    }

    void FixedUpdate() {
        HandleMove();
        HandleOrientation();
        HandleClimb();
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
        }
    }

    void HandleOrientation() {
        // handle sprite flip
        if (isRunning && !isClimbing && orientationX != Mathf.Sign(rb.velocity.x))
        {
            orientationX = -orientationX;
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }

        // handle camera positioning
        if (isClimbing)
        {
            followCam.LookAt = cameraCenter.transform;
        }
        else if (orientationX >= 0)
        {
            followCam.LookAt = cameraRight.transform;
        }
        else
        {
            followCam.LookAt = cameraLeft.transform;
        }
    }

    bool CanMoveX() {
        if (isGrounded) return true;
        if (isClimbing) return false;
        if (canAirControl) return true;
        return false;
    }

    void HandleClimb() {
        canClimb = CheckCanClimb();
        bool isClimbBottomReached = isClimbing && isGrounded && moveInput.y < Mathf.Epsilon;

        // handle climb cancel
        if (!canClimb || isJumping || isClimbBottomReached) {
            isClimbing = false;
        }

        // handle climb start
        if (canClimb && !isClimbing && !isJumpPressed) {
            isClimbing = Mathf.Abs(moveInput.y) > Mathf.Epsilon;
        }

        // handle climb movement
        if (isClimbing) {
            rb.velocity = new Vector2(moveInput.x * climbSpeed, moveInput.y * climbSpeed);
            isClimbIdle = Mathf.Abs(moveInput.y) < Mathf.Epsilon && Mathf.Abs(moveInput.x) < Mathf.Epsilon;
        }

        // handle climb gravity
        if (isClimbing) {
            rb.gravityScale = 0;
        } else {
            rb.gravityScale = gravityScale;
        }

        // handle ladder collisions
        // Physics2D.IgnoreLayerCollision(ToLayer(playerLayerMask), ToLayer(laddersLayerMask), isClimbing);
        // Physics2D.IgnoreLayerCollision(ToLayer(playerLayerMask), ToLayer(laddersLayerMask));
    }

    void HandleJump() {
        // implement coyote (hang) time before disallowing jump
        if (CheckIsGrounded())
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

        // increment jump press time elapsed
        jumpShortTimeElapsed = Mathf.Min(jumpShortTimeElapsed + Time.fixedDeltaTime, jumpShortTime + 1f);

        // handle early jump button release (shorter jump)
        if (!isJumpPressed) {
            isJumping = false;
        }

        // handle initial jump impulse
        if (ShouldJump()) {
            isJumping = true;
            jumpShortTimeElapsed = 0f;
            // rb.velocity += new Vector2(0f, jumpSpeed);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(jumpSpeed, jumpSpeed + rb.velocity.y));
        }
    }

    bool CheckIsGrounded()
    {
        // note - tutorial used Collider.IsTouchingLayers(groundLayerMask)
        // however, this would mean that touching a Ground layer sideways or even at the top would be considered "grounded"
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 0.1f, groundLayerMask);
        return hit.collider != null;
    }

    bool CheckCanClimb() {
        return col.IsTouchingLayers(laddersLayerMask);
    }

    bool CheckClimbBottomReached() {
        return isClimbing && isGrounded && moveInput.y < Mathf.Epsilon;
    }

    bool ShouldJump() {
        if (!isGrounded && !isClimbing) return false;
        // disallow double-jumps
        if (isJumping) return false;
        if (isJumpPressedEarly) return true;
        if (isJumpPressed) return true;
        return false;
    }

    void HandleFall() {
        if (!isClimbing) {
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

        // handle max fall speed
        float terminalVelocity = maxFallSpeed * Physics2D.gravity.y;
        if (rb.velocity.y < terminalVelocity) {
            // apply dampening
            rb.velocity = new Vector2(rb.velocity.x, terminalVelocity);
        }
    }

    void Animate()
    {
        if (isClimbing && isClimbIdle) {
            nextAnimState = ANIM_PLAYER_CLIMB_IDLE;
        }
        else if (isClimbing)
        {
            nextAnimState = ANIM_PLAYER_CLIMBING;
        }
        else if (isRunning)
        {
            nextAnimState = ANIM_PLAYER_RUNNING;
        }
        else
        {
            nextAnimState = ANIM_PLAYER_IDLE;
        }

        if (currentAnimState == nextAnimState) return;

        currentAnimState = nextAnimState;
        anim.Play(currentAnimState);
    }

    // void OnTriggerEnter2D(Collider2D other) {
    //     // NOTE - could have also evaluated trigger overlap using Rigidbody2D.IsTouchingLayers
    //     if (LayerMaskContainsLayer(laddersLayerMask, other.gameObject.layer)) {
    //         canClimb = true;
    //     }
    // }

    // void OnTriggerExit2D(Collider2D other) {
    //     if (LayerMaskContainsLayer(laddersLayerMask, other.gameObject.layer)) {
    //         canClimb = false;
    //         isClimbing = false;
    //     }
    // }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        isJumpPressed = value.isPressed;
    }

    // 
    // UTIL METHODS - move these to a static class
    // 

    // check to see whether a LayerMask contains a layer
    // see: https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html
    bool LayerMaskContainsLayer(int mask, int layer) {
        bool contains = ((mask & (1 << layer)) != 0);
        return contains;
    }

    // get the layer num from a layermask
    // see: https://forum.unity.com/threads/get-the-layernumber-from-a-layermask.114553/#post-3021162
    int ToLayer(int layerMask) {
        int result = layerMask > 0 ? 0 : 31;
        while( layerMask > 1 ) {
            layerMask = layerMask >> 1;
            result++;
        }
        return result;
    }

    // Get a child game object by name or tag
    // see: https://answers.unity.com/questions/183649/how-to-find-a-child-gameobject-by-name.html
    GameObject FindChildGameObject(GameObject fromGameObject, string search) {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == search || t.gameObject.tag == search) return t.gameObject;
        return null;
    }
}
