using UnityEngine;

// Scripts/GameScene/types/PlayerLocation.cs

/// <summary>
/// Represents parsed player state and position, compatible with PlayerMove structure
/// </summary>
public class PlayerLocation {
    public Vector3 position;
    public int motion;
    public (bool crouch, bool jump) stance;
    public bool aim;
    public bool fire;

    public static PlayerLocation FromString(string line) {
        var result = new PlayerLocation();

        int posStart = line.IndexOf("POS(") + 4;
        int posEnd = line.IndexOf(")", posStart);
        string[] posParts = line.Substring(posStart, posEnd - posStart).Split(',');
        result.position = new Vector3(
            float.Parse(posParts[0]),
            float.Parse(posParts[1]),
            float.Parse(posParts[2])
        );

        int stateStart = line.IndexOf("STATE(") + 6;
        int stateEnd = line.LastIndexOf(")");
        string[] stateParts = line.Substring(stateStart, stateEnd - stateStart).Split(' ');

        result.motion = stateParts[0] switch {
            "LeanLeft" => -1,
            "LeanRight" => 1,
            _ => 0
        };

        for (int i = 1; i < stateParts.Length; i++) {
            switch (stateParts[i]) {
                case "Crouch": result.stance.crouch = true; break;
                case "Jump": result.stance.jump = true; break;
                case "Aiming": result.aim = true; break;
                case "Firing": result.fire = true; break;
            }
        }

        return result;
    }

    public override string ToString() {
        string motionStr = motion switch {
            -1 => "LeanLeft",
            1 => "LeanRight",
            _ => "Neutral"
        };
        string stanceStr = "";
        if (stance.crouch) stanceStr += "Crouch ";
        if (stance.jump) stanceStr += "Jump ";
        string aimStr = aim ? "Aiming " : "";
        string fireStr = fire ? "Firing" : "";
        string stateStr = $"{motionStr} {stanceStr}{aimStr}{fireStr}".Trim();
        return $"POS({position.x:F2},{position.y:F2},{position.z:F2}) STATE({stateStr})";
    }
}
