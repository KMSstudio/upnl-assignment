using UnityEngine;

/// <summary>
/// PlayerLocation
/// 
/// Represents a compact, serializable snapshot of a player's physical and control state,
/// suitable for network transmission or state logging. Includes:
/// 
/// - loc  : world-space position (Vector3)  
/// - spin : world-space rotation (Quaternion)  
/// - stance : tuple (crouch, jump) representing physical stance  
/// - aim  : whether the player is currently aiming  
/// 
/// Used primarily in synchronization of remote player state or replay systems.
/// 
/// author: Serius (tomskang@naver.com)
/// date  : 25-06-12
/// </summary>
public class PlayerLocation {
    public Vector3 loc;
    public Quaternion spin;
    public (bool crouch, bool jump) stance;
    public bool aim;

    public static PlayerLocation FromText(string line) {
        var result = new PlayerLocation();
        try {
            int pStart = line.IndexOf("P(") + 2;
            int pEnd = line.IndexOf(")", pStart);
            string[] posParts = line.Substring(pStart, pEnd - pStart).Split(',');
            result.loc = new Vector3(
                float.Parse(posParts[0]),
                float.Parse(posParts[1]),
                float.Parse(posParts[2])
            );

            int rStart = line.IndexOf("R(") + 2;
            int rEnd = line.IndexOf(")", rStart);
            string[] rotParts = line.Substring(rStart, rEnd - rStart).Split(',');
            result.spin = new Quaternion(
                float.Parse(rotParts[0]),
                float.Parse(rotParts[1]),
                float.Parse(rotParts[2]),
                float.Parse(rotParts[3])
            );

            result.stance.crouch = line.Contains("C");
            result.stance.jump = line.Contains("J");
            result.aim = line.Contains("A");
        }
        catch { Debug.LogWarning($"Failed to parse PlayerLocation from string: {line}"); }
        return result;
    }

    public string ToText() {
        string flag = "";
        if (stance.crouch) flag += "C";
        if (stance.jump) flag += "J";
        if (aim) flag += "A";

        return $"P({loc.x:F1},{loc.y:F1},{loc.z:F1}) R({spin.x:F2},{spin.y:F2},{spin.z:F2},{spin.w:F2}) {flag}";
    }
}