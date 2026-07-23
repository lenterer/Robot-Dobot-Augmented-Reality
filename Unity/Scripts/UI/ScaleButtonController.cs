using UnityEngine;

public class ScaleButtonController : MonoBehaviour
{
    // Komponen ini akan diisi secara dinamis oleh skrip listener saat robot spawn
    private RobotScaleController scaleControl;

    [Header("Pengaturan Batas Skala")]
    public float step = 0.1f;
    public float minScale = 0.1f;
    public float maxScale = 3.0f;

    // ===== TOMBOL UI PLUS =====
    public void Scale_Plus()
    {
        if (scaleControl == null) return;

        float targetScale = scaleControl.GetCurrentScale() + step;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        scaleControl.ApplyScale(targetScale);
    }

    // ===== TOMBOL UI MINUS =====
    public void Scale_Minus()
    {
        if (scaleControl == null) return;

        float targetScale = scaleControl.GetCurrentScale() - step;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        scaleControl.ApplyScale(targetScale);
    }

    // 🔥 Dipanggil dari listener untuk menghubungkan skrip robot ke UI ini
    public void SetScaler(RobotScaleController scaler)
    {
        scaleControl = scaler;
        Debug.Log("[UI] RobotScaleController berhasil terhubung ke UI Scale Button");
    }
}