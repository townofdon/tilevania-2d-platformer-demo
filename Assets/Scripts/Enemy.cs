using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Characteristics")]
    [Space]

    [Tooltip("How much health the enemy has at game start")]
    [SerializeField] float startHealth = 20f;

    [Tooltip("How long enemy should blink after taking damage")]
    [SerializeField] float damageBlinkDuration = 0.1f;

    [Tooltip("Rate at which to blink enemy damage")]
    [SerializeField] float damageBlinkRate = 0.05f;

    [Tooltip("Rate at which to blink enemy damage")]
    [SerializeField] Color damageBlinkColor = Color.red;

    [Tooltip("How much damage the enemy deals the player when the player gets hit")]
    [SerializeField] float attackDamage = 40f;
    
    [Tooltip("What angle of attack constitutes landing on the enemy's head? (degrees)")]
    [SerializeField] float topKillAngle = 40f;

    [Tooltip("How far the player goes flying after being hit")]
    [SerializeField] float damageRebound = 5f;

    [Header("Movement Control")]
    [Space]

    [Tooltip("How fast the enemy moves horizontally")]
    [SerializeField] float moveSpeed = 2f;

    [Tooltip("How long the character should wait when turning around")]
    [SerializeField] float turnAroundTime = 1f;

    [Tooltip("How long the character should wait before moving after it spawns")]
    [SerializeField] float startWaitTime = 1f;

    [Tooltip("How far away from the character's collider to check for the ground ending")]
    [SerializeField] float groundCheckOffset = .1f;

    [Header("Debug")]
    [Space]

    [SerializeField] bool debug = true;

    // cached components
    SpriteRenderer spriteRenderer;
    Animator animator;
    Rigidbody2D rb;
    PolygonCollider2D polygon;
    CapsuleCollider2D capsule;

    // cached player components
    PlayerMovement playerMovement;
    Rigidbody2D rbPlayer;
    CapsuleCollider2D capsulePlayer;

    // cached properties
    Vector3 groundCheck = new Vector3(0f, 0f);
    int groundLayerMask;
    int enemiesLayerMask;

    // state
    bool isAlive = true;
    bool isFacingRight = true;
    float turnAroundTimeElapsed = 10f;
    float startWaitTimeElapsed = 0f;
    float health = 20f;
    float timeLastDamaged = 0f;
    Vector2 prevVelocity = Vector2.zero;
    Vector2 prevPosition = Vector3.zero;
    float prevAngularVelocity = 0f;

    public void TakeDamage(float amount)
    {
        if (!isAlive) return;

        timeLastDamaged = 0f;
        health -= amount;

        Debug.Log("Enemy damage=" + amount + " health=" + health);

        if (health <= 0) {
            Die();
            DeathSpin();
        }

    }

    public bool IsAlive => isAlive;

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        animator.speed = 0f;
        StartCoroutine(Remove());
    }

    private void DeathSpin()
    {
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + 10f);
        // turn upside down
        transform.rotation = new Quaternion(0f, 0f, 90f, 0f);
    }

    private void Squished()
    {
        transform.localScale -= new Vector3(0f, 0.6f);
        // we also need to disable the capsule collider since it does not scale down; however a polygon collider does scale down
        polygon.isTrigger = false;
        capsule.enabled = false;
        Physics2D.IgnoreCollision(capsulePlayer, polygon);
    }

    void Start()
    {
        // components
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        polygon = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();

        // layers, ground stuff
        groundLayerMask = LayerMask.GetMask("Ground");
        enemiesLayerMask = LayerMask.GetMask("Enemies");
        groundCheck = CalcGroundCheckPos();

        // player instance && respective components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        capsulePlayer = player.GetComponent<CapsuleCollider2D>();
        rbPlayer = player.GetComponent<Rigidbody2D>();

        // ignore collisions between main colliders used for physics for enemy <=> player
        Physics2D.IgnoreCollision(capsulePlayer, capsule);

        // initialize
        startWaitTimeElapsed = 0f;
        timeLastDamaged = damageBlinkDuration + 1f;
        isAlive = true;
        health = startHealth;
        polygon.isTrigger = true;
        capsule.enabled = true;

        // verifications
        AppIntegrity.AssertPresent(spriteRenderer);
        AppIntegrity.AssertPresent(rb);
        AppIntegrity.AssertPresent(capsule);
        AppIntegrity.AssertPresent(polygon);
        AppIntegrity.AssertPresent(animator);
        AppIntegrity.AssertPresent(groundLayerMask);
        AppIntegrity.AssertPresent(enemiesLayerMask);
        AppIntegrity.AssertPresent(player);
        AppIntegrity.AssertPresent(playerMovement);
        AppIntegrity.AssertPresent(capsulePlayer);
        AppIntegrity.AssertPresent(rbPlayer);
    }

    void Update()
    {
        HandleMove();
        BlinkWhenDamaged();

        Utils.Elapse(ref turnAroundTimeElapsed, Time.deltaTime, turnAroundTime);
        Utils.Elapse(ref startWaitTimeElapsed, Time.deltaTime, startWaitTime);
        Utils.Elapse(ref timeLastDamaged, Time.deltaTime, damageBlinkDuration);

        prevVelocity = rb.velocity;
        prevPosition = transform.position;
        prevAngularVelocity = rb.angularVelocity;
    }

    // KNOWN ISSUE - enemies get stuck facing each other - I believe this is due to the fact that
    // they flip after colliding, but the OnCollisionEnter2D event never fires again since their colliders are still touching.
    // SOLUTION - refactor to perform a collision check on every frame
    void OnCollisionEnter2D(Collision2D other) {
        if (!isAlive) return;

        if (Utils.LayerMaskContainsLayer(enemiesLayerMask, other.gameObject.layer))
        {
            if (!other.gameObject.GetComponent<Enemy>().IsAlive) {
                // TODO: make this into a utility - would be very useful
                // see: https://answers.unity.com/questions/55711/cancel-a-collision.html
                Physics2D.IgnoreCollision(capsule, other.gameObject.GetComponent<CapsuleCollider2D>());
                Physics2D.IgnoreCollision(capsule, other.gameObject.GetComponent<PolygonCollider2D>());
                rb.velocity = prevVelocity;
                rb.angularVelocity = prevAngularVelocity;
                transform.position = prevPosition + rb.velocity * Time.deltaTime;
                return;
            }
        }

        Vector3 contactPoint = other.contacts[0].normal;
        float contactAngle = Vector3.Angle(contactPoint, Vector3.up);

        if (contactAngle > 70f && contactAngle < 110f)
        {
            // turn this thing around, mister
            Flip();
            rb.velocity = Vector3.zero;
            turnAroundTimeElapsed = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!isAlive) return;

        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player")
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position) - (Vector2)other.transform.position;
            float contactAngle = Vector3.Angle(contactPoint, Vector3.down);

            if (contactAngle <= topKillAngle)
            {
                Die();
                Squished();
                // player should bounce off of opponent
                rbPlayer.velocity = Vector3.Reflect(rbPlayer.velocity, Vector3.up);
            }
            else
            {
                TriggerPlayerDamage(other.gameObject, contactPoint);
            }

            // TODO: thrust back player in direction they came from

            // TODO: if player hits from above, kill the enemy

            // TODO: if player hits from the side or below, damage the player && cancel 
        }
    }

    void TriggerPlayerDamage(GameObject player, Vector3 contactPoint) {
        

        if (!playerMovement)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            AppIntegrity.AssertPresent(playerMovement);
        }

        if (playerMovement.TakeDamage(attackDamage))
        {
            rbPlayer.AddForce(contactPoint.normalized * -1 * damageRebound, ForceMode2D.Impulse);
        }
    }

    void HandleMove()
    {
        if (!isAlive)
        {
            // slow down the dang quesadilla - since we are using ice physics for everything
            rb.velocity = new Vector2(rb.velocity.x - rb.velocity.x * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
            return;
        }

        if (startWaitTimeElapsed < startWaitTime) return;

        Vector3 nextMove = (isFacingRight ? Vector3.right : Vector3.left) * moveSpeed;
        Vector3 nextGroundCheck = transform.position + (isFacingRight ? groundCheck : Utils.FlipX(groundCheck)) + nextMove * Time.deltaTime;

        if (debug) Utils.DebugDrawRect(transform.position + nextMove * Time.deltaTime);
        if (debug) Utils.DebugDrawRect(nextGroundCheck);

        if (turnAroundTimeElapsed < turnAroundTime) {
            if (debug) Utils.DebugDrawRect(transform.position, capsule.size.x, Color.yellow);
            return;
        };

        // check if the character is up against a cliff
        if (!CheckGrounded(nextGroundCheck))
        {
            Flip();
            rb.velocity = Vector3.zero;
            turnAroundTimeElapsed = 0f;
            return;
        }

        rb.velocity = new Vector3(nextMove.x, rb.velocity.y, 0f);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = Utils.FlipX(transform.localScale);
    }

    bool CheckGrounded(Vector3 position)
    {
        if (debug) Debug.DrawRay(position, Vector3.down * 0.1f, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.1f, groundLayerMask);
        return hit.collider != null;
    }

    void BlinkWhenDamaged() {
        if (timeLastDamaged < damageBlinkDuration)
        {
            if (Utils.shouldBlink(timeLastDamaged, damageBlinkRate))
            {
                spriteRenderer.color = damageBlinkColor;
            }
            else
            {
                spriteRenderer.color = new Color(1f, 1f, 1f);
            }
        }
        else
        {
            spriteRenderer.color = new Color(1f, 1f, 1f);
        }
    }


    Vector3 CalcGroundCheckPos() {
        return new Vector3(
            capsule.size.x / 2f + groundCheckOffset + capsule.offset.x,
            -capsule.size.y / 2f + capsule.offset.y,
            0f
        );
    }

    IEnumerator Remove() {
        // wait for enemy to stop moving
        while (rb.velocity.magnitude > 0.1f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        Color c = spriteRenderer.color;
        for (float alpha = 1f; alpha >= 0; alpha -= 0.01f)
        {
            c.a = alpha;
            spriteRenderer.color = c;
            yield return null;
        }

        Destroy(gameObject);

        yield return null;
    }
}
