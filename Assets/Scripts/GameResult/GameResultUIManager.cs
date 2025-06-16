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
            GameObject entry = Instantiate(textPrefab, container);
            string text = $"#{i + 1} - Player {ranking[i]}";
            var tmpComponent = entry.GetComponent<TextMeshProUGUI>();
            if (tmpComponent != null) tmpComponent.text = text;
        }
    }
}
