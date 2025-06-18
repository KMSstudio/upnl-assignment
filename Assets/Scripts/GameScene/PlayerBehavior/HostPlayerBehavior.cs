using System.Collections;
using UnityEngine;

public class HostPlayerBehavior : PlayerBehavior {
    [Header("Host Fire Settings")]
    public GameObject bulletPrefab;

    private int relaodTime = 400;
    
    [Header("Player Movement")]
    [Tooltip("unit/sec")]
    public float moveSpeed = 2f;
    public float jumpForce = 4f;
    
    public float rotationSpeed = 240f;
    public float maxFwdSpeed = 4f;
    public float maxBwdSpeed = 2f;
    public float maxRhtSpeed = 2.5f;
    public float maxLftSpeed = 2.5f;
    public float acc = 50f;

    private bool isJumping = false;
    private float speed = 0f;

    private float lastFireTimeHost = -9999f;
    
    private void ApplyGravity(float deltaTime) {
        Vector3 v = velocity;
        if (v.y > terminalVelocity) {
            v.y += gravity * deltaTime;
            velocity = v;
        }
    }

    protected override void Start() {
        base.Start();
        BulletBehavior bullet = bulletPrefab.GetComponent<BulletBehavior>();
        relaodTime = bullet?.reload ?? 400;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        // MOVING AS GIVEN VELOCITY
        if (isJumping) ApplyGravity(Time.fixedDeltaTime);
        rb.MovePosition(rb.position + velocity * (moveSpeed * Time.fixedDeltaTime));
        // RESET X&Z AXIS ROATION
        Vector3 euler = transform.rotation.eulerAngles;
        if (Mathf.Abs(euler.x) > 0.01f || Mathf.Abs(euler.z) > 0.01f) { transform.rotation = Quaternion.Euler(0f, euler.y, 0f); }
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
        if (!isAlive) return;
        // MOTION
        if (!aiming && input.aim) enableAimMotion();
        if (aiming && !input.aim) disableAimMotion();
        if (input.fire && CanFire()) Fire();
        if (ShouldJump(input)) { Jump(); DoJumpMotion(); }
        // CROUCH MOTION
        if (ShouldCrouch(input)) {
            StopCrouchCoroutineIfRunning();
            stance = (true, stance.jump);
            crouchCoroutine = StartCoroutine(DoCrouch(true));
        } else if (ShouldNotCrouch(input)) {
            StopCrouchCoroutineIfRunning();
            stance = (false, stance.jump);
            crouchCoroutine = StartCoroutine(DoCrouch(false));
        }
        // ROTATION. Q = -1, E = +1
        float turnInput = input.rotation;
        if (Mathf.Abs(turnInput) > 0.1f) {
            float yRotation = turnInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, yRotation, 0f);
        }
        // ACCELERATION.
            // NORMALIZED TARGET VERTOR
            // REAL TARGET VECTOR
            // VELOCITY VECTOR
        float forwardInp = input.move.x;
        float strafeInp = input.move.y;
        Vector2 inputDir = new Vector2(strafeInp, forwardInp);
        Vector2 normalizedDir = inputDir.normalized;
        
        float fwdSpeed = (forwardInp >= 0f) ? maxFwdSpeed : maxBwdSpeed;
        float strafeSpeed = (strafeInp >= 0f) ? maxRhtSpeed : maxLftSpeed;
        Vector3 targetVelocity =
            transform.forward * normalizedDir.y * fwdSpeed +
            transform.right * normalizedDir.x * strafeSpeed;
        
        Vector3 velocityDiff = targetVelocity - new Vector3(velocity.x, 0f, velocity.z);
        Vector3 deltaVel = Vector3.ClampMagnitude(velocityDiff, acc * Time.deltaTime);
        if(velocity.y <= 0.1f) velocity += new Vector3(deltaVel.x, 0f, deltaVel.z);
        // STATE UPDATE
        stance = input.stance;
        aiming = input.aim;
    }

    private bool CanFire() {
        return (Time.time * 1000f) - lastFireTimeHost >= relaodTime;
    }

    private void Fire() {
        if (!bulletPrefab) return;
        lastFireTimeHost = Time.time * 1000f;
        Vector3 spawnPos = transform.position + transform.forward * 1.2f;
        Quaternion spawnRot = transform.rotation * Quaternion.Euler(90f, 0f, 0f);
        Instantiate(bulletPrefab, spawnPos, spawnRot);
    }
}