using System.Collections;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour {
    [Header("Player Movement")]
    [Tooltip("unit/sec")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Fire Settings")]
    public GameObject bulletPrefab;
    [Tooltip("millisecond")]
    public float fireCooldown = 2000f;
    
    public event System.Action<int> OnDead;
    
    [HideInInspector]
    public int playerNo = 0;

    private float lastFireTime = -9999f;

    public Vector3 velocity { get; private set; }
    public (bool crouch, bool jump) stance { get; private set; }
    public bool aiming { get; private set; }

    public bool isJumping { get; private set; } = false;

    private float gravity = -9.8f;
    private float terminalVelocity = -20f;

    private Rigidbody rb;
    private Coroutine crouchCoroutine;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update() { }

    protected virtual void FixedUpdate() {
        if (isJumping) ApplyGravity(Time.fixedDeltaTime);
        rb.MovePosition(rb.position + velocity * (moveSpeed * Time.fixedDeltaTime));
    }

    public string ToText() => ToLocation().ToText();

    public void ApplyInput(PlayerInput input) {
        if (!aiming && input.aim) enableAimMotion();
        if (aiming && !input.aim) disableAimMotion();
        if (input.fire && CanFire()) { Fire(); }

        if (ShouldJump(input)) Jump();

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

    public void ApplyLocation(PlayerLocation loc) {
        transform.position = loc.loc;
        transform.rotation = loc.spin;

        if (!aiming && loc.aim) enableAimMotion();
        if (aiming && !loc.aim) disableAimMotion();

        if (loc.stance.jump && !stance.crouch && !isJumping) Jump();

        if (!isJumping) {
            if (!stance.crouch && loc.stance.crouch) {
                StopCrouchCoroutineIfRunning();
                stance = (true, stance.jump);
                crouchCoroutine = StartCoroutine(DoCrouch(true));
            } else if (stance.crouch && !loc.stance.crouch) {
                StopCrouchCoroutineIfRunning();
                stance = (false, stance.jump);
                crouchCoroutine = StartCoroutine(DoCrouch(false));
            }
        }

        velocity = Vector3.zero;
        stance = loc.stance;
        aiming = loc.aim;
    }

    public PlayerLocation ToLocation() {
        return new PlayerLocation { loc = transform.position, spin = transform.rotation, stance = stance, aim = aiming }; }

    public bool ShouldJump(PlayerInput input) => !isJumping && !stance.crouch && input.stance.jump && !stance.jump;
    public bool ShouldCrouch(PlayerInput input) => !isJumping && !stance.crouch && input.stance.crouch;
    public bool ShouldNotCrouch(PlayerInput input) => !isJumping && stance.crouch && !input.stance.crouch;

    public void ApplyGravity(float deltaTime) {
        Vector3 v = velocity;
        if (v.y > terminalVelocity) {
            v.y += gravity * deltaTime;
            velocity = v;
        }
    }

    protected void Jump() {
        Vector3 v = velocity;
        v.y = jumpForce;
        velocity = v;
        isJumping = true;
    }

    protected void StopCrouchCoroutineIfRunning() {
        if (crouchCoroutine != null) {
            StopCoroutine(crouchCoroutine);
            crouchCoroutine = null;
        }
    }

    protected IEnumerator DoCrouch(bool crouch) {
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

    protected void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map")) {
            Vector3 v = velocity;
            v.y = 0f;
            velocity = v;
            isJumping = false;
        }
    }
    
    protected bool CanFire() {
        return (Time.time * 1000f) - lastFireTime >= fireCooldown;
    }

    protected void Fire() {
        if (!bulletPrefab) { return; }
        lastFireTime = Time.time * 1000f;
        Vector3 spawnPos = transform.position + transform.forward * 1.2f;
        Quaternion spawnRot = transform.rotation * Quaternion.Euler(90f, 0f, 0f);;
        Instantiate(bulletPrefab, spawnPos, spawnRot);
    }
    
    public void Dead() {
        if (OnDead != null) OnDead.Invoke(playerNo);
        else Debug.LogWarning($"[PlayerBehavior] Player {playerNo} died, but no OnDead listener attached.");
    }

    protected void enableAimMotion() { ; }

    protected void disableAimMotion() { ; }
}
