using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ColliderOutlineRenderer : MonoBehaviour
{
    [Header("Target Collider")]
    public BoxCollider targetCollider; // Drag BoxCollider pinggir robot ke sini

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // Pengaturan dasar Line Renderer
        lineRenderer.useWorldSpace = true; // Tetap TRUE karena kita konversi manual ke koordinat AR
        lineRenderer.positionCount = 5;    // 5 titik agar garis kembali menyambung ke titik awal (kotak)
        
        // Ketebalan garis (1 cm)
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
    }

    void Update()
    {
        if (targetCollider == null || lineRenderer == null) return;

        // 1. Ambil data ukuran dan pusat dari Box Collider
        Vector3 size = targetCollider.size;
        Vector3 center = targetCollider.center;

        // 2. Hitung posisi 4 titik pojok bawah/atas collider secara LOKAL
        // Kita ambil bidang horizontal (X dan Z), Y diatur sesuai posisi collider
        float extX = size.x * 0.5f;
        float extZ = size.z * 0.5f;

        Vector3[] pojokLokal = new Vector3[5];
        
        // Membuat pola kotak menyambung
        pojokLokal[0] = center + new Vector3(-extX, 0f, -extZ); // Pojok Kiri Belakang
        pojokLokal[1] = center + new Vector3(extX, 0f, -extZ);  // Pojok Kanan Belakang
        pojokLokal[2] = center + new Vector3(extX, 0f, extZ);   // Pojok Kanan Depan
        pojokLokal[3] = center + new Vector3(-extX, 0f, extZ);  // Pojok Kiri Depan
        pojokLokal[4] = pojokLokal[0];                          // Kembali ke awal agar menyambung

        // 3. Konversi semua titik lokal tadi ke posisi Dunia AR mengikuti objek bodi robot
        for (int i = 0; i < 5; i++)
        {
            // Menggunakan transform dari tempat BoxCollider itu menempel
            Vector3 posisiDunia = targetCollider.transform.TransformPoint(pojokLokal[i]);
            lineRenderer.SetPosition(i, posisiDunia);
        }
    }
}