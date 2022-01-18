using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] bool debug = true;
    [SerializeField] float damageAmount = 10f;

    int groundLayerMask;
    int enemiesLayerMask;

    // cached components
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    CapsuleCollider2D capsule;

    Coroutine remover;
    bool isRemoving = false;

    // cached velocity, pos etc.
    Vector2 prevVelocity = Vector2.zero;
    Vector2 prevPosition = Vector3.zero;
    float prevAngularVelocity = 0f;

    void Start()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
        enemiesLayerMask = LayerMask.GetMask("Enemies");
        AppIntegrity.AssertNonZero(groundLayerMask);
        AppIntegrity.AssertNonZero(enemiesLayerMask);

        rb = Utils.GetRequiredComponent<Rigidbody2D>(this.gameObject);
        spriteRenderer = Utils.GetRequiredComponent<SpriteRenderer>(this.gameObject);
        capsule = Utils.GetRequiredComponent<CapsuleCollider2D>(this.gameObject);
    }

    void Update() {
        if (rb.velocity.magnitude <= 0.1f)
        {
            if (remover == null) remover = StartCoroutine(Remove());
        }

        prevVelocity = rb.velocity;
        prevPosition = transform.position;
        prevAngularVelocity = rb.angularVelocity;
    }

    void FixedUpdate() {
        RaycastIgnoreDeadEnemies();

        if (capsule.IsTouchingLayers(groundLayerMask)) {
            rb.velocity *= 0.8f;
        }
    }

    void RaycastIgnoreDeadEnemies() {
        // get current direction
        Vector2 dir = rb.velocity.normalized;
        // raycast and see if it hits anything
        if (debug) Debug.DrawRay(transform.position, dir * 5f, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 5f, enemiesLayerMask);
        if (hit.collider == null) return;
        // if a dead enemy, ignore the collision
        Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
        if (!enemy.IsAlive) Physics2D.IgnoreCollision(capsule, hit.collider);
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (isRemoving) return;

        if (Utils.LayerMaskContainsLayer(groundLayerMask, other.gameObject.layer)) {
            if (remover == null) remover = StartCoroutine(OnGroundHit());
        }

        if (Utils.LayerMaskContainsLayer(enemiesLayerMask, other.gameObject.layer) && rb.velocity.magnitude > 0.1f) {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive) {
                enemy.TakeDamage(rb.velocity.magnitude / 10f * damageAmount);
                Destroy(gameObject);
            }
            else
            {
                // "Reset" the collision since the enemy was already dead
                // TODO: make this into a utility - would be very useful
                // see: https://answers.unity.com/questions/55711/cancel-a-collision.html
                Physics2D.IgnoreCollision(other.collider, other.otherCollider);
                rb.velocity = prevVelocity;
                rb.angularVelocity = prevAngularVelocity;
                transform.position = prevPosition + rb.velocity * Time.deltaTime;
            }
        }
    }

    IEnumerator OnGroundHit() {
        // wait for extra time to give the arrow a chance to hit something after stiking the ground
        yield return new WaitForSeconds(1f);
        remover = StartCoroutine(Remove());
    }

    IEnumerator Remove() {
        yield return new WaitForSeconds(.5f);

        // cancel if the arrow starts moving again (e.g. bounced off a wall)
        if (rb.velocity.magnitude > 0.1f) {
            remover = null;
            yield break;
        }

        isRemoving = true;
    
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
