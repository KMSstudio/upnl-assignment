using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletBehavior : MonoBehaviour {
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 500;

    private Rigidbody rb;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.up * speed;
        BulletControl.Instance?.Register(this);
        Destroy(gameObject, lifetime);
    }

    protected virtual void OnDestroy() {
        if (BulletControl.Instance) { BulletControl.Instance.Unregister(this); }
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Map")) { Destroy(gameObject); }
    }
}
