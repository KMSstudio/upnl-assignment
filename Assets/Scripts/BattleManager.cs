using UnityEngine;

public class BattleManager : MonoBehaviour {
    public PlayerBehavior player1;

    void Update() {
        if (player1 && NetworkManager.Instance) {
            NetworkManager.Instance.SendChatMessage(player1.ToText());
        }
    }
}