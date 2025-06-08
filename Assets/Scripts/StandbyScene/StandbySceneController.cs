using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StandbySceneController : MonoBehaviour
{
    public TextMeshProUGUI playerCountText;
    public Button startButton;

    void Start() {
        startButton.gameObject.SetActive(NetworkManager.Instance.IsServer());
        startButton.onClick.AddListener(OnClickStart);
        InvokeRepeating(nameof(UpdatePlayerCount), 0f, 1f);
    }

    void Update() {
        while (NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            if (msg == "STAT {start}") {
                if (!NetworkManager.Instance.IsServer()) {
                    SceneManager.LoadScene("ClientGameScene");
                }
            }
        }
    }

    void UpdatePlayerCount() {
        int count = NetworkManager.Instance.TotalPlayers;
        playerCountText.text = $"Players: {count}";
    }

    void OnClickStart() {
        NetworkManager.Instance.SendChatMessage("STAT {start}");
        SceneManager.LoadScene("HostGameScene");
    }
}