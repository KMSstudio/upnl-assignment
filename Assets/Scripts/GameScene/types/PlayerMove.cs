using UnityEngine;

/// <summary>
/// Represents the physical and action state of a player, updated by input and used for animation/network sync.
/// 
/// Author: Serius <tomskang@naver.com>
/// Last Modified: 2025-05-28
/// </summary>
public class PlayerMove {
    public Vector3 velocity { get; private set; }
    public int motion { get; private set; }         // 좌우 기울임
    public (bool crouch, bool jump) stance { get; private set; }    // 점프 및 앉기
    public bool aim { get; private set; }   // 타겟팅
    public bool fire { get; private set; }  // 발파

    private float gravity = -9.8f;
    private float terminalVelocity = -20f;

    public PlayerMove() {
        velocity = Vector3.zero;
        motion = 0;
        stance = (false, false);
        aim = false;
        fire = false;
    }

    public void ApplyInput(PlayerInput input) {
        Vector3 v = velocity;
        v.x = input.move.y; // left/right → x
        v.z = input.move.x; // forward/back → z
        velocity = v;

        motion = input.motion;
        stance = input.stance;
        aim = input.aim;
        fire = input.fire;
    }
    
    public void ApplyLocation(PlayerLocation loc) {
        velocity = Vector3.zero;
        motion = loc.motion;
        stance = loc.stance;
        aim = loc.aim;
        fire = loc.fire;
    }

    public void ApplyGravity(float deltaTime) {
        Vector3 v = velocity;
        if (v.y > terminalVelocity) {
            v.y += gravity * deltaTime;
            velocity = v;
        }
    }

    public void ForceJump(float power) {
        Vector3 v = velocity;
        v.y = power;
        velocity = v;
    }

    public void ResetYVelocity() {
        Vector3 v = velocity;
        v.y = 0f;
        velocity = v;
    }
}