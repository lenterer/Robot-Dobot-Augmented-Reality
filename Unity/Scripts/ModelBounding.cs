using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoundingBoxDrawer : MonoBehaviour
{
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.loop = false;
        line.useWorldSpace = true;
        line.widthMultiplier = 0.01f;

        DrawBoundingBox();
    }

    void DrawBoundingBox()
    {
        Bounds bounds = GetBounds();

        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        // 8 titik sudut
        Vector3[] p = new Vector3[8];

        p[0] = c + new Vector3(-e.x, -e.y, -e.z);
        p[1] = c + new Vector3(e.x, -e.y, -e.z);
        p[2] = c + new Vector3(e.x, -e.y, e.z);
        p[3] = c + new Vector3(-e.x, -e.y, e.z);

        p[4] = c + new Vector3(-e.x, e.y, -e.z);
        p[5] = c + new Vector3(e.x, e.y, -e.z);
        p[6] = c + new Vector3(e.x, e.y, e.z);
        p[7] = c + new Vector3(-e.x, e.y, e.z);

        // urutan garis (12 edge → 24 titik)
        Vector3[] lines = new Vector3[]
        {
            // bawah
            p[0], p[1], p[1], p[2], p[2], p[3], p[3], p[0],

            // atas
            p[4], p[5], p[5], p[6], p[6], p[7], p[7], p[4],

            // vertikal
            p[0], p[4], p[1], p[5], p[2], p[6], p[3], p[7]
        };

        line.positionCount = lines.Length;
        line.SetPositions(lines);
    }

    Bounds GetBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds;
    }
}