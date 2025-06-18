using UnityEngine;
using System.Collections;

public abstract class CameraBehavior : MonoBehaviour {
    public Vector3 offset = new Vector3(0f, 5f, -7f);
    public float followSpeed = 10f;

    protected Transform target;
    protected abstract IEnumerator FindTarget();

    IEnumerator Start() {
        yield return StartCoroutine(FindTarget());
    }

    void LateUpdate() {
        if (target == null) return;
        // TRANSFORM
        // Vector3 desiredPos = target.position + target.TransformDirection(offset);
        // transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.position = target.position + target.TransformDirection(offset);
        // ROTATION
        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        transform.LookAt(lookTarget);
    }
}
