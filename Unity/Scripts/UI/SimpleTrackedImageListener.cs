using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SimpleTrackedImageListener : MonoBehaviour
{
    ARTrackedImageManager manager;
    public JointButtonController_2 jointUI;
    public RobotMover robotMover;
    public IKButtonController_2 ikUI;
    public ModeSelector_2 modeSelector;
    public RobotTransformInfo_2 robotInfo;
    public TeachingPlayback_2 teachingPlayback;
    public ScaleButtonController uiScaleButtonController;
    public BoxManager boxManager;
    public PickAndPlace pickandplace;
    public RobotHomeButton homebutton;
    public TextMeshProUGUI statusText;

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    
    void Awake()
    {
        manager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        manager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        manager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log("Marker terdeteksi: " + trackedImage.name);

            Transform robot = trackedImage.transform.GetChild(0);

            StartCoroutine(FixPosition(robot, trackedImage.transform));

            // 👉 kirim ke RobotMover
            robotMover.SetRobot(robot);

            Debug.Log("Robot dikirim ke RobotMover: " + robot.name);

            // 🔥 cari script joint control di robot
            MultiJointControl jointControl = robot.GetComponentInParent<MultiJointControl>();
            // 🔥 sync ke posisi joint sekarang
            jointControl.SyncFromCurrentPose();

            // matikan IK default
            jointControl.SetIKMode(false);

            if (jointControl != null)
            {
                jointUI.SetController(jointControl);
                Debug.Log("JointControl dikirim ke UI");
            }
            else
            {
                Debug.LogError("MultiJointControl tidak ditemukan di robot!");
            }

            // cari IK TARGET (misal nama: "TargetIK" atau "Cube")
            Transform ikTarget = FindChildRecursive(robot, "TargetIK");

            if (ikTarget != null)
            {
                ikUI.SetTarget(ikTarget);
                ikUI.jointControl = jointControl;
                Debug.Log("IK Target dikirim ke UI: " + ikTarget.name);
            }
            else
            {
                Debug.LogError("IK Target tidak ditemukan!");
            }
            
            // Mode Selector
            MultiJointControl joint = robot.GetComponentInParent<MultiJointControl>();
            InverseKinematics3DOF_2 ik = robot.GetComponentInParent<InverseKinematics3DOF_2>();
            Transform endEffector = FindChildRecursive(robot, "link_5");
            Transform anchor = robot;

            // kirim ke mode selector
            if (ikTarget != null && endEffector != null)
            {
                modeSelector.ikTarget = ikTarget;
                modeSelector.endEffector = endEffector;
                modeSelector.anchor = anchor;

                Debug.Log("IK hierarchy reference dikirim ke ModeSelector");
            }
            else
            {
                Debug.LogError("IK Target / EndEffector tidak ditemukan!");
            }

            if (joint != null && ik != null)
            {
                modeSelector.SetReferences(joint, ik);
                Debug.Log("ModeSelector terhubung");
            }
            else
            {
                Debug.LogError("ModeSelector tidak ditemukan!");
            }


            // kirim ke robot info
            Transform BaseRefrence = FindChildRecursive(robot, "dobotcp_link1_visible_vis_2");
            if (robotInfo != null)
            {
                robotInfo.SetTarget(robot, ikTarget, BaseRefrence);
                robotInfo.SetJointController(jointControl);

                Debug.Log("RobotInfo terhubung");
            }

            // TEACHING PLAYBACK
            Transform TargetEndEffector = FindChildRecursive(robot, "TargetEndEffector");
            if (teachingPlayback != null)
            {
                // target visual (marker waypoint)
                Transform target1 = TargetEndEffector;

                // target robot (IK target yg digerakkan)
                Transform target2 = ikTarget;

                // base reference robot
                Transform baseRef = robot;

                teachingPlayback.SetTarget(
                    target1,
                    target2,
                    baseRef,
                    jointControl
                );

                Debug.Log("TeachingPlayback terhubung");
            }

            RobotScaleController robotScaler = robot.GetComponentInParent<RobotScaleController>();

            if (robotScaler != null)
            {
                // Hubungkan komponen robot ke skrip tombol UI Scale
                uiScaleButtonController.SetScaler(robotScaler);
            }
            else
            {
                Debug.LogError("[LISTENER] RobotScaleController tidak ditemukan pada robot yang di-spawn!");
            }

            if (boxManager != null)
            {
                if (robot != null)
                {
                    boxManager.boxContainer = robot;
                    Debug.Log("[BOX] boxContainer berhasil diset ke parent robot: " + robot.name);
                }
                else
                {
                    Debug.LogError("[BOX] Gagal set boxContainer! Objek robot tidak memiliki parent.");
                }
            }

            if (pickandplace != null)
            {
                if (robot != null)
                {
                    pickandplace.robotBase = robot;
                    pickandplace.jointController = joint;
                    pickandplace.TargetEndEffector = TargetEndEffector;
                    Debug.Log("[PickAndPlace] berhasil.");
                }
                else
                {
                    Debug.LogError("[PickAndPlace] Gagal.");
                }
            }

            if (homebutton != null)
            {
                homebutton.jointController = jointControl;
            }
        }

        bool isAnyMarkerTracked = false;

        foreach (var trackedImage in manager.trackables)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                isAnyMarkerTracked = true;
                
                break; // Keluar dari loop jika sudah ada minimal satu marker yang aktif
            }
        }

        // Perbarui teks UI berdasarkan hasil pengecekan status tracking
        if (isAnyMarkerTracked)
        {
            statusText.text = "Marker Terdeteksi";
        }
        else
        {
            statusText.text = "Marker Tidak Terdeteksi";
        }
    }

    IEnumerator FixPosition(Transform robot, Transform marker)
    {
        yield return new WaitForSeconds(0.05f);

        ArticulationBody root = robot.GetComponent<ArticulationBody>();

        if (root != null)
        {
            Vector3 pos = marker.position;
            Quaternion rot = Quaternion.Euler(0, marker.eulerAngles.y, 0);

            root.TeleportRoot(pos, rot);

            Debug.Log("Robot dikembalikan ke marker");
        }
    }
}