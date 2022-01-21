using System.Collections;
using UnityEngine;

public class BounceUp : MonoBehaviour
{
    [Tooltip("Normal bounce impulse")]
    [Range(10f, 200f)] [SerializeField] float bounceForce = 20f;

    [Tooltip("Extra impulse when jumping")]
    [Range(1f, 5f)] [SerializeField] float jumpMultiplier = 2f;

    [Tooltip("The proper angle of attack to achieve a vertical bounce")]
    [SerializeField] float approachAngleThreshold = 70f;

    [Tooltip("Should this mushroom emit a huge sound when bounced upon?")]
    [SerializeField] bool huge = false;

    [Range(0f, 0.5f)]
    [SerializeField] float[] animStepTime = new float[4] { 0.20f, 0.15f, 0.08f, 0.08f };

    [Range(0f, 2f)]
    [SerializeField] float[] animScaleAmount = new float[2] { 0.5f, 1.2f };

    [Range(-1f, 1f)]
    [SerializeField] float[] animMoveAmount = new float[2] { -0.22f, 0.1f };

    // state
    Vector3 originalPosition;
    Vector3 currentPosition;
    Vector3 originalScale;
    Vector3 currentScale;
    Coroutine bounceAnimation;

    // cached fields
    private Rigidbody2D rbPlayer;
    private PlayerMovement playerMovement;

    void Start() {
        originalScale = transform.localScale;
        currentScale = transform.localScale;
        originalPosition = transform.position;
        currentPosition = transform.position;
    }

    void SetPlayerComponents(GameObject player) {
        if (!rbPlayer) {
            rbPlayer = player.GetComponent<Rigidbody2D>();
            AppIntegrity.AssertPresent(rbPlayer);
        }

        if (!playerMovement) {
            playerMovement = player.GetComponent<PlayerMovement>();
            AppIntegrity.AssertPresent(playerMovement);
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player") {
            float contactAngle = Vector3.Angle(other.contacts[0].normal, Vector3.down);

            SetPlayerComponents(other.gameObject);
            TriggerSound(other.gameObject);

            if (contactAngle <= approachAngleThreshold) {
                TriggerImpulseUp(other.gameObject);
            } else {
                TriggerImpulseOpposite(other.gameObject, other.contacts[0].normal);
            }
        }
    }

    void TriggerSound(GameObject player) {
        if (huge)
        {
            if (playerMovement.IsJumpPressed)
            {
                AudioManager.instance.Play("BounceJumpHuge");
            }
            else
            {
                AudioManager.instance.Play("BounceHuge");
            }
        }
        else
        {
            if (playerMovement.IsJumpPressed)
            {
                AudioManager.instance.Play("BounceJump");
            }
            else
            {
                AudioManager.instance.Play("Bounce");
            }
        }
    }

    void TriggerImpulseUp(GameObject player) {
        // gravity affects the player differently when they are considered jumping, so we need to cancel out a jump so that gravity is consistent
        // additionally, this disallows the player from lazily holding down the jump button
        float extraImpulse = playerMovement.IsJumpPressed ? jumpMultiplier : 1f;
        playerMovement.CancelJump();
        rbPlayer.AddForce(Vector2.up * bounceForce * extraImpulse, ForceMode2D.Impulse);
        if (bounceAnimation != null) StopCoroutine(bounceAnimation);
        bounceAnimation = StartCoroutine(AnimateBounceJiggle());
    }

    void TriggerImpulseOpposite(GameObject player, Vector2 direction) {
        float extraImpulse = playerMovement.IsJumpPressed ? jumpMultiplier : 1f;
        // direction is almost always 90. We want the player to rebound up slightly, hence the Vector2.up * N
        Vector2 flingDir = (direction * -1 + Vector2.up);
        // We make the player take damage only to make them momentarily unable to move (otherwise, their movement will immediately cancel out the AddForce below)
        playerMovement.TakeDamage(0f);
        rbPlayer.AddForce(flingDir.normalized * bounceForce * extraImpulse * 0.5f, ForceMode2D.Impulse);
    }

    IEnumerator AnimateBounceJiggle() {
        yield return AnimationStep(1, animScaleAmount[0], 0, animMoveAmount[0], animStepTime[0]);
        yield return AnimationStep(animScaleAmount[0], 1, animMoveAmount[0], 0, animStepTime[1]);
        yield return AnimationStep(1, animScaleAmount[1], 0, animMoveAmount[1], animStepTime[2]);
        yield return AnimationStep(animScaleAmount[1], 1, animMoveAmount[1], 0, animStepTime[3]);
    }

    IEnumerator AnimationStep(float scale0, float scale1, float move0, float move1, float duration) {
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            currentScale.y = Mathf.Lerp(
                originalScale.y * scale0,
                originalScale.y * scale1,
                t / duration);
            transform.localScale = currentScale;

            currentPosition.y = Mathf.Lerp(
                originalPosition.y + move0 * originalScale.y,
                originalPosition.y + move1 * originalScale.y,
                t / duration);
            transform.position = currentPosition;
            yield return null;
        }
    }
}
