using UnityEngine;

public class RobotScaleController : MonoBehaviour
{
    private InverseKinematics3DOF_2 ikController;
    private float currentScale = 1.0f;

    void Start()
    {
        ikController = GetComponent<InverseKinematics3DOF_2>();
        currentScale = transform.localScale.x;
    }

    public float GetCurrentScale()
    {
        return currentScale;
    }

    /// <summary>
    /// Fungsi inti untuk mengubah skala fisik robot dan update link IK
    /// </summary>
    public void ApplyScale(float newScale)
    {
        Vector3 previousScale = transform.localScale;
        transform.localScale = Vector3.one * newScale;
        currentScale = newScale;

        // 1. Perbarui jangkar fisika ArticulationBody di semua sendi anak
        ArticulationBody[] bodies = GetComponentsInChildren<ArticulationBody>();
        foreach (ArticulationBody body in bodies)
        {
            if (body.isRoot) continue;

            float scaleRatio = newScale / previousScale.x;
            body.parentAnchorPosition *= scaleRatio;
            body.anchorPosition *= scaleRatio;
        }

        // 2. Hitung ulang panjang lengan robot pada komponen IK (sudah public)
        if (ikController != null)
        {
            ikController.CalculateLinkLength();

            // 3. --- TAMBAHAN UNTUK FIX SINKRONISASI TARGET IK ---
            // Memaksa posisi target IK kembali menempel pada endEffector setelah skala berubah
            if (ikController.target != null && ikController.endEffector != null)
            {
                ikController.target.position = ikController.endEffector.position;
                ikController.target.rotation = ikController.endEffector.rotation;
                
                Debug.Log("[SCALE] Posisi ikTarget berhasil disinkronkan ulang dengan End Effector baru.");
            }
        }

        Debug.Log($"[SCALE] Skala fisik robot diperbarui ke: {newScale:F2}");
    }
}