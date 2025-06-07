using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles user chat input and displays incoming messages in UI
/// </summary>
public class ChatManager : MonoBehaviour {
    public TMP_InputField inputField;
    public TextMeshProUGUI chatLogText;
    public ScrollRect scrollRect;

    void Update() {
        while (NetworkManager.Instance && NetworkManager.Instance.HasMessage()) {
            string msg = NetworkManager.Instance.GetNextMessage();
            chatLogText.text += "\nClient: " + msg;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }

    public void OnSendClicked() {
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        NetworkManager.Instance.SendMessage(msg);
        chatLogText.text += "\nServer: " + msg;
        inputField.text = "";
        Canvas.ForceUpdateCanvases();   
        scrollRect.verticalNormalizedPosition = 0;
    }
}