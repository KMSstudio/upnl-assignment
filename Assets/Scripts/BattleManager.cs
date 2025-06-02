using UnityEngine;

public class BattleManager : MonoBehaviour {
    public PlayerBehavior player1;

    private float sendInterval = 0.05f; // 50ms
    private float timer = 0f;

    void Update() {
        if (player1 && NetworkManager.Instance) {
            NetworkManager.Instance.SendChatMessage(player1.ToText());
        }
    }
}