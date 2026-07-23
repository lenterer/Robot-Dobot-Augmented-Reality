using UnityEngine;

public class JointRealRobot : MonoBehaviour
{
    [Header("Step Perubahan")]
    public float step = 5f;
    public WebSocketClient websocket;

    void SendDelta(float dj1, float dj2, float dj3)
    {
        Debug.Log($"Delta: {dj1}, {dj2}, {dj3}");

        if (websocket != null)
        {
            websocket.SendDelta(dj1, dj2, dj3);
        }
    }

    // ===== JOINT 1 =====
    public void Joint1Plus()
    {
        SendDelta(step, 0, 0);
    }

    public void Joint1Minus()
    {
        SendDelta(-step, 0, 0);
    }

    // ===== JOINT 2 =====
    public void Joint2Plus()
    {
        SendDelta(0, step, 0);
    }

    public void Joint2Minus()
    {
        SendDelta(0, -step, 0);
    }

    // ===== JOINT 3 =====
    public void Joint3Plus()
    {
        SendDelta(0, 0, step);
    }

    public void Joint3Minus()
    {
        SendDelta(0, 0, -step);
    }
}