using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
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
    Rigidbody2D rb;
    CapsuleCollider2D capsule;

    // cached properties
    Vector3 groundCheck = new Vector3(0f, 0f);
    int groundLayerMask;

    // state
    bool isFacingRight = true;
    float turnAroundTimeElapsed = 10f;
    float startWaitTimeElapsed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        AppIntegrity.AssertPresent(rb);

        capsule = GetComponent<CapsuleCollider2D>();
        AppIntegrity.AssertPresent(capsule);

        groundLayerMask = LayerMask.GetMask("Ground");
        AppIntegrity.AssertPresent(groundLayerMask);

        groundCheck = CalcGroundCheckPos();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        AppIntegrity.AssertPresent(player);
        CapsuleCollider2D capsulePlayer = player.GetComponent<CapsuleCollider2D>();
        AppIntegrity.AssertPresent(capsulePlayer);

        // ignore collisions between main colliders used for physics for enemy <=> player
        Physics2D.IgnoreCollision(capsulePlayer, capsule);

        startWaitTimeElapsed = 0f;
    }

    void Update()
    {
        HandleMove();

        Elapse(ref turnAroundTimeElapsed, Time.deltaTime, turnAroundTime);
        Elapse(ref startWaitTimeElapsed, Time.deltaTime, startWaitTime);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player") {

            Debug.Log("Player collided with enemy");

            // if player hits from above, kill the enemy

            // if player hits from the side or below, damage the player && cancel 
        }
    }

    void HandleMove()
    {
        if (startWaitTimeElapsed < startWaitTime) return;

        Vector3 nextMove = (isFacingRight ? Vector3.right : Vector3.left) * moveSpeed;
        Vector3 nextGroundCheck = transform.position + (isFacingRight ? groundCheck : FlipX(groundCheck)) + nextMove * Time.deltaTime;

        if (debug) DebugDrawRect(transform.position + nextMove * Time.deltaTime);
        if (debug) DebugDrawRect(nextGroundCheck);

        if (turnAroundTimeElapsed < turnAroundTime) {
            if (debug) DebugDrawRect(transform.position, capsule.size.x, Color.yellow);
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
        transform.localScale = FlipX(transform.localScale);
    }

    bool CheckGrounded(Vector3 position)
    {
        if (debug) Debug.DrawRay(position, Vector3.down * 0.1f, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.1f, groundLayerMask);
        return hit.collider != null;
    }


    Vector3 CalcGroundCheckPos() {
        return new Vector3(
            capsule.size.x / 2f + groundCheckOffset + capsule.offset.x,
            -capsule.size.y / 2f + capsule.offset.y,
            0f
        );
    }

    // UTILITY FUNCTIONS - move to a separate file

    void Elapse(ref float timer, float amount, float max = Mathf.Infinity)
    {
        timer = Mathf.Min(timer + amount, max + Mathf.Epsilon);
    }

    Vector3 FlipX(Vector3 v) {
        v.x *= -1;
        return v;
    }

    void DebugDrawRect(Vector3 pos, float size, Color color)
    {
        Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y + size / 2, 0f), new Vector3(pos.x + size / 2, pos.y + size / 2, 0f), color);
        Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y + size / 2, 0f), new Vector3(pos.x - size / 2, pos.y - size / 2, 0f), color);
        Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y - size / 2, 0f), new Vector3(pos.x + size / 2, pos.y - size / 2, 0f), color);
        Debug.DrawLine(new Vector3(pos.x + size / 2, pos.y + size / 2, 0f), new Vector3(pos.x + size / 2, pos.y - size / 2, 0f), color);
    }
    void DebugDrawRect(Vector3 position, float size)
    {
        DebugDrawRect(position, size, Color.red);
    }
    void DebugDrawRect(Vector3 position, Color color)
    {
        DebugDrawRect(position, .1f, color);
    }
    void DebugDrawRect(Vector3 position)
    {
        DebugDrawRect(position, .1f, Color.red);
    }
}
