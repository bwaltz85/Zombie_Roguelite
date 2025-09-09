using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;     // player
    public Vector3 offset = new Vector3(0, 15, -10);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(target); // or comment this out if you want fixed isometric angle
    }
}
