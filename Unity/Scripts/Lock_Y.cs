using UnityEngine;

public class LockGripperRotation : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 rot = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(rot.x, 0, rot.z);
    }
}