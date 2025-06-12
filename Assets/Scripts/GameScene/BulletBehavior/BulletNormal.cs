using UnityEngine;

public class BulletNormal : BulletBehavior {
    protected override void Start() {
        speed = 20f;
        lifetime = 3f;
        damage = 500;
        base.Start();
    }
}