using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] GameObject spawnLocation;
    [SerializeField] SpriteRenderer spriteRenderer;

    void Start() {
        AppIntegrity.AssertPresent<GameObject>(spawnLocation);
        AppIntegrity.AssertPresent<SerializeField>(spriteRenderer);
        spriteRenderer.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            playerMovement.SetCheckpoint(spawnLocation.transform.position);
        }
    }
}
