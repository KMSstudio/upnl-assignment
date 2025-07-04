using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ClientParse : MonoBehaviour {
    public GameObject playerPrefab;
    public MonoBehaviour inputSource;

    private List<ClientPlayerBehavior> players = new List<ClientPlayerBehavior>();
    private Queue<PlayerInput> inputQueue;

    private int playerNo;
    private string playerId;
    private int playerCnt;
    
    
    public static ClientParse Instance { get; private set; }
    void Awake() {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; } Instance = this; }

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
            ClientPlayerBehavior player = instance.GetComponent<ClientPlayerBehavior>();
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
            else if (msg.StartsWith("DEAD")) {
                var match = Regex.Match(msg, @"DEAD\s*(\d+)");
                if (match.Success) {
                    int deadPlayer = int.Parse(match.Groups[1].Value);
                    players[deadPlayer].gameObject.SetActive(false);
                }
            }
            else if (msg.StartsWith("BULT")) { BulletManager.Instance?.HandleMsg(msg); }
            else if (msg.StartsWith("STAT")) { ParseStatusMessage(msg); }
        }
        // USER INP
        if (inputQueue != null && inputQueue.Count > 0) {
            var input = inputQueue.Dequeue();
            string inputStr = input.ToString();
            string userMsg = $"USER {playerNo}{{{inputStr}}}";
            NetworkManager.Instance.SendChatMessage(userMsg);
        }
    }
    
    public GameObject GetLocalPlayer() {
        int localPlayerNo = NetworkManager.Instance?.PlayerNumber ?? 0;
        if (localPlayerNo >= 0 && localPlayerNo < players.Count)
            return players[localPlayerNo].gameObject;
        return null;
    }

    void ParseGameMessage(string msg) {
        if (!msg.StartsWith("GAME")) return;
        string payload = msg.Substring(5);
        var matches = Regex.Matches(payload, @"(\d+)\s*\{\s*(.*?)\s*\}");
        // EXECUTE
        foreach (Match match in matches) {
            int playerNo = int.Parse(match.Groups[1].Value);
            string locationText = match.Groups[2].Value;
            if (playerNo < 0 || playerNo >= players.Count) { Debug.LogWarning($"[ClientParse] Invalid player index: {playerNo}"); continue; }
            Debug.Log($"[ClientParse] Player {playerNo}: {locationText}");
            try { players[playerNo].ApplyLocation(PlayerLocation.FromText(locationText)); }
            catch (Exception e) { Debug.LogError($"[ClientParse] Failed to parse location for player {playerNo}: {e.Message}"); }
        }
    }
    
    void ParseStatusMessage(string msg) {
        var match = Regex.Match(msg, @"STAT\s*\{gamefin\s*([\d\s]+)\}");
        if (!match.Success) { Debug.LogWarning("[ClientParse] Invalid STAT message format."); return; }
        // PARSE
        string[] tokens = match.Groups[1].Value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        List<int> ranking = new();
        foreach (string token in tokens) { if (int.TryParse(token, out int num)) ranking.Add(num); }
        if (ranking.Count == 0) { Debug.LogWarning("[ClientParse] No ranking data in STAT message."); return; }
        GameResultData.Ranking = ranking;
        // EXECUTE
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameResultScene");
    }
}
