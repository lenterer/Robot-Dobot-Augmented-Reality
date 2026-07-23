using UnityEngine;

public class InverseKinematics3DOF : MonoBehaviour
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

    private float L1 = 0.1485f;
    private float L2 = 0.1617f;

    [Header("Output Angle (deg)")]
    public float theta1;
    public float theta2;
    public float theta3;

    [Header("Controller")]
    public SimpleJointController jointController;

    [Header("IK Debug Visual")]
    public bool showIKGizmos = true;
    public float gizmoSize = 0.02f;

    // private float baseOffset = 0f;
    private float shoulderOffset = 90.068f;
    private float elbowOffset = -90.068f;

    // ===== LIMIT JOINT =====
    private float baseMin = -90f;
    private float baseMax = 90f;

    private float shoulderMin = 0f;
    private float shoulderMax = 80f;

    private float elbowMin = -80f;
    private float elbowMax = 50f;

    // Titik hasil IK
    private Vector3 p0, p2, p3;

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
        // =========================
        // WORLD SPACE (AMAN)
        // =========================
        Vector3 dirBase = target.position - baseJoint.position;
        Vector3 localDir = baseJoint.InverseTransformPoint(target.position);
        Vector3 reconstructed = baseJoint.TransformPoint(localDir);
        Vector3 dirBase2 = reconstructed - baseJoint.position;

        // =========================
        // BASE
        // =========================
        Vector3 flatDir = new Vector3(dirBase2.x, 0, dirBase2.z);
        theta1 = Mathf.Atan2(flatDir.z, flatDir.x) * Mathf.Rad2Deg;

        // =========================
        // 2D IK (SHOULDER + ELBOW)
        // =========================
        Vector3 dir = target.position - shoulderJoint.position;
        float r = new Vector2(dir.x, dir.z).magnitude;
        float y = dir.y;

        float D = (r*r + y*y - L1*L1 - L2*L2) / (2 * L1 * L2);
        D = Mathf.Clamp(D, -1f, 1f);

        // ELBOW
        float theta3Rad = -Mathf.Acos(D);
        theta3 = theta3Rad * Mathf.Rad2Deg;

        // SHOULDER
        float theta2Rad = Mathf.Atan2(y, r) -
                        Mathf.Atan2(L2 * Mathf.Sin(theta3Rad),
                                    L1 + L2 * Mathf.Cos(theta3Rad));

        theta2 = theta2Rad * Mathf.Rad2Deg;

        // =========================
        // 🔥 FORWARD KINEMATICS (DEBUG)
        // =========================

        // Base position
        p0 = shoulderJoint.position;

        // Rotasi base
        Vector3 dirLine = new Vector3(
            Mathf.Cos(theta1 * Mathf.Deg2Rad),
            0,
            Mathf.Sin(theta1 * Mathf.Deg2Rad)
        );

        // Elbow (L1)
        p2 = p0 +
            dirLine * (Mathf.Cos(theta2 * Mathf.Deg2Rad) * L1) +
            Vector3.up * (Mathf.Sin(theta2 * Mathf.Deg2Rad) * L1);
        
        // End Effector (L2)
        float totalAngle = theta2 + theta3;

        p3 = p2 +
            dirLine * (Mathf.Cos(totalAngle * Mathf.Deg2Rad) * L2) +
            Vector3.up * (Mathf.Sin(totalAngle * Mathf.Deg2Rad) * L2);
    }

    void SendToController()
    {
        if (jointController == null) return;

        // Mapping IK → Joint
        float baseAngle = theta1;
        float shoulderAngle = shoulderOffset - theta2;
        float elbowAngle = elbowOffset - theta3;

        // ===== LIMIT =====
        if (baseAngle < baseMin)
        {
            baseAngle = baseMin;
            // Debug.Log("⚠️ Base kena MIN limit");
        }
        else if (baseAngle > baseMax)
        {
            baseAngle = baseMax;
            // Debug.Log("⚠️ Base kena MAX limit");
        }

        // ===== LIMIT SHOULDER =====
        if (shoulderAngle < shoulderMin)
        {
            shoulderAngle = shoulderMin;
            // Debug.Log("⚠️ Shoulder kena MIN limit");
        }
        else if (shoulderAngle > shoulderMax)
        {
            shoulderAngle = shoulderMax;
            // Debug.Log("⚠️ Shoulder kena MAX limit");
        }

        // ===== LIMIT ELBOW =====
        if (elbowAngle < elbowMin)
        {
            elbowAngle = elbowMin;
            // Debug.Log("⚠️ Elbow kena MIN limit");
        }
        else if (elbowAngle > elbowMax)
        {
            elbowAngle = elbowMax;
            // Debug.Log("⚠️ Elbow kena MAX limit");
        }

        // ⚠️ Sesuaikan tanda kalau arah kebalik
        jointController.targetAngles[0] = baseAngle;
        jointController.targetAngles[1] = shoulderAngle;
        jointController.targetAngles[2] = elbowAngle;
    }
    
    void OnDrawGizmos()
    {
        if (baseJoint != null && target != null)
        {
            // WORLD direction (kuning)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(baseJoint.position, target.position);

            // LOCAL direction (cyan, hasil rekonstruksi)
            Vector3 localDir = baseJoint.InverseTransformPoint(target.position);
            Vector3 reconstructed = baseJoint.TransformPoint(localDir);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(baseJoint.position, reconstructed);
        }
    }
}