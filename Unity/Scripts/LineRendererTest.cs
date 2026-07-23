using UnityEngine;

public class LineRendererTest : MonoBehaviour
{
    public LineRenderer lineRenderer;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer tidak ditemukan!");
            return;
        }

        // Jumlah titik
        lineRenderer.positionCount = 0; // RESET dulu

        lineRenderer.positionCount = 3;

        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, new Vector3(0.2f, 0, 0.2f));
        lineRenderer.SetPosition(2, new Vector3(0.4f, 0, 0));

        Debug.Log("LineRenderer test berhasil dibuat");
    }
}