using UnityEngine;

public class Bullet : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 5f;

    void Start() {
        Destroy(gameObject, lifetime);
    }

    void Update() {
        transform.position += transform.up * (speed * Time.deltaTime);
    }
}
