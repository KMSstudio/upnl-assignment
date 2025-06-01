using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Concurrent;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI chatLogText;
    public ScrollRect scrollRect;

    private static ConcurrentQueue<string> incomingMessages = new ConcurrentQueue<string>();

    void Update() {
        while (incomingMessages.TryDequeue(out var msg)) {
            chatLogText.text += "\n상대: " + msg;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0; // 자동 스크롤 맨 아래로
        }
    }

    public void OnSendClicked()
    {
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        NetworkManager.Instance.SendChatMessage(msg); // 메시지 전송
        chatLogText.text += "\n나: " + msg;
        inputField.text = "";
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    public static void ReceiveMessage(string msg)
    {
        incomingMessages.Enqueue(msg);
    }
}
