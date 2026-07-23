using TMPro;
using UnityEngine;

public class RobotButtonTester : MonoBehaviour
{
    public TextMeshProUGUI commandText;

    public void MoveForward()
    {
        commandText.text = "Forward";
    }

    public void MoveBackward()
    {
        commandText.text = "Backward";
    }

    public void MoveLeft()
    {
        commandText.text = "Left";
    }

    public void MoveRight()
    {
        commandText.text = "Right";
    }

    public void RotateRight()
    {
        commandText.text = "Rotate 1";
    }

    public void RotateLeft()
    {
        commandText.text = "Rotate 2";
    }
}