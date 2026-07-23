using UnityEngine;

public class Link2RotationLock : MonoBehaviour
{
    public float fixedY = -90f;
    public float fixedZ = -90f;

    public float minX = -90f;
    public float maxX = 90f;

    void LateUpdate()
    {
        Vector3 rot = transform.localEulerAngles;

        float x = rot.x;

        if (x > 180) x -= 360;

        x = Mathf.Clamp(x, minX, maxX);

        transform.localEulerAngles = new Vector3(x, fixedY, fixedZ);
    }
}