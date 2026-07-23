using TMPro;
using UnityEngine;

public class RobotTransformInfo : MonoBehaviour
{
    public enum Mode
    {
        STIR,
        MOVEFK,
        MOVEFK_2,
        MOVEIK
    }

    private Mode currentMode;
    public TextMeshProUGUI infoText;
    public Transform targetPart;
    public Transform ikTarget;
    public Transform linkReference;
    public SimpleJointController jointController;

    public void SetMode(Mode mode)
    {
        currentMode = mode;
    }

    void Update()
    {
        if (targetPart == null || infoText == null) return;

        Vector3 pos = targetPart.localPosition;
        Vector3 rot = targetPart.localEulerAngles;
        Vector3 pos_ik = linkReference.InverseTransformPoint(ikTarget.position);
        float j1 = jointController.targetAngles[0] - 45;
        float j2 = jointController.targetAngles[1];
        float j3 = jointController.elbowGlobalLock;

        switch (currentMode)
        {
            case Mode.STIR:
                infoText.text =
                    "STIR MODE\n" +
                    "X: " + pos.x.ToString("F2") + " | " + rot.x.ToString("F1") + "\n" +
                    "Y: " + pos.y.ToString("F2") + " | " + rot.y.ToString("F1") + "\n" +
                    "Z: " + pos.z.ToString("F2") + " | " + rot.z.ToString("F1");
                break;

            case Mode.MOVEFK:
                infoText.text =
                    "MOVE FK\n" +
                    "X: " + pos_ik.x.ToString("F2") + " | J1: " + j1.ToString("F1") + "\n" +
                    "Y: " + pos_ik.y.ToString("F2") + " | J2: " + j2.ToString("F1") + "\n" +
                    "Z: " + pos_ik.z.ToString("F2") + " | J3: " + j3.ToString("F1");
                break;

            case Mode.MOVEFK_2:
                infoText.text =
                    "MOVE FK_2\n" +
                    "X: " + pos_ik.x.ToString("F2") + " | J1: " + j1.ToString("F1") + "\n" +
                    "Y: " + pos_ik.y.ToString("F2") + " | J2: " + j2.ToString("F1") + "\n" +
                    "Z: " + pos_ik.z.ToString("F2") + " | J3: " + j3.ToString("F1");
                break;

            case Mode.MOVEIK:
                infoText.text =
                    "MOVE IK\n" +
                    "X: " + pos_ik.x.ToString("F2") + " | J1: " + j1.ToString("F1") + "\n" +
                    "Y: " + pos_ik.y.ToString("F2") + " | J2: " + j2.ToString("F1") + "\n" +
                    "Z: " + pos_ik.z.ToString("F2") + " | J3: " + j3.ToString("F1");
                break;
        }
    }

    // Fungsi untuk dipanggil oleh TrackedImageUIListener
    public void SetTarget(Transform robotPart, Transform ikPart, Transform refrence)
    {
        targetPart = robotPart;
        ikTarget = ikPart;
        linkReference = refrence;
    }

    // Fungsi menghubungkan ke joint controller
    public void SetJointController(SimpleJointController controller)
    {
        jointController = controller;
    }
}