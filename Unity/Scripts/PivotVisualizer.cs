using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PivotVisualizer : MonoBehaviour
{
    public List<Transform> links = new List<Transform>();
    public float sphereSize = 0.02f;

    void OnDrawGizmos()
    {
        if (links == null || links.Count == 0) return;

        Gizmos.color = Color.red;

        // Gambar pivot (bola)
        foreach (Transform t in links)
        {
            if (t != null)
            {
                Gizmos.DrawSphere(t.position, sphereSize);
            }
        }

        Gizmos.color = Color.green;

        for (int i = 0; i < links.Count - 1; i++)
        {
            if (links[i] != null && links[i + 1] != null)
            {
                Transform parent = links[i];
                Transform child  = links[i + 1];

                Vector3 p1 = parent.position;
                Vector3 p2 = child.position;

                Gizmos.DrawLine(p1, p2);

                float distance = Vector3.Distance(p1, p2);

                // 🔥 WORLD delta
                Vector3 deltaWorld = p2 - p1;

                // 🔥 LOCAL delta (INI YANG SESUAI URDF)
                Vector3 deltaLocal = parent.InverseTransformPoint(p2);

#if UNITY_EDITOR
                Handles.Label(
                    (p1 + p2) / 2,
                    $"{parent.name} → {child.name}\n" +
                    $"Dist: {distance:F3} m\n" +
                    $"[World]\n" +
                    $"dx: {deltaWorld.x:F3}\n" +
                    $"dy: {deltaWorld.y:F3}\n" +
                    $"dz: {deltaWorld.z:F3}\n\n" +
                    $"[Local / URDF]\n" +
                    $"dx: {deltaLocal.x:F3}\n" +
                    $"dy: {deltaLocal.y:F3}\n" +
                    $"dz: {deltaLocal.z:F3}"
                );
#endif
            }
        }
    }
}