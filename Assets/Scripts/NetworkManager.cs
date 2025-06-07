using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
    private readonly List<TcpClient> connectedClients = new List<TcpClient>();

    // PUBLIC STATE
    public int PlayerNumber { get; private set; } = -1;
    public int TotalPlayers => playerCount;

    // PRIVATE STATE
    private int playerCount = 1;
    private bool isServer = false;

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void StartServer(int port) {
        isServer = true; PlayerNumber = 0; playerCount = 1;
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        server.BeginAcceptTcpClient(OnClientConnected, null);
        Debug.Log("Server Standby...");
    }

    public void ConnectToServer(string ip, int port) {
        client = new TcpClient();
        client.Connect(ip, port);
        stream = client.GetStream();
        receiveThread = new Thread(() => ReceiveLoop(client));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("Server Connected...");
    }

    private void OnClientConnected(IAsyncResult ar) {
        TcpClient newClient = server.EndAcceptTcpClient(ar);
        connectedClients.Add(newClient);
        NetworkStream newStream = newClient.GetStream();
        // SEND PLAYER NO
        int assignedPlayerNo = playerCount++;
        Debug.Log($"[NetworkManager] New client connected. Assigned: playerno={assignedPlayerNo}");
        string message = $"NTWK (playerno={assignedPlayerNo})";
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        newStream.Write(buffer, 0, buffer.Length);
        // START RECEIVING
        Thread thread = new Thread(() => ReceiveLoop(newClient));
        thread.IsBackground = true;
        thread.Start();
        server.BeginAcceptTcpClient(OnClientConnected, null);
    }

    private void ReceiveLoop(TcpClient targetClient) {
        NetworkStream s = targetClient.GetStream();
        while (true) {
            try {
                if (s == null || !s.CanRead) { continue;}
                byte[] buffer = new byte[1024];
                int length = s.Read(buffer, 0, buffer.Length);
                if (length == 0) { continue;}
                string msg = Encoding.UTF8.GetString(buffer, 0, length);
                Debug.Log($"[NetworkManager] Received: {msg}");
                // MSG HDL
                if (msg.StartsWith("NTWK")) {
                    var match = Regex.Match(msg, @"NTWK\s*\{\s*(\w+)\s*=\s*(\w+)\s*\}");
                    if (match.Success) {
                        string key = match.Groups[1].Value;
                        string val = match.Groups[2].Value;
                        if (key == "playerno" && int.TryParse(val, out int no)) { PlayerNumber = no; Debug.Log($"[NetworkManager] Assigned PlayerNumber = {PlayerNumber}"); }
                    }
                }
                else { incomingMessages.Enqueue(msg); }
            } catch (Exception e) {
                Debug.LogError("[NetworkManager] Receive Error: " + e.Message);
                break;
            }
        }
    }

    public void SendMessage(string msg) {
        if (isServer) { SendMessageServer(msg, null);}
        else { SendMessageClient(msg); }
    }
    
    public void SendMessageServer(string msg, TcpClient except) {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        foreach (var c in connectedClients) {
            if (c == null || !c.Connected || c == except) { continue; }
            try {
                NetworkStream s = c.GetStream();
                if (s.CanWrite) s.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e) { Debug.LogWarning("[NetworkManager] Send failed: " + e.Message); }
        }
    }

    public void SendMessageClient(string msg) {
        if (stream != null && stream.CanWrite) {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    public bool HasMessage() { return !incomingMessages.IsEmpty; }
    public string GetNextMessage() { return incomingMessages.TryDequeue(out string msg) ? msg : null; }
}
