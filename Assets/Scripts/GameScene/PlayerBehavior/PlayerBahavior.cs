using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerBehavior.cs
/// Handles base movement, jump, crouch, and motion state of a player
/// </summary>
public class PlayerBehavior : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    protected PlayerMove move = new PlayerMove();
    protected Rigidbody rb;

    protected bool isJumping = false;
    protected bool isCrouch = false;
    protected Coroutine crouchCoroutine;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update() { }

    protected virtual void FixedUpdate() {
        if (isJumping) move.ApplyGravity(Time.fixedDeltaTime);
        rb.MovePosition(rb.position + move.velocity * (moveSpeed * Time.fixedDeltaTime));
    }

    public string ToText() {
        Vector3 pos = transform.position;
        string motionStr = move.motion switch {
            -1 => "LeanLeft",
            1 => "LeanRight",
            _ => "Neutral"
        };
        string stanceStr = "";
        if (move.stance.crouch) stanceStr += "Crouch ";
        if (move.stance.jump) stanceStr += "Jump ";
        string aimStr = move.aim ? "Aiming " : "";
        string fireStr = move.fire ? "Firing" : "";
        string stateStr = $"{motionStr} {stanceStr}{aimStr}{fireStr}".Trim();
        return $"POS({pos.x:F2},{pos.y:F2},{pos.z:F2}) STATE({stateStr})";
    }

    public void ApplyInput(PlayerInput input) {
        if (!move.aim && input.aim) enableAimMotion();
        if (move.aim && !input.aim) disableAimMotion();
        if (!move.fire && input.fire) enableFireMotion();
        if (move.fire && !input.fire) disableFireMotion();

        if (!isCrouch) {
            if (input.stance.jump && !move.stance.jump && !isJumping && !move.stance.crouch) Jump();
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
    
    public void ApplyLocation(PlayerLocation loc) {
        transform.position = loc.position;
        
        if (!move.aim && loc.aim) enableAimMotion();
        if (move.aim && !loc.aim) disableAimMotion();
        if (!move.fire && loc.fire) enableFireMotion();
        if (move.fire && !loc.fire) disableFireMotion();

        if (!isCrouch) {
            if (loc.stance.jump && !move.stance.jump && !isJumping && !move.stance.crouch) Jump();
        }

        if (!isJumping) {
            if (loc.stance.crouch && !move.stance.crouch) {
                if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
                isCrouch = true;
                crouchCoroutine = StartCoroutine(DoCrouch(true));
            } else if (!loc.stance.crouch && move.stance.crouch) {
                if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
                isCrouch = false;
                crouchCoroutine = StartCoroutine(DoCrouch(false));
            }
        }
        
        move.ApplyLocation(loc);
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
