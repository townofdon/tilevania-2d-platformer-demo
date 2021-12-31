using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 8f;

    Vector2 moveInput;
    Rigidbody2D rb;

    Animator anim;

    bool isRunning = false;

    // ANIMATION STATES - can also use an ENUM
    const string ANIM_PLAYER_IDLE = "PlayerIdle";
    const string ANIM_PLAYER_RUNNING = "PlayerRunning";
    string currentAnimState;
    string nextAnimState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        AppIntegrity.AssertPresent(rb);

        anim = GetComponent<Animator>();
        AppIntegrity.AssertPresent(anim);
    }

    void Update()
    {
        Run();
        FlipSprite();
        Animate();
    }

    void Run()
    {
        Vector2 vel = new Vector2(moveInput.x * runSpeed, rb.velocity.y);
        rb.velocity = vel;

        // Mathf.Epsilon is better than using a hard-coded zero value
        // see: https://docs.unity3d.com/ScriptReference/Mathf.Epsilon.html
        isRunning = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
    }

    void FlipSprite() {
        if (isRunning) {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
    }

    void Animate()
    {
        if (isRunning) {
            nextAnimState = ANIM_PLAYER_RUNNING;
        } else {
            nextAnimState = ANIM_PLAYER_IDLE;
        }

        if (currentAnimState == nextAnimState) return;

        currentAnimState = nextAnimState;
        anim.Play(currentAnimState);
    }


    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump()
    {

    }
}
