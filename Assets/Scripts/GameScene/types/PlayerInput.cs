using UnityEngine;
using System;

/// <summary>
/// Represents structured player input state at a single frame.
/// Includes movement direction, motion axis, stance state, aiming and firing status.
/// 
/// Author: Serius <tomskang@naver.com>
/// Last Modified: 2025-05-28
/// </summary>
public class PlayerInput {
    public Vector2 move;
    public int motion;
    public (bool crouch, bool jump) stance;
    public bool aim;
    public bool fire;

    public PlayerInput(Vector2 move, int motion, (bool, bool) stance, bool aim, bool fire) {
        this.move = move;
        this.motion = motion;
        this.stance = stance;
        this.aim = aim;
        this.fire = fire;
    }

    public PlayerInput(Vector2 move, int motion)
        : this(move, motion, (false, false), false, false) { }

    public PlayerInput(Vector2 move)
        : this(move, 0, (false, false), false, false) { }

    public PlayerInput()
        : this(Vector2.zero, 0, (false, false), false, false) { }

    public override string ToString() {
        return $"{move.x},{move.y},{motion},{stance.crouch},{stance.jump},{aim},{fire}";
    }

    public static PlayerInput FromString(string str) {
        var tokens = str.Split(',');
        if (tokens.Length != 7) throw new FormatException("Invalid input string");

        float x = float.Parse(tokens[0]);
        float y = float.Parse(tokens[1]);
        int motion = int.Parse(tokens[2]);
        bool crouch = bool.Parse(tokens[3]);
        bool jump = bool.Parse(tokens[4]);
        bool aim = bool.Parse(tokens[5]);
        bool fire = bool.Parse(tokens[6]);

        return new PlayerInput(new Vector2(x, y), motion, (crouch, jump), aim, fire);
    }
}