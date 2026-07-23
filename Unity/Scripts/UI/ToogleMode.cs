using UnityEngine;
using TMPro;

public class ModeSelector : MonoBehaviour
{
    [Header("Panel Utama")]
    public GameObject panelPilihan; // panel dropdown (isi 3 tombol)

    [Header("Panel Mode")]
    public GameObject panelGeser;
    public GameObject panelMoveFK;
    public GameObject panelMoveFK_2;
    public GameObject panelMoveIK;

    [Header("UI")]
    public TMP_Text buttonText;

    public enum Mode
    {
        STIR,
        MOVEFK,
        MOVEFK_2,
        MOVEIK
    }

    private Mode currentMode;
    private bool isPanelOpen = false;

    public RobotTransformInfo robotInfo;

    InverseKinematics3DOF ikController;
    SimpleJointController jointController;
    Transform ikTarget;
    Transform endEffector;
    Transform anchor;

    void Start()
    {
        panelPilihan.SetActive(false);
        SetMode(Mode.STIR);
    }

    // Tombol utama ditekan
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelPilihan.SetActive(isPanelOpen);
    }

    // Dipanggil dari tombol pilihan
    public void SelectSTIR()
    {
        SetMode(Mode.STIR);
    }

    public void SelectMOVEFK()
    {
        SetMode(Mode.MOVEFK);
    }

    public void SelectMOVEFK_2()
    {
        SetMode(Mode.MOVEFK_2);
    }

    public void SelectMOVEIK()
    {
        SetMode(Mode.MOVEIK);
    }

    public void SetIKReferences(
        InverseKinematics3DOF controller,
        SimpleJointController joint,
        Transform target,
        Transform effector,
        Transform link0)
    {
        ikController = controller;
        jointController = joint;
        ikTarget = target;
        endEffector = effector;
        anchor = link0;

        // default aman
        if (ikController != null)
            ikController.enabled = false;

        Debug.Log("[MODE] Semua referensi IK sudah diterima");
    }

    void SetIKMode(bool isIK)
    {
        if (ikTarget == null || endEffector == null) return;

        if (isIK)
        {
            // 🔥 LEPAS dari end effector
            ikTarget.SetParent(anchor);
            Debug.Log("[IK] Target dilepas dari End Effector");
        }
        else
        {
            // 🔥 IKUTKAN lagi ke end effector
            ikTarget.SetParent(endEffector);
            Debug.Log("[FK] Target ikut End Effector");
        }
    }

    void SetMode(Mode mode)
    {
        currentMode = mode;

        panelPilihan.SetActive(false);
        isPanelOpen = false;

        panelGeser.SetActive(false);
        panelMoveFK.SetActive(false);
        panelMoveFK_2.SetActive(false);
        panelMoveIK.SetActive(false);

        switch (mode)
        {
            case Mode.STIR:
                panelGeser.SetActive(true);
                buttonText.text = "STIR";

                if (ikController != null)
                {
                    ikController.enabled = false;
                    SetIKMode(false);
                }

                if (jointController != null)
                {
                    jointController.isIK = false; // 🔥 NON IK
                }

                break;

            case Mode.MOVEFK:
                panelMoveFK.SetActive(true);
                buttonText.text = "MOVE FK";

                if (ikController != null)
                {
                    ikController.enabled = false;
                    SetIKMode(false);
                }

                if (jointController != null)
                {
                    if(jointController.isIK)
                    {
                        jointController.SyncFromCurrentPose();
                    }
                    jointController.isIK = false;
                }

                break;

            case Mode.MOVEFK_2:
                panelMoveFK_2.SetActive(true);
                buttonText.text = "MOVE FK_2";

                if (ikController != null)
                {
                    ikController.enabled = false;
                    SetIKMode(false);
                }

                if (jointController != null)
                {
                    jointController.isIK = false;
                }

                break;

            case Mode.MOVEIK:
                panelMoveIK.SetActive(true);
                buttonText.text = "MOVE IK";

                if (ikController != null)
                {
                    ikController.enabled = true;
                    SetIKMode(true);
                }

                if (jointController != null)
                {
                    jointController.isIK = true; // 🔥 IK AKTIF
                }

                break;
        }

        if (robotInfo != null)
        {
            robotInfo.SetMode((RobotTransformInfo.Mode)mode);
        }
    }
}