using UnityEngine;

public class PlayerMove {
    public Vector3 velocity { get; private set; }
    public int motion { get; private set; }
    public (bool crouch, bool jump) stance { get; private set; }
    public bool aim { get; private set; }
    public bool fire { get; private set; }

    public bool isJumping { get; private set; } = false;
    public bool isCrouch { get; private set; } = false;

    private float gravity = -9.8f;
    private float terminalVelocity = -20f;

    private Transform trsf;

    public void BindTransform(Transform t) { trsf = t; }

    public void ApplyInput(PlayerInput input) {
        Vector3 v = velocity;
        v.x = input.move.y;
        v.z = input.move.x;
        velocity = v;

        motion = input.motion;
        stance = input.stance;
        aim = input.aim;
        fire = input.fire;

        isCrouch = stance.crouch;
    }

    public bool ShouldJump(PlayerInput input) {
        return !isJumping && !isCrouch && input.stance.jump && !stance.jump;
    }

    public bool ShouldCrouch(PlayerInput input) { return !isJumping && !isCrouch && input.stance.crouch && !stance.crouch; }
    public bool ShouldNotCrouch(PlayerInput input) { return !isJumping && !isCrouch && !input.stance.crouch && stance.crouch; }
    
    public bool ShouldJump(PlayerLocation loc) { return !isJumping && !isCrouch && loc.stance.jump && !stance.jump; }

    public bool ShouldCrouch(PlayerLocation loc) { return !isJumping && !isCrouch && loc.stance.crouch && !stance.crouch; }

    public bool ShouldNotCrouch(PlayerLocation loc) { return !isJumping && !isCrouch && !loc.stance.crouch && stance.crouch; }

    public void ForceJump(float power) {
        Vector3 v = velocity;
        v.y = power;
        velocity = v;
        isJumping = true;
    }

    public void CollisionWithMap() {
        Vector3 v = velocity;
        v.y = 0f;
        velocity = v;
        isJumping = false;
    }

    public void ApplyGravity(float deltaTime) {
        Vector3 v = velocity;
        if (v.y > terminalVelocity) {
            v.y += gravity * deltaTime;
            velocity = v;
        }
    }

    public void ApplyLocation(PlayerLocation loc) {
        if (!ReferenceEquals(trsf, null)) { trsf.position = loc.position; }
        velocity = Vector3.zero;
        motion = loc.motion;
        stance = loc.stance;
        aim = loc.aim;
        fire = loc.fire;
    }

    public PlayerLocation ToLocation() {
        return new PlayerLocation {
            position = trsf?.position ?? Vector3.zero,
            motion = motion,
            stance = stance,
            aim = aim,
            fire = fire
        };
    }
}
