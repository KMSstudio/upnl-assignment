using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player behavior based on structured input.
/// Applies state changes, motion updates, and handles jump/crouch logic.
/// 
/// Author: Serius <tomskang@naver.com>
/// Last Modified: 2025-05-28
/// </summary>
public class PlayerBehavior : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public MonoBehaviour inputSource;

    private Queue<PlayerInput> inputQueue;
    private PlayerMove move = new PlayerMove();
    private Rigidbody rb;
    
    private bool isJumping = false;
    private bool isCrouch = false;
    private Coroutine crouchCoroutine;

    void Start() {
        if (inputSource is IInputProvider provider) { inputQueue = provider.GetInputQueue(); }
        else { Debug.LogError("Input source must implement IInputProvider."); }
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        if (inputQueue == null) return;

        while (inputQueue.Count > 0) {
            var input = inputQueue.Dequeue();
            ApplyInput(input);
        }
    }

    void FixedUpdate() {
        if (isJumping) { move.ApplyGravity(Time.fixedDeltaTime); }
        rb.MovePosition(rb.position + move.velocity * (moveSpeed * Time.fixedDeltaTime));
    }

    void ApplyInput(PlayerInput input) {
        if (!move.aim && input.aim) enableAimMotion();
        if (move.aim && !input.aim) disableAimMotion();

        if (!move.fire && input.fire) enableFireMotion();
        if (move.fire && !input.fire) disableFireMotion();

        if (!isCrouch) {
            if (input.stance.jump && !move.stance.jump && !isJumping && !move.stance.crouch) { Jump(); }
        }

        if (!isJumping) {
            if (input.stance.crouch && !move.stance.crouch) {
                if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
                isCrouch = true;
                crouchCoroutine = StartCoroutine(DoCrouch(true));
            } else if (!input.stance.crouch && move.stance.crouch) {
                if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
                isCrouch = false;
                crouchCoroutine = StartCoroutine(DoCrouch(false));
            }
        }

        if (move.motion != input.motion) {
            if (input.motion == -1) enableLeanLeftMotion();
            else if (input.motion == 1) enableLeanRightMotion();
            else disableLeanMotion();
        }

        move.ApplyInput(input);
    }

    void Jump() {
        isJumping = true;
        move.ForceJump(jumpForce);
    }

    IEnumerator DoCrouch(bool crouch) {
        float duration = 0.2f;
        float time = 0f;
        float from = transform.localScale.y;
        float to = crouch ? 0.6f : 1.0f;

        Vector3 scale = transform.localScale;

        while (time < duration) {
            float t = time / duration;
            scale.y = Mathf.Lerp(from, to, t);
            transform.localScale = scale;
            time += Time.deltaTime;
            yield return null;
        }

        scale.y = to;
        transform.localScale = scale;
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map")) {
            isJumping = false;
            move.ResetYVelocity();
        }
    }

    void enableAimMotion()        { Debug.Log("Enable Aim Motion"); }
    void disableAimMotion()       { Debug.Log("Disable Aim Motion"); }

    void enableFireMotion()       { Debug.Log("Enable Fire Motion"); }
    void disableFireMotion()      { Debug.Log("Disable Fire Motion"); }

    void enableLeanLeftMotion()   { Debug.Log("Enable Lean Left Motion"); }
    void enableLeanRightMotion()  { Debug.Log("Enable Lean Right Motion"); }
    void disableLeanMotion()      { Debug.Log("Disable Lean Motion"); }
}
