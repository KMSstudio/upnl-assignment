using System.Collections.Generic;
using System.Linq;

public static class PlayerInfoList {
    public class PlayerInfo {
        public int PlayerNo { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string IP { get; set; } // OPTIONAL
        public int? Port { get; set; } // OPTIONAL
    }

    private static Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();
    public static void AddPlayer(int playerNo, string playerId, string playerName, string ip = null, int? port = null) {
        if (!players.ContainsKey(playerNo)) {
            players[playerNo] = new PlayerInfo {
                PlayerNo = playerNo,
                PlayerId = playerId,
                PlayerName = playerName,
                IP = ip,
                Port = port
            };
        }
    }

    public static PlayerInfo GetPlayer(int playerNo) { return players.TryGetValue(playerNo, out var player) ? player : null; }
    public static List<PlayerInfo> GetAllPlayers() { return players.Values.ToList(); }
    public static void RemovePlayer(int playerNo) { players.Remove(playerNo); }
    public static void Clear() { players.Clear(); }
    
    public static void UpdatePlayerName(int playerNo, string newName) {
        if (players.TryGetValue(playerNo, out var player)) { player.PlayerName = newName; } }
    public static void UpdateNetworkInfo(int playerNo, string ip, int? port) {
        if (players.TryGetValue(playerNo, out var player)) { player.IP = ip; player.Port = port; } }
}
