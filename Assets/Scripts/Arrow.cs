using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float damageAmount = 10f;

    int enemiesLayerMask;

    // cached components
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;

    Coroutine remover;
    bool isRemoving = false;

    void Start()
    {
        enemiesLayerMask = LayerMask.GetMask("Enemies");
        AppIntegrity.AssertNonZero(enemiesLayerMask);

        rb = Utils.GetRequiredComponent<Rigidbody2D>(this.gameObject);
        spriteRenderer = Utils.GetRequiredComponent<SpriteRenderer>(this.gameObject);
    }

    void Update() {
        if (rb.velocity.magnitude <= 0f)
        {
            if (remover == null) remover = StartCoroutine(Remove());
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (isRemoving) return;

        if (Utils.LayerMaskContainsLayer(enemiesLayerMask, other.gameObject.layer) && rb.velocity.magnitude > 0f) {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive) {
                enemy.TakeDamage(rb.velocity.magnitude / 10f * damageAmount);
                Destroy(gameObject);
            }
        }
    }

    IEnumerator Remove() {
        Debug.Log("WAITING FOR 2 SECONDS");

        yield return new WaitForSeconds(2f);

        if (rb.velocity.magnitude > 0f) yield break;

        isRemoving = true;

        Debug.Log("FADING");
    
        Color c = spriteRenderer.color;
        for (float alpha = 1f; alpha >= 0; alpha -= 0.01f)
        {
            c.a = alpha;
            spriteRenderer.color = c;
            yield return null;
        }

        Debug.Log("DESTROYING GAME OBJECT");

        Destroy(gameObject);

        yield return null;
    }
}
