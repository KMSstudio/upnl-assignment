using System;
using UnityEngine;

public class ClientParse : MonoBehaviour {
    public GameObject playerPrefab;
    private PlayerBehavior player;
    private PlayerLocation latestLocation;

    void Start() {
        // 1. Instantiate player from prefab
        GameObject instance = Instantiate(playerPrefab);
        player = instance.GetComponent<PlayerBehavior>();
        if (player == null) {
            Debug.LogError("Player prefab must contain PlayerBehavior component.");
        }
    }

    void Update() {
        if (player == null || NetworkManager.Instance == null) return;
        Debug.Log("Client Parse Update Run");

        while (NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            Debug.Log(msg);
            if (!string.IsNullOrEmpty(msg)) {
                latestLocation = PlayerLocation.FromString(msg);
            }
        }

        if (latestLocation != null) {
            player.ApplyLocation(latestLocation);
        }
    }
}