using UnityEngine;
using System.Collections;

public class ClientCameraBehavior : CameraBehavior {
    protected override IEnumerator FindTarget() {
        while (ClientParse.Instance?.GetLocalPlayer() == null)
            yield return null;

        target = ClientParse.Instance.GetLocalPlayer().transform;
    }
}
