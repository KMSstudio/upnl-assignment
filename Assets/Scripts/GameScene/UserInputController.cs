using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collects user input each frame and queues it as structured PlayerInput.
/// 
/// Author: Serius <tomskang@naver.com>
/// Last Modified: 2025-05-28
/// </summary>
public class UserInputController : MonoBehaviour, IInputProvider
{
    private Queue<PlayerInput> inputQueue = new Queue<PlayerInput>();
    private float repeatInterval = 0.02f;
    private float lastInputTime = 0;

    public Queue<PlayerInput> GetInputQueue() { return inputQueue; }

    void Update() {
        if (Time.time - lastInputTime < repeatInterval) return;
        lastInputTime = Time.time;

        Vector2 move = Vector2.zero;
        int motion = 0;
        bool crouch = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetKey(KeyCode.Space);
        bool aim = Input.GetMouseButton(1);
        bool fire = Input.GetMouseButton(0);

        if (Input.GetKey(KeyCode.W)) move.x += 1;
        if (Input.GetKey(KeyCode.S)) move.x -= 1;
        if (Input.GetKey(KeyCode.D)) move.y += 1;
        if (Input.GetKey(KeyCode.A)) move.y -= 1;

        if (Input.GetKey(KeyCode.Q)) motion += -1;
        else if (Input.GetKey(KeyCode.E)) motion += +1;

        var input = new PlayerInput(move, motion, (crouch, jump), aim, fire);
        inputQueue.Enqueue(input);
    }
}