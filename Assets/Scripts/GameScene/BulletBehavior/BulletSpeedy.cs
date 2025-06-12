using UnityEngine;

public class BulletSpeedy : BulletBehavior {
    protected override void Start() {
        speed = 40f;
        lifetime = 2f;
        damage = 300;
        base.Start();
    }
}