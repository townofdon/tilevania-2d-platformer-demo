using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyBat : MonoBehaviour
{
    [SerializeField] float awarenessRadius = 4f;
    [SerializeField] float lookDownDistance = 1f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float damageToPlayer = 5f;
    [SerializeField] float startingHealth = 10f;
    [SerializeField] float delayBeforeBeginSweep = 1f;
    [SerializeField] float sweepTimePeriod = 4f;
    [SerializeField] float sweepSpeed = 1f;
    [SerializeField] float randomMoveSpeed = 1f;
    [SerializeField] float randomMovePeriod = 1.5f;
    [SerializeField] Sound batWingSound;
    [SerializeField] AudioMixerGroup soundFXMix;

    // cached
    int playerLayerMask;
    int hazardsLayerMask;
    int enemiesLayerMask;
    int groundLayerMask;
    SpriteRenderer spriteRenderer;
    Animator anim;
    Rigidbody2D rb;
    Collider2D col;
    CircleCollider2D pdt;

    // cached values
    GameObject player;
    PlayerMovement playerMovement;
    CapsuleCollider2D playerCapsule;

    // state
    float health = 20f;
    bool isAlive = true;
    bool isAwake = false;
    bool isHit = false;
    float blockedTimer = 0f;
    float sweepTimer = 0f;
    float randomTimer = 0f;
    int sweepReversed = 0;
    Vector3 prevPosition = Vector3.zero;
    Vector3 sweepVelocity = Vector3.zero;
    Vector3 randomVelocity = Vector3.zero;
    Vector3 randomDirection = Vector3.zero;

    // ANIMATION STATES
    const string ANIM_BAT_SLEEPING = "BatSleeping";
    const string ANIM_BAT_FLYING = "BatFlying";
    const string ANIM_BAT_HIT = "BatHit";

    string currentAnimState;
    string nextAnimState;

    public void TakeDamage(float amount)
    {
        if (!isAlive) return;

        isHit = true;
        health -= amount;

        if (health <= 0) {
            Die();
            DeathSpin();
        } else {
            AudioManager.instance.Play("BatDamage");
        }
    }

    public bool IsAlive => isAlive;

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        anim.speed = 0f;
        rb.gravityScale = 1f;
        IgnoreEnemyCollisions();
        StartCoroutine(Remove());
        GameSession.instance.ProcessBatDeath();
        batWingSound.Stop();
        if (UnityEngine.Random.Range(0, 1) == 0)
            AudioManager.instance.Play("BatDeath1");
        else
            AudioManager.instance.Play("BatDeath2");
    }

    private void DeathSpin()
    {
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + 1f);
        // turn upside down
        transform.rotation = new Quaternion(0f, 0f, 90f, 0f);
    }

    void Start() {
        spriteRenderer = Utils.GetRequiredComponent<SpriteRenderer>(this.gameObject);
        anim = Utils.GetRequiredComponent<Animator>(this.gameObject);
        rb = Utils.GetRequiredComponent<Rigidbody2D>(this.gameObject);
        col = Utils.GetRequiredComponent<Collider2D>(this.gameObject);

        GameObject pdtGameObject = Utils.GetRequiredChild(this.gameObject, "PlayerDamageTrigger");
        pdt = Utils.GetRequiredComponent<CircleCollider2D>(pdtGameObject);
        player = GameObject.FindGameObjectWithTag("Player");
        playerCapsule = Utils.GetRequiredComponent<CapsuleCollider2D>(player);
        playerMovement = Utils.GetRequiredComponent<PlayerMovement>(player);

        playerLayerMask = LayerMask.GetMask("Player");
        hazardsLayerMask = LayerMask.GetMask("Hazards");
        enemiesLayerMask = LayerMask.GetMask("Enemies");
        groundLayerMask = LayerMask.GetMask("Ground");
        AppIntegrity.AssertNonZero(playerLayerMask);
        AppIntegrity.AssertNonZero(hazardsLayerMask);
        AppIntegrity.AssertNonZero(enemiesLayerMask);
        AppIntegrity.AssertNonZero(groundLayerMask);

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        batWingSound.setSource(audioSource, soundFXMix);
        audioSource.spatialBlend = 1f;
        audioSource.spatialize = true;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 25f;
        audioSource.spread = 180f;
        blockedTimer = delayBeforeBeginSweep;
        randomTimer = randomMovePeriod;

        Physics2D.IgnoreCollision(col, playerCapsule);

        isAlive = true;
        isAwake = false;
        health = startingHealth;
        rb.gravityScale = 0f;
        sweepTimePeriod = sweepTimePeriod * UnityEngine.Random.Range(0.8f, 1.2f);
        sweepSpeed = sweepSpeed * UnityEngine.Random.Range(0.8f, 1.2f);
        sweepReversed = (int)Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
        randomMoveSpeed = randomMoveSpeed * UnityEngine.Random.Range(0.8f, 1.2f);
        randomMovePeriod = randomMovePeriod * UnityEngine.Random.Range(0.8f, 1.2f);
    }

    void Update() {
        Animate();
        HandleSleeping();
    }

    void FixedUpdate() {
        Sweep();
        RandomMovement();
        FollowPlayer();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!isAlive) return;

        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player") {
            playerMovement.TakeDamage(damageToPlayer);
        }  
    }

    void OnTriggerStay2D(Collider2D other) {
        if (!isAlive) return;

        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player") {
            playerMovement.TakeDamage(damageToPlayer);
        }  
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (!isAlive) return;

        if (Utils.LayerMaskContainsLayer(hazardsLayerMask, other.gameObject.layer))
        {
            Die();
            DeathSpin();
            return;
        }
    }

    void HandleSleeping() {
        if (isAwake || !isAlive) return;

        rb.velocity *= 0.1f * Time.deltaTime;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, awarenessRadius, Vector2.down, lookDownDistance, playerLayerMask);
        if (hit.collider == null) return;

        player = hit.collider.gameObject;
        WakeUp();
    }

    void FollowPlayer() {
        if (!isAwake || !isAlive) return;

        Vector3 direction = player.transform.position - transform.position;
        rb.velocity = direction.normalized * GetFollowMagnitude() + sweepVelocity + randomVelocity;
    }

    void Sweep() {
        if (!isAwake || !isAlive) return;

        Vector2 diff = player.transform.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, diff.normalized, diff.magnitude, groundLayerMask);

        if (hit.collider != null) {
            blockedTimer = Mathf.Max(blockedTimer - Time.fixedDeltaTime, 0f);
        } else {
            blockedTimer = delayBeforeBeginSweep;
        }

        sweepTimer = (sweepTimer + Time.fixedDeltaTime) % sweepTimePeriod;
        sweepVelocity = new Vector3(
            Mathf.Cos(sweepTimer / sweepTimePeriod * 2f * Mathf.PI) * sweepReversed,
            Mathf.Sin(sweepTimer / sweepTimePeriod * 2f * Mathf.PI)
        ) * GetSweepMagnitude();
    }

    void RandomMovement() {
        if (!isAwake || !isAlive) return;

        if (randomTimer <= 0f) {
            randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
            randomTimer = randomMovePeriod;
        } else {
            randomTimer -= Time.fixedDeltaTime;
        }

        randomVelocity = randomDirection * GetRandomMoveMagnitude();
    }

    float GetFollowMagnitude() {
        // when out of attack range
        if (!IsWithinAttackDistance()) return 0.2f;
        // when not blocked
        if (!IsBlocked()) return moveSpeed;
        // when blocked
        return 0.2f;
    }

    float GetSweepMagnitude() {
        if (!IsWithinAttackDistance()) return sweepSpeed * 1.5f;
        if (!IsBlocked()) return 0.1f;
        return sweepSpeed;
    }

    float GetRandomMoveMagnitude() {
        if (!IsWithinAttackDistance()) return randomMoveSpeed;
        if (!IsBlocked()) return 0.1f;
        return randomMoveSpeed * 0.2f;
    }

    bool IsWithinAttackDistance() {
        return (transform.position - player.transform.position).magnitude <= awarenessRadius * 2f;
    }

    bool IsBlocked() {
        return blockedTimer <= 0f;
    }

    void WakeUp() {
        isAwake = true;
        if (UnityEngine.Random.Range(0, 1) == 0)
            AudioManager.instance.Play("BatAwake1");
        else
            AudioManager.instance.Play("BatAwake2");
        batWingSound.Play();
    }

    void IgnoreEnemyCollisions() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 100f, Vector2.down, 0.1f, enemiesLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            Physics2D.IgnoreCollision(col, hit.collider);
        }
    }

    void Animate() {
        if (!isAlive)
        {
            nextAnimState = ANIM_BAT_SLEEPING;
        }
        else if (!isAwake)
        {
            nextAnimState = ANIM_BAT_SLEEPING;
        }
        else if (isHit)
        {
            nextAnimState = ANIM_BAT_HIT;
            isHit = false;
        }
        else {
            nextAnimState = ANIM_BAT_FLYING;
        }

        if (currentAnimState == nextAnimState) return;

        currentAnimState = nextAnimState;
        anim.Play(currentAnimState);
    }

    IEnumerator Remove() {
        // wait for enemy to stop moving
        while (rb.velocity.magnitude > 0.1f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

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
