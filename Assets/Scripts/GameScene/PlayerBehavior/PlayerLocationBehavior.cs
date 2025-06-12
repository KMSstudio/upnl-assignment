using UnityEngine;

/// <summary>
/// PlayerLocationBehavior.cs
/// Controls a player via externally received PlayerLocation updates
/// </summary>
public class PlayerLocationBehavior : PlayerBehavior {
    public MonoBehaviour locationSource;

    protected override void Start() {
        base.Start();
    }

    // protected override void FixedUpdate() {
    //     base.FixedUpdate();
    //
    //     if (!NetworkManager.Instance) return;
    //
    //     string msg = NetworkManager.Instance.PeekLatestMessage();
    //     if (!string.IsNullOrEmpty(msg)) {
    //         Debug.Log("[PlayerLocationBehavior] Applying location: " + msg);
    //         var loc = PlayerLocation.FromString(msg);
    //         ApplyLocation(loc);
    //     }
    // }
}