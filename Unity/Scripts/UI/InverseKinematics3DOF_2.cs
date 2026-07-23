using UnityEngine;

public class InverseKinematics3DOF_2 : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Base Joint (Referensi)")]
    public Transform baseJoint;

    [Header("Shoulder Joint (Pivot IK)")]
    public Transform shoulderJoint;

    [Header("Robot Real Transform")]
    public Transform shoulder;
    public Transform elbow;
    public Transform endEffector;

    [Header("Arm Length")]
    public float L1 = 0.135f;
    public float L2 = 0.147f;

    [Header("Output Angle (deg)")]
    public float theta1;
    public float theta2;
    public float theta3;

    [Header("Controller")]
    public MultiJointControl jointController;

    // private float baseOffset = 0f;
    private float shoulderOffset = 90.068f;
    private float elbowOffset = -90.068f;

    // Titik hasil IK
    private Vector3 p0, p2, p3;
    private Vector3 ikShoulderPos;
    private Vector3 ikElbowPos;
    private Vector3 ikEndPos;
    private Vector3 debugShoulderPos;
    private Vector3 debugElbowPos;
    private Vector3 debugEndPos;

    void Start()
    {
        CalculateLinkLength();
    }

    void OnEnable()
    {
        // 1. Pastikan panjang lengan diupdate saat skrip aktif (penting untuk AR)
        CalculateLinkLength();

        // 2. Paksa target (kubus) ke posisi endEffector sekarang agar tidak meloncat
        if (target != null && endEffector != null)
        {
            target.position = endEffector.position;
            target.rotation = endEffector.rotation;
        }

        // 3. Update sudut theta dari nilai yang ada di controller sekarang
        if (jointController != null)
        {
            theta1 = jointController.target1; 
            theta2 = shoulderOffset - jointController.target2;
            theta3 = elbowOffset - jointController.target3;
        }
    }
    
    void Update()
    {
        if (target == null || baseJoint == null) return;

        SolveIK();

        // 🔥 Debug sudut (tidak spam)
        if (Time.frameCount % 30 == 0)
        {
        }

        SendToController();
    }

    void SolveIK()
    {
        // =================================================================
        // 1. GUNAKAN REFERENSI PARENT YANG STABIL
        // =================================================================
        Vector3 localTargetParent;
        if (baseJoint.parent != null)
            localTargetParent = baseJoint.parent.InverseTransformPoint(target.position);
        else
            localTargetParent = transform.InverseTransformPoint(target.position);
        Vector3 localTargetArm = baseJoint.InverseTransformPoint(target.position);

        // =================================================================
        // 2. BASE (Theta 1) - (Sudah Benar)
        // =================================================================
        float rawTheta1 = -Mathf.Atan2(localTargetParent.z, localTargetParent.x) * Mathf.Rad2Deg;
        theta1 = Mathf.Round(rawTheta1 * 100f) / 100f;

        // =================================================================
        // 3. 2D IK (SHOULDER + ELBOW) - SEKARANG STABIL
        // =================================================================
        float currentRobotScale = transform.localScale.x;

        // KALIKAN r dan y dengan skala untuk membatalkan efek pembagian otomatis dari Unity
        float r = localTargetArm.x * currentRobotScale;
        
        // y_lokal dikali skala, baru dikurangi tinggi bahu asli (tidak perlu dikali scale lagi karena y sudah disetarakan ke dunia)
        float y = (localTargetArm.y * currentRobotScale) - (shoulderJoint.localPosition.y * currentRobotScale);

        // Sekarang r, y, L1, dan L2 semuanya berada di skala dunia nyata (meter) yang sama!
        float D = (r * r + y * y - L1 * L1 - L2 * L2) / (2 * L1 * L2);
        D = Mathf.Clamp(D, -1f, 1f);

        // ELBOW (Theta 3)
        float theta3Rad = -Mathf.Acos(D);
        theta3 = theta3Rad * Mathf.Rad2Deg;

        // SHOULDER (Theta 2)
        float theta2Rad = Mathf.Atan2(y, r) -
                        Mathf.Atan2(L2 * Mathf.Sin(theta3Rad),
                                    L1 + L2 * Mathf.Cos(theta3Rad));

        theta2 = theta2Rad * Mathf.Rad2Deg;

        // =================================================================
        // 4. UPDATE DEBUG GIZMO (Tetap sama)
        // =================================================================
        debugShoulderPos = shoulderJoint.position;
        Quaternion baseParentRotation = (baseJoint.parent != null) ? baseJoint.parent.rotation : Quaternion.identity;
        Quaternion finalBaseRot = baseParentRotation * Quaternion.Euler(0, theta1, 0);
        Vector3 localForward = finalBaseRot * Vector3.right; 

        float r1 = Mathf.Cos(theta2 * Mathf.Deg2Rad) * L1;
        float y1 = Mathf.Sin(theta2 * Mathf.Deg2Rad) * L1;
        debugElbowPos = debugShoulderPos + (localForward * r1) + (baseJoint.up * y1);

        float r2 = Mathf.Cos((theta2 + theta3) * Mathf.Deg2Rad) * L2;
        float y2 = Mathf.Sin((theta2 + theta3) * Mathf.Deg2Rad) * L2;
        debugEndPos = debugElbowPos + (localForward * r2) + (baseJoint.up * y2);
    }

    void SendToController()
    {
        if (jointController == null) return;

        float baseAngle = theta1;
        float shoulderAngle = shoulderOffset - theta2;
        float elbowAngle = elbowOffset - theta3;

        // 🔥 kirim ke JointControl (WAJIB pakai ini)
        jointController.SetTarget(0, baseAngle);
        jointController.SetTarget(1, shoulderAngle);
        jointController.SetTarget(2, elbowAngle);
    }
    
    public void CalculateLinkLength()
    {
        if (shoulder == null || elbow == null || endEffector == null) return;

        L1 = Vector3.Distance(shoulder.position, elbow.position);
        L2 = Vector3.Distance(elbow.position, endEffector.position);

        Debug.Log($"L1: {L1:F4} | L2: {L2:F4}");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Garis dari Bahu ke Siku (Warna Kuning)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(debugShoulderPos, debugElbowPos);
        Gizmos.DrawSphere(debugElbowPos, 0.01f);

        // Garis dari Siku ke End Effector (Warna Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(debugElbowPos, debugEndPos);
        Gizmos.DrawSphere(debugEndPos, 0.01f);

        // Garis ke Target Asli (Warna Merah)
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(debugEndPos, target.position);
        }
    }
}