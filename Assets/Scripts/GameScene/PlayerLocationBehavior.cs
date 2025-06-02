using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerLocationBehavior.cs
/// Controls a player via externally received PlayerLocation updates
/// </summary>
public class PlayerLocationBehavior : PlayerBehavior {
    public MonoBehaviour locationSource;

    private Queue<PlayerLocation> locationQueue;

    protected override void Start() {
        base.Start();
        if (locationSource is ILocationController controller) locationQueue = controller.GetLocationQueue();
        else Debug.LogError("Location source must implement ILocationController.");
    }

    protected override void Update() {
        if (locationQueue == null) return;
        while (locationQueue.Count > 0) {
            var loc = locationQueue.Dequeue();
            ApplyLocation(loc);
        }
    }
}