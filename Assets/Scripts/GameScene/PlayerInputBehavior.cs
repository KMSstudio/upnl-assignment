using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a player via external input source
/// </summary>
public class PlayerInputBehavior : PlayerBehavior {
    public MonoBehaviour inputSource;

    private Queue<PlayerInput> inputQueue;

    protected override void Start() {
        base.Start();
        if (inputSource is IInputProvider provider) inputQueue = provider.GetInputQueue();
        else Debug.LogError("Input source must implement IInputProvider.");
    }

    protected override void Update() {
        if (inputQueue == null) return;
        while (inputQueue.Count > 0) {
            var input = inputQueue.Dequeue();
            ApplyInput(input);
        }
    }
}