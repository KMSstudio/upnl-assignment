using UnityEngine;

public class BulletHeavy : BulletBehavior {
    protected override void Start() {
        speed = 10f;
        lifetime = 5f;
        damage = 1000;
        base.Start();
    }
}