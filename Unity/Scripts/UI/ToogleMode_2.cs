using UnityEngine;
using TMPro;
using System.Collections;

public class ModeSelector_2 : MonoBehaviour
{
    [Header("Panel Utama")]
    public GameObject panelPilihan;

    [Header("Panel Mode")]
    public GameObject panelGeser;
    public GameObject panelMoveFK;
    public GameObject panelMoveIK;
    public GameObject panelMoveBox;
    public GameObject panelTeachingPlayback;

    [Header("UI")]
    public TMP_Text buttonText;

    public enum Mode
    {
        STIR,
        MOVEFK,
        MOVEIK,
        MOVEBOX
    }

    private Mode currentMode;
    private bool isPanelOpen = false;

    // ===== REFERENSI =====
    public RobotMover robotMover;
    public MultiJointControl jointController;
    public InverseKinematics3DOF_2 ikController;
    public Transform ikTarget;
    public Transform endEffector;
    public Transform anchor;
    public RobotTransformInfo_2 robotInfo;
    public BoxManager boxManager;

    void Start()
    {
        panelPilihan.SetActive(false);
        SetMode(Mode.STIR);
    }

    // ===== TOGGLE DROPDOWN =====
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelPilihan.SetActive(isPanelOpen);
    }

    // ===== BUTTON MODE =====
    public void SelectSTIR()   => SetMode(Mode.STIR);
    public void SelectMOVEFK() => SetMode(Mode.MOVEFK);
    public void SelectMOVEIK() => SetMode(Mode.MOVEIK);
    public void SelectMOVEBOX() => SetMode(Mode.MOVEBOX);

    // ===== TERIMA REFERENSI DARI LISTENER =====
    public void SetReferences(
        MultiJointControl joint,
        InverseKinematics3DOF_2 ik
    )
    {
        jointController = joint;
        ikController = ik;

        Debug.Log("[MODE] Referensi berhasil diterima");
    }

    void SetIKParent(bool isIK)
    {
        if (ikTarget == null ||
            endEffector == null ||
            anchor == null)
            return;

        if (isIK)
        {
            // lepas target TANPA ubah posisi
            ikTarget.SetParent(anchor, true);

            Debug.Log("[MODE] IK Target lepas");
        }
        else
        {
            // ikut EE lagi
            ikTarget.SetParent(endEffector, false);

            ikTarget.localPosition = Vector3.zero;
            ikTarget.localRotation = Quaternion.identity;

            Debug.Log("[MODE] IK Target ikut EE");
        }
    }

    void SetMode(Mode mode)
    {
        // 🔥 kalau mode sama, jangan execute lagi
        if (currentMode == mode)
            return;

        Mode previousMode = currentMode;

        if (robotInfo != null)
        {
            robotInfo.SetMode(
                (RobotTransformInfo_2.Mode)mode
            );
        }

        // tutup dropdown
        panelPilihan.SetActive(false);
        isPanelOpen = false;

        // matikan semua panel
        panelGeser.SetActive(false);
        panelMoveFK.SetActive(false);
        panelMoveIK.SetActive(false);
        panelMoveBox.SetActive(false);
        panelTeachingPlayback.SetActive(false);

        switch (mode)
        {
            case Mode.STIR:
                panelGeser.SetActive(true);
                panelTeachingPlayback.SetActive(false);
                boxManager.DeleteAllBoxes();
                buttonText.text = "STIR";

                // 🔥 matikan IK dulu
                if (ikController != null)
                    ikController.enabled = false;

                if (jointController != null)
                {
                    jointController.SetIKMode(false);
                    jointController.SyncFromCurrentPose();
                    jointController.lockElbow = true;
                }

                SetIKParent(false);
                break;

            case Mode.MOVEFK:
                panelMoveFK.SetActive(true);
                panelTeachingPlayback.SetActive(true);
                boxManager.DeleteAllBoxes();
                buttonText.text = "MOVE FK";

                if (ikController != null)
                    ikController.enabled = false;

                if (jointController != null)
                {
                    jointController.SetIKMode(false);
                    jointController.SyncFromCurrentPose();
                    jointController.lockElbow = true;
                }

                SetIKParent(false);
                break;

            case Mode.MOVEIK:
                panelMoveIK.SetActive(true);
                panelTeachingPlayback.SetActive(true);
                boxManager.DeleteAllBoxes();
                buttonText.text = "MOVE IK";

                SetIKParent(true);

                if (jointController != null)
                {
                    jointController.SetIKMode(true);
                    jointController.lockElbow = false;
                }

                if (ikController != null)
                    ikController.enabled = true;

                break;

            case Mode.MOVEBOX:
                panelMoveBox.SetActive(true);
                panelTeachingPlayback.SetActive(false);
                boxManager.DeleteAllBoxes();
                boxManager.SpawnBoxAmbil();
                buttonText.text = "MOVE BOX";

                SetIKParent(true);

                if (jointController != null)
                {
                    jointController.SetIKMode(true);
                    jointController.lockElbow = false;
                }

                if (ikController != null)
                    ikController.enabled = true;

                break;
        }

        // 🔥 update terakhir
        currentMode = mode;
    }
}