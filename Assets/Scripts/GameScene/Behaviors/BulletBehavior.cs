using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifetime = 5f;

    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 중력은 아직 적용 안 함
        rb.linearVelocity = transform.up * speed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Map")) {
            Destroy(gameObject);
        }
    }
}
