using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI myIpText;
    public TMP_InputField ipInputField;
    public TMP_InputField nameInputField;
    public bool verbose;

    void Start() {
        myIpText.text = ": " + GetMyLocalIP(); 
    } 

    // START SERVER
    public void OnClickStartServer() {
        if (verbose) { Debug.Log($"[UIManager] nameInputField: {nameInputField.text}"); }
        NetworkManager.Instance.SetName(nameInputField.text);
        NetworkManager.Instance.StartServer(7777);
        SceneManager.LoadScene("StandbyScene");
    }

    // ON SERVER CONN
    public void OnClickConnectToServer() {
        string ip = ipInputField.text;
        if (verbose) { Debug.Log($"[UIManager] nameInputField: {nameInputField.text}"); }
        if (!string.IsNullOrEmpty(ip)) {
            NetworkManager.Instance.SetName(nameInputField.text);
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
