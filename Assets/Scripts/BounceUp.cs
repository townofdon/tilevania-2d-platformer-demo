using UnityEngine.InputSystem;
using UnityEngine;

public class BounceUp : MonoBehaviour
{
    [Tooltip("Normal bounce impulse")]
    [Range(10f, 200f)] [SerializeField] float bounceForce = 20f;

    [Tooltip("Extra impulse when jumping")]
    [Range(1f, 5f)] [SerializeField] float jumpMultiplier = 2f;

    [Tooltip("The proper angle of attack to achieve a vertical bounce")]
    [SerializeField] float approachAngleThreshold = 70f;

    [SerializeField] bool huge = false;

    // TODO: animate y-scale for simple bounce effect

    // cached fields
    private Rigidbody2D rbPlayer;
    private PlayerMovement playerMovement;

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
            if (playerMovement.IsJumpPressed())
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
            if (playerMovement.IsJumpPressed())
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
        float extraImpulse = playerMovement.IsJumpPressed() ? jumpMultiplier : 1f;
        rbPlayer.AddForce(Vector2.up * bounceForce * extraImpulse, ForceMode2D.Impulse);
    }

    void TriggerImpulseOpposite(GameObject player, Vector2 direction) {
        float extraImpulse = playerMovement.IsJumpPressed() ? jumpMultiplier : 1f;
        // direction is almost always 90. We want the player to rebound up slightly, hence the Vector2.up * N
        Vector2 flingDir = (direction * -1 + Vector2.up);
        // We make the player take damage only to make them momentarily unable to move (otherwise, their movement will immediately cancel out the AddForce below)
        playerMovement.TakeDamage(0f);
        rbPlayer.AddForce(flingDir.normalized * bounceForce * extraImpulse * 0.5f, ForceMode2D.Impulse);
    }
}
