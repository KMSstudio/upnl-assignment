using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

/// <summary>
/// Handles TCP-based networking: server/client setup, message transmission, and reception queue
/// </summary>
public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance;

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    private static readonly ConcurrentQueue<string> incomingMessages = new ConcurrentQueue<string>();

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    
    void Update() {
        UpdateLatestMessage();
    }

    public void StartServer(int port) {
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        server.BeginAcceptTcpClient(OnClientConnected, null);
        Debug.Log("Server Standby...");
    }

    public void ConnectToServer(string ip, int port) {
        client = new TcpClient();
        client.Connect(ip, port);
        stream = client.GetStream();
        StartReceive();
        Debug.Log("Server Connected...");
    }

    private void OnClientConnected(IAsyncResult ar) {
        client = server.EndAcceptTcpClient(ar);
        stream = client.GetStream();
        StartReceive();
        Debug.Log("Client Connected...");
    }

    private void StartReceive() {
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveLoop() {
        while (true) {
            try {
                if (stream == null || !stream.CanRead) continue;

                byte[] buffer = new byte[1024];
                int length = stream.Read(buffer, 0, buffer.Length);
                if (length == 0) continue;

                string message = Encoding.UTF8.GetString(buffer, 0, length);
                incomingMessages.Enqueue(message);
                Debug.Log("Receive: " + message);
            }
            catch (Exception e) {
                Debug.LogError("Receive Error: " + e.Message);
                break;
            }
        }
    }

    public void SendChatMessage(string msg) {
        if (stream != null && stream.CanWrite) {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    public bool HasMessage() {
        return !incomingMessages.IsEmpty;
    }

    public string GetNextMessage() {
        if (incomingMessages.TryDequeue(out string msg)) return msg;
        return null;
    }

    /// ✅ NEW: Safely get the latest message only
    public string GetLatestMessage() {
        string latest = null;
        while (incomingMessages.TryDequeue(out var msg)) {
            latest = msg;
        }
        return latest;
    }
    
    private static string cachedMessage = null; // 💡 캐시용 변수 추가

    public void UpdateLatestMessage() {
        while (incomingMessages.TryDequeue(out var msg)) {
            cachedMessage = msg;
        }
    }

    public string PeekLatestMessage() => cachedMessage;
}
