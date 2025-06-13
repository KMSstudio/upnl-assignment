using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {
    public GameObject bulletPrefab;
    public static BulletManager Instance { get; private set; }

    private readonly HashSet<BulletBehavior> _bullets = new();
    private readonly List<BulletBehavior> _bulletsPending = new();

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(BulletBehavior bullet) {
        if (!_bulletsPending.Contains(bullet)) { _bulletsPending.Add(bullet); }
    }

    public void Unregister(BulletBehavior bullet) {
        _bullets.Remove(bullet); _bulletsPending.Remove(bullet);
    }
    
    public bool HasPendingBullets() { return _bulletsPending.Count > 0; }
    public string GetPendingBulletsAsStr() {
        var str = "BULT novel";
        foreach (var bullet in _bulletsPending) {
            if (bullet != null) { str += " " + bullet.ToText(); }
            _bullets.Add(bullet);
        }
        _bulletsPending.Clear();
        return str;
    }

    public string GetAllBulletsAsStr() {
        var str = "BULT total";
        foreach (var bullet in _bullets) { if (bullet != null) { str += " " + bullet.ToText(); } }
        return str;
    }
    
    public void HandleMsg(string msg) {
        if (string.IsNullOrWhiteSpace(msg) || bulletPrefab == null) return;
        msg = msg.Trim();
        if (msg.StartsWith("BULT novel")) {
            ParseAndRegisterBullets(msg.Substring("BULT novel".Length), bulletPrefab); }
        else if (msg.StartsWith("BULT total")) {
            ClearAllBullets();
            ParseAndRegisterBullets(msg.Substring("BULT total".Length), bulletPrefab);
        }
    }
    
    private void ParseAndRegisterBullets(string raw, GameObject prefab) {
        string[] tokens = raw.Split(')');
        foreach (string token in tokens) {
            string trimmed = token.Trim();
            if (trimmed.Length == 0) { continue; }
            BulletBehavior bullet = BulletBehavior.FromText(trimmed + ")", prefab);
            if (bullet != null) { Register(bullet); }
        }
    }

    public IEnumerable<BulletBehavior> GetAllBullets() => _bullets;

    public void ClearAllBullets() {
        foreach (var bullet in _bullets)
            if (bullet != null) Destroy(bullet.gameObject);

        _bullets.Clear();
        _bulletsPending.Clear();
    }

    public int Count => _bullets.Count;
}