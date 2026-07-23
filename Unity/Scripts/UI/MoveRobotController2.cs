using UnityEngine;

public class MoveRobotController2 : MonoBehaviour
{
    public Transform robotBase;
    public float moveStep = 0.02f;
    public float rotateStep = 5f;

    public void MoveForward()
    {
        if (robotBase == null) return;
        robotBase.localPosition += new Vector3(0, 0, moveStep);
    }

    public void MoveBackward()
    {
        if (robotBase == null) return;
        robotBase.localPosition += new Vector3(0, 0, -moveStep);
    }

    public void MoveLeft()
    {
        if (robotBase == null) return;
        robotBase.localPosition += new Vector3(-moveStep, 0, 0);
    }

    public void MoveRight()
    {
        if (robotBase == null) return;
        robotBase.localPosition += new Vector3(moveStep, 0, 0);
    }

    public void MoveUp()
    {
        if (robotBase == null) return;
        robotBase.localPosition += new Vector3(0, moveStep, 0);
    }

    public void MoveDown()
    {
        if (robotBase == null) return;
        robotBase.localPosition += new Vector3(0, -moveStep, 0);
    }

    public void RotateLeft()
    {
        if (robotBase == null) return;
        robotBase.Rotate(0, -rotateStep, 0);
    }

    public void RotateRight()
    {
        if (robotBase == null) return;
        robotBase.Rotate(0, rotateStep, 0);
    }
}