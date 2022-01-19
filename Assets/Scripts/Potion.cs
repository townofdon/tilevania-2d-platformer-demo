using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    [SerializeField] float healingAmount = 10f;

    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player" || other.gameObject.name == "Player") {
            if (playerMovement == null) playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement.TakeHealth(healingAmount)) {
                AudioManager.instance.Play("Potion");
                Destroy(gameObject);
            }
        }    
    }
}
