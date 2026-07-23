using UnityEngine;

[System.Flags]
public enum RotationAxis
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4
}

public class LimitJointRotation : MonoBehaviour
{
    public RotationAxis allowedAxis = RotationAxis.Z;

    public float minAngle = -90f;
    public float maxAngle = 90f;

    Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
    }

    void LateUpdate()
    {
        Vector3 rot = transform.localEulerAngles;

        rot.x = NormalizeAngle(rot.x);
        rot.y = NormalizeAngle(rot.y);
        rot.z = NormalizeAngle(rot.z);

        if (!allowedAxis.HasFlag(RotationAxis.X))
            rot.x = initialRotation.x;

        if (!allowedAxis.HasFlag(RotationAxis.Y))
            rot.y = initialRotation.y;

        if (!allowedAxis.HasFlag(RotationAxis.Z))
            rot.z = initialRotation.z;
        else
            rot.z = Mathf.Clamp(rot.z, minAngle, maxAngle);

        transform.localEulerAngles = rot;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360;
        return angle;
    }
}