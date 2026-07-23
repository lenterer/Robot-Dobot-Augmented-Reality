using UnityEngine;

public class MultiJointControl : MonoBehaviour
{
    [Header("Joint Articulation")]
    public ArticulationBody joint1; // base
    public ArticulationBody joint2; // shoulder
    public ArticulationBody joint3; // elbow

    [Header("Target Angle")]
    [Range(-80f, 80f)] public float target1 = 0f;
    [Range(0f, 55f)] public float target2 = 0f;
    [Range(-180f, 180f)] public float target3 = 0f;

    [Header("Real Robot Angle (Output)")]
    public float realTarget1;
    public float realTarget2;
    public float realTarget3;
    public float realTarget3FK;

    [Header("Mode")]
    public bool isIK = false;

    [Header("Elbow Lock (FK only)")]
    public bool lockElbow = true;

    private float elbowGlobalLock;
    private bool isInitialized = false;

    float lastElbowInputTime = 0f;
    public float inputDelay = 0.3f;

    void Start()
    {
        InitializeElbowLock();
    }

    // =========================
    // INIT LOCK
    // =========================
    void InitializeElbowLock()
    {
        if (joint2 == null || joint3 == null) return;

        float shoulder = joint2.jointPosition[0] * Mathf.Rad2Deg;
        float elbowLocal = joint3.jointPosition[0] * Mathf.Rad2Deg;

        // simpan sudut global elbow
        elbowGlobalLock = shoulder + elbowLocal;

        isInitialized = true;
    }

    public void UpdateRealTargetsExternal()
    {
        UpdateRealTargets();
    }

    // =========================
    // MAIN UPDATE
    // =========================
    void Update()
    {
        SetJointTarget(joint1, target1);
        SetJointTarget(joint2, target2);

        float elbowTarget = target3;

        if (!isIK)
        {
            float timeSinceInput = Time.time - lastElbowInputTime;
            bool isUserControlling = timeSinceInput <= inputDelay;

            if (isUserControlling)
            {
                // USER GERAK → bebas
                elbowTarget = target3;

                lockElbow = false;
            }
            else
            {
                // SAAT BARU SELESAI INPUT → update lock dulu
                if (!lockElbow)
                {
                    InitializeElbowLock();
                    lockElbow = true;

                    Debug.Log("[LOCK] Update dari posisi terakhir");
                }

                // BARU lock digunakan
                float shoulder = joint2.jointPosition[0] * Mathf.Rad2Deg;
                elbowTarget = elbowGlobalLock - shoulder;
                target3 = elbowTarget;
            }
        }
        else
        {
            // IK bebas
            elbowTarget = target3;
        }

        SetJointTarget(joint3, elbowTarget);

        UpdateRealTargets();
    }

    // =========================
    // APPLY KE ARTICULATION
    // =========================
    void SetJointTarget(ArticulationBody joint, float target)
    {
        if (joint == null) return;

        var drive = joint.xDrive;
        drive.target = target;
        joint.xDrive = drive;
    }

    // =========================
    // DIPAKAI IK & UI
    // =========================
    public void SetTarget(int index, float value)
    {
        switch (index)
        {
            case 0:
                target1 = value;
                break;

            case 1:
                target2 = value;
                break;

            case 2:
                target3 = value;

                // 🔥 kalau manual gerak elbow saat FK
                if (!isIK)
                {
                    lastElbowInputTime = Time.time;
                    lockElbow = false;
                }
                break;
        }
    }

    // =========================
    // MODE SWITCH (DARI UI)
    // =========================
    public void SetIKMode(bool value)
    {
        isIK = value;

        if (!isIK)
        {
            // balik ke FK → reset lock
            InitializeElbowLock();
        }
    }

    // =========================
    // SYNC FK DARI POSISI SEKARANG
    // =========================
    public void SyncFromCurrentPose()
    {
        if (joint1 != null)
            target1 = Mathf.Round(joint1.jointPosition[0] * Mathf.Rad2Deg);

        if (joint2 != null)
            target2 = Mathf.Round(joint2.jointPosition[0] * Mathf.Rad2Deg);

        if (joint3 != null)
            target3 = Mathf.Round(joint3.jointPosition[0] * Mathf.Rad2Deg);

        InitializeElbowLock();

        Debug.Log("[SYNC] FK dari posisi sekarang");
    }

    void UpdateRealTargets()
    {
        // sesuaikan dengan mapping robot asli kamu

        realTarget1 = -target1 - 35f;
        realTarget2 = target2 + 5f;
        realTarget3 = isIK ? (target2 + target3) : elbowGlobalLock;
        realTarget3FK = target2 + target3;

        // rounding biar rapi
        realTarget1 = Mathf.Round(realTarget1 * 100f) / 100f;
        realTarget2 = Mathf.Round(realTarget2 * 100f) / 100f;
        realTarget3 = Mathf.Round(realTarget3 * 100f) / 100f;
        realTarget3FK = Mathf.Round(realTarget3FK * 100f) / 100f;
    }
}