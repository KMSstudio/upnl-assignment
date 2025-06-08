using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class HostParse : MonoBehaviour {
    public GameObject playerPrefab;
    public MonoBehaviour inputSource;
    public bool verbose;

    private List<PlayerBehavior> players = new List<PlayerBehavior>();
    private Queue<PlayerInput> inputQueue;
    private int playerCount;

    void Start() {
        // NTWK MANAGER
        if (ReferenceEquals(NetworkManager.Instance, null)) { if (verbose) Debug.LogError("NetworkManager is null."); return; }
        playerCount = NetworkManager.Instance.TotalPlayers;
        if (verbose) Debug.Log($"[HostParse] Initializing with playerCount = {playerCount}");
        // PLAYER BEHAVIOR
        for (int i = 0; i < playerCount; i++) {
            Vector3 pos = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
            GameObject instance = Instantiate(playerPrefab, pos, Quaternion.identity);
            PlayerBehavior player = instance.GetComponent<PlayerBehavior>();
            if (player == null) { if (verbose) Debug.LogError("Player prefab must have PlayerBehavior component."); continue; }
            players.Add(player);
        }
        // USER INP CTRL
        if (inputSource is IInputProvider provider) { inputQueue = provider.GetInputQueue(); }
        else { if (verbose) Debug.LogError("inputSource must implement IInputProvider."); }
    }

    void Update() {
        // LOCAL INP CTRL FOR PLAYER[0]
        if (players.Count > 0 && inputQueue != null) {
            while (inputQueue.Count > 0) { players[0].ApplyInput(inputQueue.Dequeue()); }
        }
        // PARSE NTWK MSG
        while (NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            if (msg.StartsWith("USER")) {
                var match = Regex.Match(msg, @"USER\s+(\d+)\s*\{\s*(.*?)\s*\}");
                if (!match.Success) continue;

                int target = int.Parse(match.Groups[1].Value);
                string inputStr = match.Groups[2].Value;
                if (verbose) Debug.Log($"[HostParse] Sending input to player {target}{inputStr}");
                if (target >= 0 && target < players.Count) {
                    try { players[target].ApplyInput(PlayerInput.FromString(inputStr)); }
                    catch { if (verbose) Debug.LogWarning($"[HostParse] Failed to parse input for player {target}"); }
                }
            }
        }
        // SEND PLAYER LOC
        StringBuilder sb = new StringBuilder("GAME ");
        for (int i = 0; i < players.Count; i++) { sb.Append($"{i}{{{players[i].ToText()}}} "); }
        NetworkManager.Instance.SendChatMessage(sb.ToString().Trim());
    }
}
