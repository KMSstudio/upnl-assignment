using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI myIpText;         // 내 IP 표시 텍스트
    public TMP_InputField ipInputField;      // 상대방 IP 입력 필드

    void Start()
    {
        myIpText.text = "YOUR IP: " + GetMyLocalIP();
    }

    // 버튼: 서버 시작
    public void OnClickStartServer()
    {
        NetworkManager.Instance.StartServer(7777);  // 포트는 7777로 고정
        SceneManager.LoadScene("StandbyScene");
    }

    // 버튼: 서버에 연결
    public void OnClickConnectToServer()
    {
        string ip = ipInputField.text;
        if (!string.IsNullOrEmpty(ip))
        {
            NetworkManager.Instance.ConnectToServer(ip, 7777);
            SceneManager.LoadScene("StandbyScene");
        }
    }

    // 로컬 IP 가져오기
    private string GetMyLocalIP()
    {
        string localIP = "Unknown";
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
}
