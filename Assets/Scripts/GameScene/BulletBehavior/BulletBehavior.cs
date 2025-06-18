using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletBehavior : MonoBehaviour {
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 500;
    [Tooltip("millisecond")]
    public int reload = 400;

    public bool hasHit = false;
    private Rigidbody rb;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.up * speed;
        BulletManager.Instance?.Register(this);
        Destroy(gameObject, lifetime);
    }
    
    public string ToText() {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        return $"({pos.x},{pos.y},{pos.z}|{rot.x},{rot.y},{rot.z},{rot.w}|{speed}|{lifetime}|{damage})";
    }
    
    public static BulletBehavior FromText(string text, GameObject bulletPrefab) {
        if (!text.StartsWith("(") || !text.EndsWith(")")) {
            Debug.LogError("[FromText] Text must be enclosed in ()");
            return null;
        }

        string content = text.Substring(1, text.Length - 2); // remove ( and )
        string[] parts = content.Split('|');
        if (parts.Length != 5) {
            Debug.LogError("[FromText] Invalid format");
            return null;
        }

        Vector3 pos = ParseVector3(parts[0]);
        Quaternion rot = ParseQuaternion(parts[1]);
        float speed = float.Parse(parts[2]);
        float lifetime = float.Parse(parts[3]);
        int damage = int.Parse(parts[4]);

        GameObject obj = Object.Instantiate(bulletPrefab, pos, rot);
        BulletBehavior bullet = obj.GetComponent<BulletBehavior>();
        if (!bullet) {
            Debug.LogError("[FromText] Prefab missing BulletBehavior");
            Object.Destroy(obj);
            return null;
        }

        bullet.speed = speed;
        bullet.lifetime = lifetime;
        bullet.damage = damage;

        return bullet;
    }
    private static Vector3 ParseVector3(string s) {
        var t = s.Split(','); return new Vector3(float.Parse(t[0]), float.Parse(t[1]), float.Parse(t[2])); }
    private static Quaternion ParseQuaternion(string s) {
        var t = s.Split(','); return new Quaternion(float.Parse(t[0]), float.Parse(t[1]), float.Parse(t[2]), float.Parse(t[3])); }


    protected virtual void OnDestroy() {
        if (BulletManager.Instance) { BulletManager.Instance.Unregister(this); }
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Map")) { Destroy(gameObject); }
    }
}
