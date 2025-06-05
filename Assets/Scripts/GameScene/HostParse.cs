using UnityEngine;
using System.Collections.Generic;

public class HostParse : MonoBehaviour {
    public GameObject playerPrefab;
    public MonoBehaviour inputSource;
    private PlayerBehavior player;
    private Queue<PlayerInput> inputQueue;

    void Start() {
        // Create Player
        GameObject instance = Instantiate(playerPrefab);
        instance.transform.position = new Vector3(0f, 0.5f, 0f);
        // Conn Player Behavior
        player = instance.GetComponent<PlayerBehavior>();
        if (player == null) { Debug.LogError("Player prefab must have PlayerBehavior component."); }
        // Conn User Input Controller
        if (inputSource is IInputProvider provider) { inputQueue = provider.GetInputQueue(); }
        else { Debug.LogError("inputSource must implement IInputProvider."); }
    }

    void Update() {
        // Apply Input
        if (player != null && inputQueue != null) {
            while (inputQueue.Count > 0) {
                var input = inputQueue.Dequeue();
                player.ApplyInput(input);
            }
        }

        // Send Location
        if (player != null && NetworkManager.Instance != null) {
            NetworkManager.Instance.SendChatMessage(player.ToText());
        }
    }
}