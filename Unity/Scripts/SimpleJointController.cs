using UnityEngine;

public class SimpleJointController : MonoBehaviour
{
    [Header("1. Tarik Bagian Robot ke Sini")]
    public ArticulationBody[] joints; // Tempat menaruh Link robot

    [Header("2. Setting Kekuatan (Otomatis)")]
    public float stiffness = 200000f; // Kekakuan (agar tidak lemas)
    public float damping = 10000f;    // Peredam getaran
    public float forceLimit = 100000f;// Tenaga motor

    [Header("3. PENGENDALI (Geser Ini)")]
    [Range(-180f, 180f)] 
    public float[] targetAngles = new float[3]; // Slider ini yang nanti digeser-geser

    [Header("4. DEBUG GLOBAL ANGLE (REAL WORLD)")]
    public Vector3[] globalEulerAngles;

    [Header("5. ELBOW GLOBAL LOCK")]
    public bool lockElbow = true;

    [Header("6. LIMIT JOINT 1")]
    public float j1Min = -80f;
    public float j1Max = 90f;

    private bool isInitialized = false;
    private float lastElbowInputTime = 0f;
    public float inputDelay = 0.2f;
    [HideInInspector]
    public bool isIK = false;
    [HideInInspector]
    public float elbowGlobalLock;

    void Start()
    {
        InitializeElbowLock();
    }

    void InitializeElbowLock()
    {
        if (joints.Length < 3) return;

        float shoulder = joints[1].jointPosition[0] * Mathf.Rad2Deg;
        float elbowLocal = joints[2].jointPosition[0] * Mathf.Rad2Deg;

        // global = parent + local (untuk 1 axis yang sama)
        elbowGlobalLock = shoulder + elbowLocal;

        isInitialized = true;
    }
    
    void FixedUpdate()
    {
        if (joints == null || joints.Length == 0) return;

        if (globalEulerAngles == null || globalEulerAngles.Length != joints.Length)
            globalEulerAngles = new Vector3[joints.Length];

        if (!lockElbow && Time.time - lastElbowInputTime > inputDelay)
        {
            InitializeElbowLock();
            lockElbow = true;
        }

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null) continue;

            var drive = joints[i].xDrive;

            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit; 

            float target = targetAngles[i];

            // 🔥 KHUSUS ELBOW (index 2)
            if (lockElbow && i == 2 && isInitialized && !isIK)
            {
                float shoulderAngle = joints[1].jointPosition[0] * Mathf.Rad2Deg;

                target = elbowGlobalLock - shoulderAngle;
                targetAngles[i] = Mathf.Round(target);
            }

            if (i == 2 && isInitialized && isIK)
            {
                float shoulder = joints[1].jointPosition[0] * Mathf.Rad2Deg;
                float elbowLocal = joints[2].jointPosition[0] * Mathf.Rad2Deg;
                elbowGlobalLock = shoulder + elbowLocal;
            }

            drive.target = target;

            joints[i].xDrive = drive;

            Vector3 euler = joints[i].transform.rotation.eulerAngles;

            euler.x = Mathf.Round(euler.x * 100f) / 100f;
            euler.y = Mathf.Round(euler.y * 100f) / 100f;
            euler.z = Mathf.Round(euler.z * 100f) / 100f;

            globalEulerAngles[i] = euler;
        }
    }

    // Fungsi Khusus untuk disambungkan ke UI Slider (Layar HP) nanti
    public void SetAngle(int index, float angle)
    {
        if (index < 0 || index >= targetAngles.Length) return;

        if (index == 0)
        {
            if (angle < j1Min)
                targetAngles[index] = j1Min;
            else if (angle > j1Max)
                targetAngles[index] = j1Max;
            else
                targetAngles[index] = angle;
        }
        else
        {
            if (angle < -180f)
                targetAngles[index] = -180f;
            else if (angle > 180f)
                targetAngles[index] = 180f;
            else
                targetAngles[index] = angle;
        }

        // JIKA ELBOW DIGERAKKAN → UPDATE LOCK
        if (index == 2 && lockElbow)
        {
            lastElbowInputTime = Time.time;
            lockElbow = false;
        }
    }

    // Fungsi sinkronisasi nilai join mode fk dan mode ik
    public void SyncFromCurrentPose()
    {
        if (joints == null) return;

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null) continue;

            float angle = joints[i].jointPosition[0] * Mathf.Rad2Deg;

            targetAngles[i] = angle;
        }

        // 🔥 update lock elbow juga
        InitializeElbowLock();

        Debug.Log("[SYNC] Joint → FK targetAngles updated");
    }
}