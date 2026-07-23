using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TrackedImageUIListener : MonoBehaviour
{
    ARTrackedImageManager manager;

    public UIManager uiManager;
    public RobotTransformInfo robotInfo;
    public MoveRobotController moveController;
    public JointButtonController jointUI;
    public IKButtonController ikButton;
    private GameObject ikTarget;
    public ModeSelector PilihMode;
    public TeachingPlayback waypointManager;

    public string robotPartName = "robot_base";

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
        Debug.Log("[INIT] ARTrackedImageManager siap");
    }

    void OnEnable()
    {
        manager.trackedImagesChanged += OnTrackedImagesChanged;
        Debug.Log("[EVENT] Subscribe trackedImagesChanged");
    }

    void OnDisable()
    {
        manager.trackedImagesChanged -= OnTrackedImagesChanged;
        Debug.Log("[EVENT] Unsubscribe trackedImagesChanged");
    }

    void PrintAllChildren(Transform parent, string prefix = "")
    {
        foreach (Transform child in parent)
        {
            Debug.Log(prefix + child.name);
            PrintAllChildren(child, prefix + "--");
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log("[EVENT] TrackedImagesChanged triggered");
            Debug.Log("[DETECTED] Image terdeteksi: " + trackedImage.name);

            uiManager.RobotDetected();

            Transform targetPart = null;

            foreach (Transform child in trackedImage.transform)
            {
                Debug.Log("[CHECK] Child of TrackedImage: " + child.name);

                if (child.name == robotPartName)
                {
                    targetPart = child;
                    Debug.Log("[FOUND] robot_base ditemukan");
                }
            }

            if (targetPart != null)
            {
                // ===== AMBIL ROOT ROBOT =====
                Transform robotRoot = targetPart.root;
                Debug.Log("[INFO] Root robot: " + robotRoot.name);
                Debug.Log("[DEBUG] Struktur robot:");
                PrintAllChildren(robotRoot);

                // ===== LEPAS DARI MARKER =====
                robotRoot.SetParent(null, true);

                Vector3 worldPos = robotRoot.position;
                Quaternion worldRot = robotRoot.rotation;

                // ===== BUAT ANCHOR =====
                GameObject anchor = new GameObject("RobotAnchor");
                anchor.transform.position = worldPos;
                anchor.transform.rotation = worldRot;

                // ===== PASANG KE ANCHOR =====
                robotRoot.SetParent(anchor.transform, false);
                robotRoot.localPosition = Vector3.zero;
                robotRoot.localRotation = Quaternion.identity;

                Debug.Log("[SUCCESS] Robot dipindah ke anchor");

                // ===== CARI CUBE IK TARGET =====
                Transform cubeTransform = FindChildRecursive(robotRoot, "Cube");
                Transform linkReference = FindChildRecursive(robotRoot, "dobotcp_link1_visible_vis_1");
                Transform targetEndEffector = FindChildRecursive(robotRoot, "TargetAkhir");
                Transform robotBase = FindChildRecursive(robotRoot, "dobotcp_link1_visible_vis_1");

                // ===== CARI SKRIP KONTROL ROBOT =====
                SimpleJointController controller = robotRoot.GetComponentInChildren<SimpleJointController>();

                if (cubeTransform != null)
                {
                    ikTarget = cubeTransform.gameObject;
                    ikTarget.SetActive(true);

                    Debug.Log("[FOUND] IK Target ditemukan di dalam robot: " + cubeTransform.name);
                }
                else
                {
                    Debug.LogWarning("[ERROR] Cube tidak ditemukan di dalam robot!");
                }

                if (waypointManager != null)
                {
                    Debug.Log("[LINK] Target dikirim ke WaypointManager ✅");

                    if (targetEndEffector != null && cubeTransform != null && robotBase != null && controller != null)
                    {
                        waypointManager.SetTarget(targetEndEffector, cubeTransform, robotBase, controller);

                        Debug.Log("[FOUND] TargetAkhir ditemukan: " + targetEndEffector.name);
                    }
                    else
                    {
                        Debug.LogWarning("[ERROR] TargetAkhir atau Cube tidak ditemukan!");
                    }
                }
                else
                {
                    Debug.LogWarning("[ERROR] WaypointManager belum di-assign!");
                }

                // ===== HUBUNGKAN KE SCRIPT LAIN =====

                if (robotInfo != null)
                {
                    robotInfo.SetTarget(anchor.transform, cubeTransform, linkReference);
                    Debug.Log("[LINK] RobotTransformInfo terhubung");
                    if (controller != null)
                    {
                        robotInfo.SetJointController(controller);
                    }
                    else
                    {
                        Debug.LogWarning("[ERROR] SimpleJointController tidak ditemukan di robot!");
                    }
                }

                if (moveController != null)
                {
                    // Set posisi anchor
                    anchor.transform.localPosition = new Vector3(0f, 0f, 0f);
                    moveController.robotBase = anchor.transform;
                    Debug.Log("[LINK] MoveRobotController terhubung");
                }

                if (ikTarget != null)
                {
                    // kirim ke IK Button Controller
                    if (ikButton != null)
                    {
                        ikButton.SetIKTarget(ikTarget.transform);
                        Debug.Log("[LINK] IKButtonController TERHUBUNG ✅");
                    }
                    else
                    {
                        Debug.LogWarning("[ERROR] IKButton belum di-assign!");
                    }
                }
                else
                {
                    Debug.LogWarning("[ERROR] IK Target tidak ditemukan saat tracking!");
                }

                if (jointUI != null)
                {

                    if (controller != null)
                    {
                        jointUI.controller = controller;
                        Debug.Log("[LINK] JointButtonController TERHUBUNG ✅");
                    }
                    else
                    {
                        Debug.LogWarning("[ERROR] SimpleJointController tidak ditemukan di robot!");
                    }
                }
                else
                {
                    Debug.LogWarning("[ERROR] JointUI belum di-assign di inspector!");
                }

                if (PilihMode != null)
                {
                    // ===== AMBIL IK CONTROLLER =====
                    InverseKinematics3DOF modeIK = robotRoot.GetComponentInChildren<InverseKinematics3DOF>();

                    // ===== AMBIL JOINT CONTROLLER =====
                    SimpleJointController jointController = robotRoot.GetComponentInChildren<SimpleJointController>();

                    // ===== CARI END EFFECTOR DAN LINK_0 =====
                    Transform endEffector = FindChildRecursive(robotRoot, "link_3");
                    Transform link0 = FindChildRecursive(robotRoot, "link_0");

                    if (modeIK != null && jointController != null && cubeTransform != null && endEffector != null && link0 != null)
                    {
                        // ✅ KIRIM SEMUA KE MODE SELECTOR
                        PilihMode.SetIKReferences(
                            modeIK,
                            jointController,
                            cubeTransform,
                            endEffector,
                            link0
                        );

                        Debug.Log("[LINK] IK system terhubung lengkap ✅");
                    }
                    else
                    {
                        Debug.LogWarning("[ERROR] Salah satu komponen IK tidak ditemukan!");

                        if (modeIK == null)
                            Debug.LogWarning(" - IK Controller tidak ada");

                        if (cubeTransform == null)
                            Debug.LogWarning(" - Cube (IK Target) tidak ada");

                        if (endEffector == null)
                            Debug.LogWarning(" - End Effector tidak ada");

                        if (jointController == null)
                        Debug.LogWarning(" - Joint Controller tidak ada");  
                    }
                }
                else
                {
                    Debug.LogWarning("[ERROR] PilihMode belum di-assign di inspector!");
                }

                Debug.Log("[DONE] Setup robot selesai");
            }
            else
            {
                Debug.LogWarning("[ERROR] robot_base tidak ditemukan!");
            }
        }
    }
}