using UnityEngine;

/// <summary>
/// Handles external string-based PlayerLocation updates and stores the latest one
/// </summary>
public class NetworkLocationController : MonoBehaviour, ILocationController {
    private PlayerLocation latestLocation;

    public PlayerLocation GetLatestLocation() => latestLocation;

    public void Receive(string line) {
        latestLocation = PlayerLocation.FromString(line);
    }

    void Update() {
        // if (!NetworkManager.Instance) return;
        // string lastMsg = null;
        // while (NetworkManager.Instance.HasMessage()) {
        //     lastMsg = NetworkManager.Instance.GetNextMessage(); }
        // if (!string.IsNullOrEmpty(lastMsg)) { Receive(lastMsg); }
    }
}