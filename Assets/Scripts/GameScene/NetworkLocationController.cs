using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles external string-based PlayerLocation updates and queues them
/// </summary>
public class NetworkLocationController : MonoBehaviour, ILocationController {
    private Queue<PlayerLocation> locationQueue = new Queue<PlayerLocation>();

    public Queue<PlayerLocation> GetLocationQueue() { return locationQueue; }

    public void Receive(string line) {
        var loc = PlayerLocation.FromString(line);
        locationQueue.Enqueue(loc);
    }
}