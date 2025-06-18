using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameResultUIManager : MonoBehaviour {
    public GameObject textPrefab;
    public Transform container;

    void Start() {
        List<int> ranking = GameResultData.Ranking;
        if (ranking == null || ranking.Count == 0) { Debug.LogWarning("[GameResultUI] No ranking data."); return; }
        for (int i = 0; i < ranking.Count; i++) {
            int playerNo = ranking[i];
            var playerInfo = PlayerInfoList.GetPlayer(playerNo);
            string playerName = playerInfo != null ? playerInfo.PlayerName : $"Player {playerNo}";
            GameObject entry = Instantiate(textPrefab, container);
            string text = $"#{i + 1} - {playerName}";
            var tmpComponent = entry.GetComponent<TextMeshProUGUI>();
            if (tmpComponent != null) tmpComponent.text = text;
        }
    }
}
