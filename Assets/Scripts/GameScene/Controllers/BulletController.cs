using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour {
    public static BulletControl Instance { get; private set; }
    private readonly HashSet<BulletBehavior> bullets = new();

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(BulletBehavior bullet) { if (!bullets.Contains(bullet)) { bullets.Add(bullet); } }
    public void Unregister(BulletBehavior bullet) { bullets.Remove(bullet); }
    public IEnumerable<BulletBehavior> GetAllBullets() => bullets;
    public void ClearAllBullets() {
        foreach (var bullet in bullets) { if (bullet != null) { Destroy(bullet.gameObject); } }
        bullets.Clear();
    }
    public int Count => bullets.Count;
}