using UnityEngine;

public class JointButtonController : MonoBehaviour
{
    [Header("Referensi Controller")]
    public SimpleJointController controller;

    [Header("Besar Perubahan Sudut")]
    public float step = 5f;

    // ===== JOINT 1 =====
    public void Joint1Plus()
    {
        controller.SetAngle(0, controller.targetAngles[0] + step);
    }

    public void Joint1Minus()
    {
        controller.SetAngle(0, controller.targetAngles[0] - step);
    }

    // ===== JOINT 2 =====
    public void Joint2Plus()
    {
        controller.SetAngle(1, controller.targetAngles[1] + step);
    }

    public void Joint2Minus()
    {
        controller.SetAngle(1, controller.targetAngles[1] - step);
    }

    // ===== JOINT 3 =====
    public void Joint3Plus()
    {
        controller.SetAngle(2, controller.targetAngles[2] + step);
    }

    public void Joint3Minus()
    {
        controller.SetAngle(2, controller.targetAngles[2] - step);
    }
}