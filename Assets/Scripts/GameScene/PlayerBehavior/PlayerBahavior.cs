using System.Collections;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour {
    [Header("Player Settings")]
    [Tooltip("초기 체력")]
    public int maxHealth = 1000;
    private int currentHealth;
    
    public event System.Action<int> OnDead;
    
    [HideInInspector]
    public int playerNo = 0;
    public bool isAlive = true;

    public Vector3 velocity { get; protected set; }
    public (bool crouch, bool jump) stance { get; protected set; }
    public bool aiming;

    protected float gravity = -9.8f;
    protected float terminalVelocity = -20f;

    protected Rigidbody rb;
    protected Coroutine crouchCoroutine;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    protected virtual void Update() { }
    protected virtual void FixedUpdate() { }

    public string ToText() => ToLocation().ToText();

    public PlayerLocation ToLocation() {
        return new PlayerLocation { loc = transform.position, spin = transform.rotation, stance = stance, aim = aiming }; }

    protected void DoJumpMotion() { ; }

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

    protected void enableAimMotion() { }
    protected void disableAimMotion() { }

    protected virtual void OnCollisionEnter(Collision collision) { }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Bullet")) {
            var bullet = other.GetComponent<BulletBehavior>();
            if (bullet == null || bullet.hasHit) return;

            bullet.hasHit = true;
            TakeDamage(bullet.damage);
            Destroy(other.gameObject);
        }
    }
    
    public void TakeDamage(int damage) {
        if (currentHealth <= 0) { return; }
        currentHealth -= damage;
        Debug.Log($"[PlayerBehavior] Player {playerNo} took {damage} damage. HP = {currentHealth}");
        if (currentHealth <= 0) { Dead(); }
    }
    
    public void Dead() {
        isAlive = false;
        if (OnDead != null) { OnDead.Invoke(playerNo); }
        else Debug.LogWarning($"[PlayerBehavior] Player {playerNo} died, but no OnDead listener attached.");
    }
}
