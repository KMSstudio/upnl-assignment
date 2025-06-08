using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ClientParse : MonoBehaviour {
    public GameObject playerPrefab;
    public MonoBehaviour inputSource;

    private List<PlayerBehavior> players = new List<PlayerBehavior>();
    private Queue<PlayerInput> inputQueue;
    
    private int playerNo;
    private string playerId;
    private int playerCnt;

    void Start() {
        // NTWK MANAGER
        if (ReferenceEquals(NetworkManager.Instance, null)) { Debug.LogError("NetworkManager instance is null."); return; }
        playerNo = NetworkManager.Instance.PlayerNumber;
        playerId = NetworkManager.Instance.PlayerIdentifier;
        playerCnt = NetworkManager.Instance.TotalPlayers;
        Debug.Log($"[ClientParse] Start: PlayerNo={playerNo}, PlayerId={playerId}, Count={playerCnt}");
        // PLAYER BEHAVIOR
        for (int i = 0; i < playerCnt; i++) {
            GameObject instance = Instantiate(playerPrefab);
            PlayerBehavior player = instance.GetComponent<PlayerBehavior>();
            if (player == null) { Debug.LogError("Player prefab must contain PlayerBehavior component."); continue; }
            players.Add(player);
        }
        // USER INP CTRL
        if (inputSource is IInputProvider provider) { inputQueue = provider.GetInputQueue(); }
        else { Debug.LogError("inputSource must implement IInputProvider."); }
    }

    void Update() {
        if (ReferenceEquals(NetworkManager.Instance, null)) return;
        // NTWK INP
        while (NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            Debug.Log($"[ClientParse] Received: {msg}");
            if (msg.StartsWith("GAME")) { ParseGameMessage(msg); }
        }
        // USER INP
        if (inputQueue != null && inputQueue.Count > 0) {
            var input = inputQueue.Dequeue();
            string inputStr = input.ToString();
            string userMsg = $"USER {playerNo}{{{inputStr}}}";
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
