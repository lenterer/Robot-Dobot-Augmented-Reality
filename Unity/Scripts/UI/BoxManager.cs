using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [Header("Referensi Prefab")]
    public GameObject boxAmbilPrefab;
    public GameObject boxTaruhPrefab;

    [Header("Referensi Parent (Wajib di bawah AR Image)")]
    public Transform boxContainer; 

    [Header("Pergerakan Box Taruh (Hold)")]
    public float boxMoveSpeed = 0.1f;
    private Vector3 currentMoveDirection = Vector3.zero;

    // Variabel untuk menyimpan objek yang sedang aktif di scene
    public GameObject currentBoxAmbil;
    public GameObject currentBoxTaruh;

    void Update()
    {
        // Hanya bergerak jika Box Taruh ada di scene dan tombol sedang ditahan (direction tidak zero)
        if (currentBoxTaruh != null && currentMoveDirection != Vector3.zero)
        {
            // Menggeser localPosition menggunakan local arah pergerakan agar sinkron dengan orientasi AR
            currentBoxTaruh.transform.localPosition += 
                currentMoveDirection * boxMoveSpeed * Time.deltaTime;
        }else if(currentBoxAmbil != null && currentMoveDirection != Vector3.zero)
        {
            currentBoxAmbil.transform.localPosition += 
                currentMoveDirection * boxMoveSpeed * Time.deltaTime;
        }
    }
    
    // 1. FUNGSI UNTUK MENAMBAH (SPAWN) BOX
    public void SpawnBoxAmbil()
    {
        // Jika box sudah ada, hapus dulu yang lama agar tidak menumpuk
        if (currentBoxAmbil != null) Destroy(currentBoxAmbil);

        // Spawn tepat di posisi container (di atas marker)
        currentBoxAmbil = Instantiate(boxAmbilPrefab, boxContainer.position, Quaternion.identity);
        
        // Masukkan ke dalam container agar satu hierarki dengan ruang AR
        currentBoxAmbil.transform.SetParent(boxContainer);
        
        // Beri sedikit offset posisi agar tidak pas di tengah robot saat muncul
        currentBoxAmbil.transform.localPosition = new Vector3(0.2065f, -0.00995f, 0); 
        
        Debug.Log("[SPAWNER] Box Ambil berhasil dimunculkan.");
    }

    public void SpawnBoxTaruh()
    {
        if (currentBoxTaruh != null) Destroy(currentBoxTaruh);

        currentBoxTaruh = Instantiate(boxTaruhPrefab, boxContainer.position, Quaternion.identity);
        currentBoxTaruh.transform.SetParent(boxContainer);
        currentBoxTaruh.transform.localPosition = new Vector3(0.2065f, -0.00995f, 0); 
        
        Debug.Log("[SPAWNER] Box Taruh berhasil dimunculkan.");
    }

    // 2. FUNGSI UNTUK MENGHAPUS BOX
    public void DeleteBoxes()
    {
        if (currentBoxAmbil != null)
        {
            Destroy(currentBoxAmbil);
            currentBoxAmbil = null;
        }

        if (currentBoxTaruh != null)
        {
            Destroy(currentBoxTaruh);
            currentBoxTaruh = null;
        }
        SpawnBoxAmbil();

        Debug.Log("[SPAWNER] Semua box virtual telah dihapus.");
    }

    public void DeleteAllBoxes()
    {
        if (currentBoxAmbil != null)
        {
            Destroy(currentBoxAmbil);
            currentBoxAmbil = null;
        }

        if (currentBoxTaruh != null)
        {
            Destroy(currentBoxTaruh);
            currentBoxTaruh = null;
        }

        Debug.Log("[SPAWNER] Semua box virtual telah dihapus.");
    }

    // --- FUNGSI UNTUK MEMULAI GERAKAN (Dipanggil oleh onHoldStart tombol UI) ---
    public void StartMoveTaruhRight()    => currentMoveDirection = Vector3.right;
    public void StartMoveTaruhLeft()     => currentMoveDirection = Vector3.left;
    public void StartMoveTaruhForward()  => currentMoveDirection = Vector3.forward;
    public void StartMoveTaruhBackward() => currentMoveDirection = Vector3.back;
    public void StartMoveTaruhUp()       => currentMoveDirection = Vector3.up;
    public void StartMoveTaruhDown()     => currentMoveDirection = Vector3.down;

    // --- FUNGSI UNTUK MENGHENTIKAN GERAKAN (Dipanggil oleh onHoldEnd semua tombol) ---

    public void StopMoveTaruh()
    {
        currentMoveDirection = Vector3.zero;
    }

    public Transform GetBoxAmbilTransform() => currentBoxAmbil != null ? currentBoxAmbil.transform : null;
    public Transform GetBoxTaruhTransform() => currentBoxTaruh != null ? currentBoxTaruh.transform : null;
}