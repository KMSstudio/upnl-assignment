using System.Collections;
using UnityEngine;

public class HostPlayerBehavior : PlayerBehavior {
    [Header("Host Fire Settings")]
    public GameObject bulletPrefab;
    [Tooltip("millisecond")]
    public float fireCooldown = 2000f;
    
    [Header("Player Movement")]
    [Tooltip("unit/sec")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private bool isJumping = false;

    private float lastFireTimeHost = -9999f;
    
    private void ApplyGravity(float deltaTime) {
        Vector3 v = velocity;
        if (v.y > terminalVelocity) {
            v.y += gravity * deltaTime;
            velocity = v;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (isJumping) ApplyGravity(Time.fixedDeltaTime);
        rb.MovePosition(rb.position + velocity * (moveSpeed * Time.fixedDeltaTime));
    }
    
    protected void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map")) {
            Vector3 v = velocity;
            v.y = 0f;
            velocity = v;
            isJumping = false;
        }
    }
    
    private bool ShouldJump(PlayerInput input) => !isJumping && !stance.crouch && input.stance.jump && !stance.jump;
    private bool ShouldCrouch(PlayerInput input) => !isJumping && !stance.crouch && input.stance.crouch;
    private bool ShouldNotCrouch(PlayerInput input) => !isJumping && stance.crouch && !input.stance.crouch;
    
    private void Jump() {
        Vector3 v = velocity;
        v.y = jumpForce;
        velocity = v;
        isJumping = true;
    }

    public void ApplyInput(PlayerInput input) {
        if(!isAlive){ return; }
        
        if (!aiming && input.aim) enableAimMotion();
        if (aiming && !input.aim) disableAimMotion();
        if (input.fire && CanFire()) Fire();

        if (ShouldJump(input)) { Jump(); DoJumpMotion(); }

        if (ShouldCrouch(input)) {
            StopCrouchCoroutineIfRunning();
            stance = (true, stance.jump);
            crouchCoroutine = StartCoroutine(DoCrouch(true));
        } else if (ShouldNotCrouch(input)) {
            StopCrouchCoroutineIfRunning();
            stance = (false, stance.jump);
            crouchCoroutine = StartCoroutine(DoCrouch(false));
        }

        Vector3 v = velocity;
        v.x = input.move.y;
        v.z = input.move.x;
        velocity = v;

        stance = input.stance;
        aiming = input.aim;
    }

    private bool CanFire() {
        return (Time.time * 1000f) - lastFireTimeHost >= fireCooldown;
    }

    private void Fire() {
        if (!bulletPrefab) return;
        lastFireTimeHost = Time.time * 1000f;
        Vector3 spawnPos = transform.position + transform.forward * 1.2f;
        Quaternion spawnRot = transform.rotation * Quaternion.Euler(90f, 0f, 0f);
        Instantiate(bulletPrefab, spawnPos, spawnRot);
    }
}