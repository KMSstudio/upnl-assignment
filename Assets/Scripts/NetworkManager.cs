using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance;
    public bool verbose;

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    private static readonly ConcurrentQueue<string> incomingMessages = new ConcurrentQueue<string>();
    private readonly List<TcpClient> connectedClients = new List<TcpClient>();

    public int PlayerNumber { get; private set; } = -1;
    public string PlayerIdentifier { get; private set; } = "";
    public string PlayerName { get; private set; } = "";
    private bool doesSendMyName = false;
    public void SetName(string name) => PlayerName = name;

    private int playerCount = 1;
    public int TotalPlayers => playerCount;
    private bool isServer = false;
    public bool IsServer() => isServer;

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void StartServer(int port) {
        isServer = true; PlayerNumber = 0; playerCount = 1;
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        server.BeginAcceptTcpClient(OnClientConnected, null);
        if (string.IsNullOrEmpty(PlayerName)) { PlayerName = $"Player {PlayerNumber}"; }
        PlayerInfoList.AddPlayer(PlayerNumber, HashPlayerId(PlayerNumber), PlayerName);
        Log("Server Standby...");
    }

    public void ConnectToServer(string ip, int port) {
        client = new TcpClient();
        client.Connect(ip, port);
        stream = client.GetStream();
        receiveThread = new Thread(() => ReceiveLoop(client));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Log("Server Connected...");
    }

    private void OnClientConnected(IAsyncResult ar) {
        TcpClient newClient = server.EndAcceptTcpClient(ar);
        connectedClients.Add(newClient);
        NetworkStream newStream = newClient.GetStream();
        int assignedPlayerNo = playerCount++;
        string playerId = HashPlayerId(assignedPlayerNo);
        string playerName = $"Player {assignedPlayerNo}";
        PlayerInfoList.AddPlayer(assignedPlayerNo, playerId, playerName);
        Log($"New client connected. Assigned: playerno={assignedPlayerNo}, playerid={playerId}");
        SendRaw(newStream, $"NTWK {{playerno={assignedPlayerNo}}}");
        SendRaw(newStream, $"NTWK {{playerid={playerId}}}");
        SendMessageServer($"NTWK {{playercnt={playerCount}}}", null);
        Thread thread = new Thread(() => ReceiveLoop(newClient));
        thread.IsBackground = true;
        thread.Start();
        server.BeginAcceptTcpClient(OnClientConnected, null);
    }

    private string HashPlayerId(int playerNo) {
        string salt = "I hate typescript";
        byte[] inputBytes = Encoding.UTF8.GetBytes($"{playerNo}:{salt}");
        using (SHA256 sha = SHA256.Create()) {
            byte[] hash = sha.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    private void ReceiveLoop(TcpClient targetClient) {
        NetworkStream s = targetClient.GetStream();
        byte[] buffer = new byte[1024];
        while (true) {
            try {
                if (s == null || !s.CanRead) continue;
                int length = s.Read(buffer, 0, buffer.Length);
                if (length == 0) continue;
                string raw = Encoding.UTF8.GetString(buffer, 0, length);
                string[] msgs = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string msg in msgs) {
                    Log($"Received: {msg}");
                    if (msg.StartsWith("NTWK")) { ParseNetworkMessage(msg); }
                    else { incomingMessages.Enqueue(msg); }
                }
            }
            catch (Exception e) { LogError("Receive Error: " + e.Message); break; }
        }
    }
    
    private void ParseNetworkMessage(string msg) {
        // HANDLE PLAYER NAME LIST
            // WE USE TEMPORARY ID FOR GENERAL PLAYERINFOLIST. CLIENT SHOULD NOT KNOW OTHER PLAYERS ID.
            // THIS LOGIC SHOULD BE IMPROVED
        var matches = Regex.Matches(msg, @"(\d+)\s*\{\s*playername\s*=\s*([^\}]+)\s*\}");
        if (matches.Count > 0) {
            Debug.Log("MATCHMATCHMATCHMATCH");
            foreach (Match m in matches) {
                int pno = int.Parse(m.Groups[1].Value);
                string pname = m.Groups[2].Value;
                string pid = $"temp_{pno}";
                var existing = PlayerInfoList.GetPlayer(pno);
                if (existing != null) PlayerInfoList.UpdatePlayerName(pno, pname);
                else PlayerInfoList.AddPlayer(pno, pid, pname);
            }
            if (isServer) {
                Debug.Log("CALLCALLCALLCALLCALL");
                var builder = new StringBuilder("NTWK");
                foreach (var p in PlayerInfoList.GetAllPlayers()) {
                    builder.Append($" {p.PlayerNo}{{playername={p.PlayerName}}}"); }
                SendMessageServer(builder.ToString(), null);
            }
        }
        // HANDLE PLAYER NO, PLAYER ID, PLAYER CNT
        var match = Regex.Match(msg, @"NTWK\s*\{\s*(\w+)\s*=\s*(\w+)\s*\}");
        if (match.Success) {
            string key = match.Groups[1].Value;
            string val = match.Groups[2].Value;
            if (key == "playerno" && int.TryParse(val, out int no)) { PlayerNumber = no; }
            else if (key == "playerid") { PlayerIdentifier = val; }
            else if (key == "playercnt" && int.TryParse(val, out int cnt)) { playerCount = cnt; }
            ///
            if (!doesSendMyName && PlayerNumber >= 0 && !string.IsNullOrEmpty(PlayerIdentifier)) {
                if (string.IsNullOrEmpty(PlayerName)) { PlayerName = $"Player {PlayerNumber}"; }
                SendMessageClient($"NTWK {PlayerNumber}{{playername={PlayerName}}}"); doesSendMyName = true; }
        }
    }

    private void SendRaw(NetworkStream stream, string msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg + "\n");
        if (stream != null && stream.CanWrite)
            stream.Write(buffer, 0, buffer.Length);
    }

    public void SendChatMessage(string msg) {
        if (isServer) { SendMessageServer(msg, null); }
        else { SendMessageClient(msg); }
    }

    public void SendMessageServer(string msg, TcpClient except) {
        byte[] buffer = Encoding.UTF8.GetBytes(msg + "\n");
        Log($"[SendMessageServer] Client count: {connectedClients.Count}");
        foreach (var c in connectedClients) {
            Log($"client valid: {c != null}, connected: {c?.Connected}");
            if (c == null || !c.Connected || c == except) continue;
            try {
                NetworkStream s = c.GetStream();
                if (s.CanWrite) s.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e) { LogWarning("Send failed: " + e.Message); }
        }
    }

    public void SendMessageClient(string msg) {
        if (stream != null && stream.CanWrite) {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    public bool HasMessage() => !incomingMessages.IsEmpty;
    public string GetNextMessage() => incomingMessages.TryDequeue(out string msg) ? msg : null;

    // LOGGING HELPERS
    private void Log(string msg) {
        if (verbose) Debug.Log("[NetworkManager] " + msg);
    }
    private void LogWarning(string msg) {
        if (verbose) Debug.LogWarning("[NetworkManager] " + msg);
    }
    private void LogError(string msg) {
        if (verbose) Debug.LogError("[NetworkManager] " + msg);
    }
}
