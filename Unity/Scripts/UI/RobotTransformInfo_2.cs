using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RobotTransformInfo_2 : MonoBehaviour
{
    public enum Mode
    {
        STIR,
        MOVEFK,
        MOVEIK
    }

    private Mode currentMode;

    [Header("UI")]
    public TextMeshProUGUI infoText;
    public Image statusCircle;
    public TMP_Text statusText;

    [Header("Color")]
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;

    [Header("Transform")]
    public Transform robotRoot;
    public Transform ikTarget;
    public Transform reference;
    public TeachingPlayback_2 playbackSystem;

    [Header("Controller")]
    public MultiJointControl jointController;

    void Start()
    {
        SetDisconnected();
    }
    
    public void SetMode(Mode mode)
    {
        currentMode = mode;
    }

    void Update()
    {
        if (robotRoot == null || infoText == null) return;

        Vector3 pos = robotRoot.position;
        Vector3 rot = robotRoot.eulerAngles;

        Vector3 pos_ik = Vector3.zero;
        if (ikTarget != null && reference != null)
        {
            pos_ik = reference.InverseTransformPoint(ikTarget.position);
        }

        float j1 = jointController != null ? jointController.realTarget1 : 0f;
        float j2 = jointController != null ? jointController.realTarget2 : 0f;
        float j3 = jointController != null ? jointController.realTarget3 : 0f;
        float j4 = playbackSystem.joint4;

        switch (currentMode)
        {
            case Mode.STIR:
                infoText.text =
                    "STIR\n" +
                    "X: " + pos.x.ToString("F2") + " | " + rot.x.ToString("F1") + "\n" +
                    "Y: " + pos.y.ToString("F2") + " | " + rot.y.ToString("F1") + "\n" +
                    "Z: " + pos.z.ToString("F2") + " | " + rot.z.ToString("F1");
                break;

            case Mode.MOVEFK:
                infoText.text =
                    "MOVE FK\n" +
                    "X: " + (pos_ik.x* 100f).ToString("F1") + " | J1: " + j1.ToString("F1") + "\n" +
                    "Y: " + (pos_ik.y* 100f).ToString("F1") + " | J2: " + j2.ToString("F1") + "\n" +
                    "Z: " + (pos_ik.z* 100f).ToString("F1") + " | J3: " + j3.ToString("F1") + "\n" +
                    "J4: " + j4.ToString("F1");
                break;

            case Mode.MOVEIK:
                infoText.text =
                    "MOVE IK\n" +
                    "X: " + (pos_ik.x* 100f).ToString("F1") + " | J1: " + j1.ToString("F1") + "\n" +
                    "Y: " + (pos_ik.y* 100f).ToString("F1") + " | J2: " + j2.ToString("F1") + "\n" +
                    "Z: " + (pos_ik.z* 100f).ToString("F1") + " | J3: " + j3.ToString("F1") + "\n" +
                    "J4: " + j4.ToString("F1");
                break;
        }
    }

    public void SetTarget(Transform robot, Transform ik, Transform refBase)
    {
        robotRoot = robot;
        ikTarget = ik;
        reference = refBase;
    }

    public void SetJointController(MultiJointControl controller)
    {
        jointController = controller;
    }

    public void SetConnected()
    {
        if (statusCircle != null)
            statusCircle.color = connectedColor;

        if (statusText != null)
            statusText.text = "Connected";
    }

    public void SetDisconnected()
    {
        if (statusCircle != null)
            statusCircle.color = disconnectedColor;

        if (statusText != null)
            statusText.text = "Disconnected";
    }
}