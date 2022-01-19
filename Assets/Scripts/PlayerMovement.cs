using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Cinemachine;

// TO-DO NOTES
// - Currently, the player jumps again if the jump button is pressed and held while the player is in mid-air. The early jump
//   mechanic should probably be overhauled. Recommend simply using a raycast and setting an isEarlyJumpPressed bool.

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Properties")]
    [Space]

    [SerializeField] float startHealth = 100f;
    [SerializeField] float timeInvincibleAfterTakingDamage = 1f;
    [SerializeField] float damagedBlinkRate = 0.1f;
    [SerializeField] float damagedBlinkOpacity = 0.5f;
    [SerializeField] Color damagedBlinkColor = Color.red;

    [Header("Weapons")]
    [Space]

    [SerializeField] GameObject arrow;
    [SerializeField] float arrowSpeed = 20f;
    [SerializeField] float timeToShowFiringAnim = 0.25f;

    [Header("Movement Control")]
    [Space]
    
    [Tooltip("How fast the player moves horizontally")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float moveSlowdown = 5f;

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
    public UnityEvent OnFireEvent;

    // STATE - GENERAL
    float health = 100f;
    bool isAlive;
    float timeInvincibleAfterTakingDamageElapsed = 0f;

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

    // STATE - WEAPONS
    bool hasWeaponBow = false;
    float timeFiring = 0f;

    // COMPONENTS
    SpriteRenderer spriteRenderer;
    ParticleSystem particles;
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    GameObject groundCheck;
    GameObject cameraLeft;
    GameObject cameraRight;
    GameObject cameraCenter;
    GameObject gun;
    int playerLayerMask;
    int groundLayerMask;
    int laddersLayerMask;
    int enemiesLayerMask;
    int hazardsLayerMask;
    float gravityScale;

    // ANIMATION STATES - can also use an ENUM
    const string ANIM_PLAYER_IDLE = "PlayerIdle";
    const string ANIM_PLAYER_DEATH = "PlayerDeath";
    const string ANIM_PLAYER_FIRING = "PlayerFiring";
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

    public bool TakeDamage(float amount)
    {
        if (!isAlive) return false;
        if (timeInvincibleAfterTakingDamageElapsed < timeInvincibleAfterTakingDamage) return false;

        timeInvincibleAfterTakingDamageElapsed = 0;
        health -= amount;

        if (health <= 0) {
            Die();
        } else if (amount > 0f) {
            if (UnityEngine.Random.Range(0, 1) == 0)
                AudioManager.instance.Play("PlayerDamage1");
            else
                AudioManager.instance.Play("PlayerDamage2");
        }

        RefreshUI();
        
        return true;
    }

    public bool TakeHealth(float amount)
    {
        if (!isAlive) return false;

        health = Mathf.Min(health + amount, 100f);
        RefreshUI();

        return true;
    }

    public void Die()
    {
        isAlive = false;
        rb.gravityScale = gravityScale;
        rb.freezeRotation = false;
        // fall over dead
        rb.AddTorque(1f);

        Time.timeScale = 0.2f;

        AudioManager.instance.Play("PlayerDeath");
        FindObjectOfType<GameSession>().ProcessPlayerDeath();
        RefreshUI();
    }

    public void AcquireWeaponBow()
    {
        hasWeaponBow = true;
        RefreshUI();
    }

    public void PlayParticleEffect()
    {
        particles.Play();
    }

    public void StopParticleEffect()
    {
        particles.Stop();
        particles.Clear();
    }

    // PRIVATE METHODS

    void Initialize()
    {
        jumpLateTimeElapsed = jumpLateTime + 1f;
        timeInvincibleAfterTakingDamageElapsed = timeInvincibleAfterTakingDamage + 1f;
        timeFiring = timeToShowFiringAnim + 1f;
        health = startHealth;
        isAlive = true;
        isRunning = false;
        isJumping = false;
        isClimbing = false;
        hasWeaponBow = false;

        Time.timeScale = 1f;

        SpriteRenderer gunSprite = gun.GetComponent<SpriteRenderer>();
        if (gunSprite)
        {
            gunSprite.enabled = false;
        }

        RefreshUI();
    }

    void Awake() {
        if (OnJumpEvent == null) OnJumpEvent = new UnityEvent();
        if (OnLandEvent == null) OnLandEvent = new UnityEvent();
        if (OnFireEvent == null) OnFireEvent = new UnityEvent();
    }

    void Start()
    {
        AppIntegrity.AssertPresent(followCam);
        AppIntegrity.AssertPresent(arrow);

        spriteRenderer = Utils.GetRequiredComponent<SpriteRenderer>(this.gameObject);
        particles = Utils.GetRequiredComponent<ParticleSystem>(this.gameObject);
        rb = Utils.GetRequiredComponent<Rigidbody2D>(this.gameObject);
        anim = Utils.GetRequiredComponent<Animator>(this.gameObject);
        col = Utils.GetRequiredComponent<Collider2D>(this.gameObject);

        groundCheck = Utils.GetRequiredChild(this.gameObject, "GroundCheck");
        cameraLeft = Utils.GetRequiredChild(this.gameObject, "CameraLeft");
        cameraRight = Utils.GetRequiredChild(this.gameObject, "CameraRight");
        cameraCenter = Utils.GetRequiredChild(this.gameObject, "CameraCenter");
        gun = Utils.GetRequiredChild(this.gameObject, "Gun");

        playerLayerMask = LayerMask.GetMask("Player");
        groundLayerMask = LayerMask.GetMask("Ground");
        laddersLayerMask = LayerMask.GetMask("Ladders");
        enemiesLayerMask = LayerMask.GetMask("Enemies");
        hazardsLayerMask = LayerMask.GetMask("Hazards");
        AppIntegrity.AssertPresent(playerLayerMask);
        AppIntegrity.AssertPresent(groundLayerMask);
        AppIntegrity.AssertPresent(laddersLayerMask);
        AppIntegrity.AssertPresent(enemiesLayerMask);
        AppIntegrity.AssertPresent(hazardsLayerMask);

        gravityScale = rb.gravityScale;
        particles.Stop();
        particles.Clear();

        Initialize();
    }

    void Update()
    {
        Animate();
        BlinkWhenDamaged();

        Utils.Elapse(ref timeInvincibleAfterTakingDamageElapsed, Time.deltaTime, timeInvincibleAfterTakingDamage);
        Utils.Elapse(ref timeFiring, Time.deltaTime, timeToShowFiringAnim);

        // manually control the particle system since it only plays when time is paused
        // see: https://answers.unity.com/questions/445843/how-to-emit-particle-or-un-pause-particle-when-tim.html
        // see: https://gist.github.com/AlexTiTanium/5676482
        if (Time.timeScale < 0.01f)
        {
            particles.Simulate(Time.unscaledDeltaTime, true, false);
        }
    }

    void FixedUpdate() {
        HandleMove();
        HandleSlowdown();
        HandleOrientation();
        HandleClimb();
        HandleJump();
        HandleFall();
    }

    void RefreshUI()
    {
        PlayerUI.instance.SetHealth(health);
        PlayerUI.instance.SetHasWeapon(hasWeaponBow);

        // TODO: update num coins
    }

    void HandleMove()
    {
        if (!isAlive || timeInvincibleAfterTakingDamageElapsed < 0.1f)
        {
            AudioManager.instance.Stop("PlayerFootsteps");
            return;
        }
    
        if (CanMoveX())
        {
            // Mathf.Epsilon is better than using a hard-coded zero value
            // see: https://docs.unity3d.com/ScriptReference/Mathf.Epsilon.html
            if (Mathf.Abs(moveInput.x) > Mathf.Epsilon)
            {
                // rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);

                // only add speed if player is not already moving that direction
                if (
                    moveInput.x > 0 && rb.velocity.x < moveSpeed ||
                    moveInput.x < 0 && rb.velocity.x > -moveSpeed
                )
                {
                    float newSpeed = Mathf.Clamp(rb.velocity.x + moveInput.x * moveSpeed, -moveSpeed, moveSpeed);
                    rb.velocity = new Vector2(newSpeed, rb.velocity.y);
                }
                isRunning = true;
                AudioManager.instance.Play("PlayerFootsteps");
            }
            else
            {
                isRunning = false;
                AudioManager.instance.Stop("PlayerFootsteps");
            }
        } else {
            AudioManager.instance.Stop("PlayerFootsteps");
        }
    }

    void HandleSlowdown()
    {
        if (isAlive && isRunning || timeInvincibleAfterTakingDamageElapsed < 0.1f) return;

        // add drag to slow down the player since we removed friction
        rb.velocity = new Vector2(rb.velocity.x - rb.velocity.x * moveSpeed * Time.fixedDeltaTime * (isAlive ? moveSlowdown : 1f), rb.velocity.y);
    }

    void HandleOrientation() {
        // handle sprite flip
        if ((isRunning || isClimbing) && Mathf.Abs(moveInput.x) > Mathf.Epsilon && orientationX != Mathf.Sign(rb.velocity.x))
        {
            orientationX = -orientationX;
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }

        // handle camera positioning
        if (isClimbing)
        {
            followCam.Follow = cameraCenter.transform;
            followCam.LookAt = cameraCenter.transform;
        }
        // else if (orientationX >= 0)
        // {
        //     followCam.Follow = cameraLeft.transform;
        //     followCam.LookAt = cameraLeft.transform;
        // }
        else
        {
            followCam.Follow = cameraRight.transform;
            followCam.LookAt = cameraRight.transform;
        }
    }

    bool CanMoveX() {
        if (isGrounded) return true;
        if (isClimbing) return false;
        if (canAirControl) return true;
        return false;
    }

    void HandleClimb() {
        if (!isAlive) return;

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
    }

    void HandleJump() {
        if (!isAlive) return;

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
            AudioManager.instance.Play("PlayerJump");
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
        // Utils.DebugDrawRect(transform.position, 0.9f);
        // RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.9f, Vector2.up, 0f, laddersLayerMask);
        // return col.IsTouchingLayers(laddersLayerMask) || hit.collider != null;
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
        if (!isAlive)
        {
            nextAnimState = ANIM_PLAYER_DEATH;
        }
        else if (timeFiring < timeToShowFiringAnim)
        {
            nextAnimState = ANIM_PLAYER_FIRING;
        }
        else if (isClimbing && isClimbIdle) {
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

    void BlinkWhenDamaged() {
        if (timeInvincibleAfterTakingDamageElapsed < timeInvincibleAfterTakingDamage)
        {
            if (Utils.shouldBlink(timeInvincibleAfterTakingDamageElapsed, damagedBlinkRate))
            {
                spriteRenderer.color = damagedBlinkColor;
            }
            else
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, damagedBlinkOpacity);
            }
        }
        else
        {
            spriteRenderer.color = new Color(1f, 1f, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (isAlive && Utils.LayerMaskContainsLayer(hazardsLayerMask, other.gameObject.layer)) {
            // send player in opposite direction
            rb.velocity *= -1.5f;
            rb.velocity += Vector2.up * jumpSpeed * 0.25f;
            rb.AddTorque(5f);
            Die();
        }
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) return;
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) return;
        isJumpPressed = value.isPressed;
    }

    void OnFire(InputValue value)
    {
        if (!isAlive) return;
        if (!hasWeaponBow) return;

        timeFiring = 0f;

        AudioManager.instance.Play("FireArrow");

        // In a real game, I would add a cool-down timer to control the firing rate.
        // I would also try and de-couple the player input from the firing action.
        // Since this is a tutorial, I'm keeping this ridiculously simple.
        GameObject instance = Instantiate(arrow, gun.transform.position, gun.transform.rotation);
        Rigidbody2D rbInstance = Utils.GetRequiredComponent<Rigidbody2D>(instance);

        // send arrow flying - matching player's current trajectory
        float dirX = Mathf.Sign(transform.localScale.x);
        rbInstance.velocity = new Vector3(arrowSpeed * dirX, 0f, 0f) + (Vector3)rb.velocity * 0.2f;

        // flip arrow
        instance.transform.localScale = transform.localScale;
    }
}
