using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ClientParse : MonoBehaviour {
    public GameObject playerPrefab;
    public MonoBehaviour inputSource;

    private List<PlayerBehavior> players = new List<PlayerBehavior>();
    private Queue<PlayerInput> inputQueue;

    void Start() {
        // NETWORK MANAGER
        if (ReferenceEquals(NetworkManager.Instance, null)) { Debug.LogError("NetworkManager instance is null."); return; }
        int playerCount = NetworkManager.Instance.TotalPlayers;
        int myPlayerNo = NetworkManager.Instance.PlayerNumber;
        string myId = NetworkManager.Instance.PlayerIdentifier;
        Debug.Log($"[ClientParse] Start: PlayerNo={myPlayerNo}, PlayerId={myId}, Count={playerCount}");
        // PLAYER BEHAVIOR
        for (int i = 0; i < playerCount; i++) {
            GameObject instance = Instantiate(playerPrefab);
            PlayerBehavior player = instance.GetComponent<PlayerBehavior>();
            if (player == null) { Debug.LogError("Player prefab must contain PlayerBehavior component."); continue; }
            players.Add(player);
        }
        // USER INPUT CONTROLLER
        if (inputSource is IInputProvider provider) { inputQueue = provider.GetInputQueue(); }
        else { Debug.LogError("inputSource must implement IInputProvider."); }
    }

    void Update() {
        if (ReferenceEquals(NetworkManager.Instance, null)) return;
        // NETWORK INPUT
        while (NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            Debug.Log($"[ClientParse] Received: {msg}");
            if (msg.StartsWith("GAME")) { ParseGameMessage(msg); }
        }
        // USER INPUT
        if (inputQueue != null && inputQueue.Count > 0) {
            var input = inputQueue.Dequeue();
            int myNo = NetworkManager.Instance.PlayerNumber;
            string inputStr = input.ToString();
            string userMsg = $"USER {myNo}{{{inputStr}}}";
            NetworkManager.Instance.SendChatMessage(userMsg);
        }
    }

    void ParseGameMessage(string msg) {
        if (!msg.StartsWith("GAME")) return;
        string payload = msg.Substring(5);
        var matches = Regex.Matches(payload, @"(\d+)\s*\{\s*(.*?)\s*\}");
        foreach (Match match in matches) {
            int playerNo = int.Parse(match.Groups[1].Value);
            string locationText = match.Groups[2].Value;
            if (playerNo < 0 || playerNo >= players.Count) { Debug.LogWarning($"[ClientParse] Invalid player index: {playerNo}"); continue; }
            Debug.Log($"[ClientParse] Player {playerNo}: {locationText}");
            try { players[playerNo].ApplyLocation(PlayerLocation.FromString(locationText)); }
            catch (Exception e) { Debug.LogError($"[ClientParse] Failed to parse location for player {playerNo}: {e.Message}"); }
        }
    }
}
