using System.Collections;
using UnityEngine;

public class ClientPlayerBehavior : PlayerBehavior {
    private bool isJumping = false;

    protected override void Update() { base.Update(); }
    protected override void FixedUpdate() { base.FixedUpdate(); }
    
    private bool ShouldJump(PlayerLocation loc) => !isJumping && !stance.crouch && loc.stance.jump;
    private bool ShouldCrouch(PlayerLocation loc) => !isJumping && !stance.crouch && loc.stance.crouch;
    private bool ShouldNotCrouch(PlayerLocation loc) => !isJumping && stance.crouch && !loc.stance.crouch;

    public void ApplyLocation(PlayerLocation loc) {
        transform.position = loc.loc;
        transform.rotation = loc.spin;

        if (!aiming && loc.aim) enableAimMotion();
        if (aiming && !loc.aim) disableAimMotion();
        
        if (ShouldJump(loc)) { DoJumpMotion(); }

        if (ShouldCrouch(loc)) {
            StopCrouchCoroutineIfRunning();
            stance = (true, stance.jump);
            crouchCoroutine = StartCoroutine(DoCrouch(true));
        } else if (ShouldNotCrouch(loc)) {
            StopCrouchCoroutineIfRunning();
            stance = (false, stance.jump);
            crouchCoroutine = StartCoroutine(DoCrouch(false));
        }

        velocity = Vector3.zero;
        stance = loc.stance;
        aiming = loc.aim;
    }
}