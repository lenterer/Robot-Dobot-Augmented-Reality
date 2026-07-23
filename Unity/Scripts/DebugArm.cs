using UnityEngine;

public class ArmDebugger : MonoBehaviour
{
    [Header("Assign Joint Transform")]
    public Transform shoulder;     // Joint 2
    public Transform elbow;        // Joint 3
    public Transform endEffector;  // Ujung

    [Header("Length (Auto Calculated)")]
    public float L1;
    public float L2;

    [Header("End Effector Position (Global)")]
    public Vector3 endEffectorWorldPos;

    [Header("Visual Settings")]
    public bool showGizmos = true;
    public float jointSize = 0.02f;

    void Start()
    {
        CalculateLength();
    }

    void Update()
    {
        // Update terus kalau robot bergerak
        CalculateLength();

        if (endEffector != null)
        {
            endEffectorWorldPos = endEffector.position;
        }
    }

    void CalculateLength()
    {
        if (shoulder == null || elbow == null || endEffector == null)
            return;

        L1 = Vector3.Distance(shoulder.position, elbow.position);
        L2 = Vector3.Distance(elbow.position, endEffector.position);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (shoulder == null || elbow == null || endEffector == null)
            return;

        // Garis L1
        Gizmos.color = Color.green;
        Gizmos.DrawLine(shoulder.position, elbow.position);

        // Garis L2
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(elbow.position, endEffector.position);

        // Titik joint
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(shoulder.position, jointSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(elbow.position, jointSize);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(endEffector.position, jointSize);
    }
}