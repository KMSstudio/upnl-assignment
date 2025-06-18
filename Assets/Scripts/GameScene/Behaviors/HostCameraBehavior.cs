using UnityEngine;
using System.Collections;

public class HostCameraBehavior : CameraBehavior {
    protected override IEnumerator FindTarget() {
        while (HostParse.Instance?.GetLocalPlayer() == null)
            yield return null;
        target = HostParse.Instance.GetLocalPlayer().transform;
    }
}
