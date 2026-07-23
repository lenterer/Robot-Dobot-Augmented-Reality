using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI notificationText;

    public void SetText(string msg)
    {
        Debug.Log("[UIManager] " + msg);

        if (notificationText != null)
            notificationText.text = msg;
        else
            Debug.LogWarning("[UIManager] Text NULL");
    }

    public void RobotDetected()
    {
        notificationText.text = "Robot detected!";
    }

    public void Scanning()
    {
        notificationText.text = "Scanning marker...";
    }
}