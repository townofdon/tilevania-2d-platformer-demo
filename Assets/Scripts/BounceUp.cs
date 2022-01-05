using UnityEngine.InputSystem;
using UnityEngine;

public class BounceUp : MonoBehaviour
{
    [Tooltip("Normal bounce impulse")]
    [Range(10f, 200f)] [SerializeField] float bounceForce = 20f;

    [Tooltip("Extra impulse when jumping")]
    [Range(1f, 5f)] [SerializeField] float jumpMultiplier = 2f;

    // cached fields
    private Rigidbody2D rbPlayer;
    private PlayerMovement playerMovement;

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player") {
            TriggerImpulseUp(other.gameObject);
        }
    }

    void TriggerImpulseUp(GameObject player) {
        if (!rbPlayer) {
            rbPlayer = player.GetComponent<Rigidbody2D>();
            AppIntegrity.AssertPresent(rbPlayer);
        }

        if (!playerMovement) {
            playerMovement = player.GetComponent<PlayerMovement>();
            AppIntegrity.AssertPresent(playerMovement);
        }

        float extraImpulse = playerMovement.IsJumpPressed() ? jumpMultiplier : 1f;
        rbPlayer.AddForce(Vector2.up * bounceForce * extraImpulse, ForceMode2D.Impulse);
    }
}
