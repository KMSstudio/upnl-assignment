using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class HostParse : MonoBehaviour {
    public GameObject playerPrefab;
    public MonoBehaviour inputSource;
    public int playerCount = 1;

    private List<PlayerBehavior> players = new List<PlayerBehavior>();
    private Queue<PlayerInput> inputQueue;

    void Start() {
        // CREATE PLAYERS
        for (int i = 0; i < playerCount; i++) {
            GameObject instance = Instantiate(playerPrefab);
            instance.transform.position = new Vector3(i * 2f, 0.5f, 0f);
            PlayerBehavior player = instance.GetComponent<PlayerBehavior>();
            if (player == null) { Debug.LogError("Player prefab must have PlayerBehavior component."); continue; }
            players.Add(player);
        }
        // CONNECT LOCAL INPUT QUEUE TO PLAYER[0]
        if (inputSource is IInputProvider provider) { inputQueue = provider.GetInputQueue(); }
        else { Debug.LogError("inputSource must implement IInputProvider."); }
    }

    void Update() {
        // LOCAL INPUT CONTROL
        if (players.Count > 0 && inputQueue != null) {
            var player0 = players[0];
            while (inputQueue.Count > 0) { player0.ApplyInput(inputQueue.Dequeue()); }
        }
        // SEND PLAYER LOCATIONS
        if (!ReferenceEquals(NetworkManager.Instance, null)) {
            StringBuilder sb = new StringBuilder("GAME ");
            for (int i = 0; i < players.Count; i++) { sb.Append($"{i}{{{players[i].ToText()}}} "); }
            NetworkManager.Instance.SendMessage(sb.ToString().Trim());
        }
    }
}