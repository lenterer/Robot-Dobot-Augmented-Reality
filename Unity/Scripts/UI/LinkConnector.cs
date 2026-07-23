using UnityEngine;

public class LinkConnector : MonoBehaviour
{
    public Transform pointA; // pangkal (link 4)
    public Transform pointB; // ujung (link 5)

    void LateUpdate()
    {
        if (pointA == null || pointB == null) return;

        Vector3 dir = pointB.position - pointA.position;

        // posisi di tengah
        transform.position = (pointA.position + pointB.position) / 2f;

        // arah
        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(-90, 0, 0);
    }
}