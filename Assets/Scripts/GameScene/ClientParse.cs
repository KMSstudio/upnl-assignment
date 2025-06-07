using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ClientParse : MonoBehaviour {
    public GameObject playerPrefab;
    private List<PlayerBehavior> players = new List<PlayerBehavior>();

    void Start() {
        // 1. 일단 플레이어 하나만 생성 (하드코딩)
        GameObject instance = Instantiate(playerPrefab);
        PlayerBehavior player = instance.GetComponent<PlayerBehavior>();
        if (player == null) {
            Debug.LogError("Player prefab must contain PlayerBehavior component.");
        }
        players.Add(player);
    }

    void Update() {
        if (ReferenceEquals(NetworkManager.Instance, null)) return;
        while (NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            Debug.Log($"[ClientParse] Received: {msg}");

            // 2. "GAME 0(...)" 형태 파싱
            if (msg.StartsWith("GAME")) {
                ParseGameMessage(msg);
            }
        }
    }

    void ParseGameMessage(string msg) {
        // PREFIX
        if (!msg.StartsWith("GAME")) return;
        string payload = msg.Substring(5);
        // EXECUTE
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