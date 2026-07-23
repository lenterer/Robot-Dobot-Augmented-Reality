using UnityEngine;

public class GizmoPoint : MonoBehaviour
{
    public Color gizmoColor = Color.red;
    public float size = 0.01f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, size);
    }
}