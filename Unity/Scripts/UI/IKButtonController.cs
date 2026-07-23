using UnityEngine;

public class IKButtonController : MonoBehaviour
{
    Transform ikTarget;

    [Header("Besar Perpindahan")]
    public float step = 0.01f;

    public void SetIKTarget(Transform target)
    {
        ikTarget = target;
        Debug.Log("[IK] Target diterima: " + target.name);
    }
    
    // ===== X =====
    public void XPlus()
    {
        MoveTarget(Vector3.right * step);
        Debug.Log("Tambah FK");
    }

    public void XMinus()
    {
        MoveTarget(Vector3.left * step);
    }

    // ===== Y =====
    public void YPlus()
    {
        MoveTarget(Vector3.up * step);
    }

    public void YMinus()
    {
        MoveTarget(Vector3.down * step);
    }

    // ===== Z =====
    public void ZPlus()
    {
        MoveTarget(Vector3.forward * step);
    }

    public void ZMinus()
    {
        MoveTarget(Vector3.back * step);
    }

    // ===== CORE MOVE =====
    void MoveTarget(Vector3 direction)
    {
        if (ikTarget == null)
        {
            Debug.LogWarning("[IK] Target belum ada!");
            return;
        }

        ikTarget.position += direction;

        Debug.Log("[IK MOVE] Posisi: " + ikTarget.position);
    }

    // ===== SET TARGET LANGSUNG =====
    public void MoveToPosition(Vector3 targetPos)
    {
        if (ikTarget == null)
        {
            Debug.LogWarning("[IK] Target belum ada!");
            return;
        }

        ikTarget.position = targetPos;

        Debug.Log("[IK MOVE] Menuju waypoint: " + targetPos);
    }
}