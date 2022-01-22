using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTop : MonoBehaviour
{
    // cached selectors
    SpriteRenderer spriteRenderer;
    EdgeCollider2D edgeCollider;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        AppIntegrity.AssertPresent(spriteRenderer);

        edgeCollider = GetComponent<EdgeCollider2D>();
        AppIntegrity.AssertPresent(edgeCollider);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        AppIntegrity.AssertPresent(player);
        playerMovement = player.GetComponent<PlayerMovement>();
        AppIntegrity.AssertPresent(playerMovement);

        spriteRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        edgeCollider.enabled = !playerMovement.IsClimbing && !playerMovement.DidReleaseLadder;
    }
}
