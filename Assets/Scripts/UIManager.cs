using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI myIpText;
    public TMP_InputField ipInputField;

    void Start()
    {
        myIpText.text = ": " + GetMyLocalIP();
    }

    // START SERVER
    public void OnClickStartServer()
    {
        NetworkManager.Instance.StartServer(7777);  // 포트는 7777로 고정
        SceneManager.LoadScene("StandbyScene");
    }

    // ON SERVER CONN
    public void OnClickConnectToServer()
    {
        string ip = ipInputField.text;
        if (!string.IsNullOrEmpty(ip))
        {
            NetworkManager.Instance.ConnectToServer(ip, 7777);
            SceneManager.LoadScene("StandbyScene");
        }
    }

    // GET LOCAL IP
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
