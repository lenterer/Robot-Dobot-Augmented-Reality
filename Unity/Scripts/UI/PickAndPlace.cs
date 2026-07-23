using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickAndPlace : MonoBehaviour
{
    [System.Serializable]
    public struct RobotAction
    {
        public float j1;
        public float j2;
        public float j3;
        public float r;
        public int suction; // 0 untuk OFF, 1 untuk ON
    }

    [Header("Data Hasil Perekaman")]
    private System.Collections.Generic.List<RobotAction> actionList = new System.Collections.Generic.List<RobotAction>();

    [Header("Referensi Spasial Robot")]
    public Transform robotBase;               // Tarik objek Base (sendi paling bawah) robot
    public Transform TargetEndEffector;
    public IKButtonController_2 ikController; // Kontroler UI IK yang memegang fungsi MoveToPosition
    public MultiJointControl jointController;
    public WebSocketClient webSocketClient;   // Komponen WebSocket untuk kirim data ke Python

    [Header("Referensi Manager Box")]
    public BoxManager boxManager;             // Tarik objek yang memegang skrip BoxManager

    private float offsetVertikal = 0.067f;       // (0.087f)  Jarak vertikal dari ujung batang ke ujung karet suction cup
    private float offsetHorizontal = 0.066f;     // Jarak horizontal (maju) dari ujung batang ke suction cup

    [Header("Pengaturan Waktu (Detik)")]
    public float travelTime = 2.5f;           // Waktu tunggu pergerakan lengan robot
    public float actionTime = 1.0f;           // Waktu tunggu saat grip/release

    [Header("Pengaturan Kecepatan Gerak")]
    public float moveSpeed = 0.1f; // Kecepatan konstan robot (meter per detik)

    private bool isProcessing = false;

    // Fungsi utama yang dipanggil oleh tombol UI "Start Automation"
    public void StartAutomation()
    {
        // Validasi ketersediaan komponen sebelum memulai
        if (isProcessing)
        {
            Debug.Log("[AUTO] Proses sedang berjalan, tunggu hingga selesai!");
            return;
        }

        if (robotBase == null || ikController == null || boxManager == null)
        {
            Debug.LogError("[AUTO] Referensi penting belum dipasang di Inspector!");
            return;
        }

        Transform boxAmbil = boxManager.GetBoxAmbilTransform();
        Transform boxTaruh = boxManager.GetBoxTaruhTransform();

        if (boxAmbil == null || boxTaruh == null)
        {
            Debug.LogError("[AUTO] Box Ambil atau Box Taruh belum di-spawn di scene!");
            return;
        }

        // Jalankan alur otomatisasi sekuensial
        StartCoroutine(ExecutePickAndPlaceRoutine(boxAmbil, boxTaruh));
    }

    private void RecordState(int suctionStatus)
    {
        RobotAction action = new RobotAction();
        
        // Membaca target sudut ril dari joint controller kamu
        action.j1 = jointController.realTarget1;
        action.j2 = jointController.realTarget2;
        action.j3 = jointController.realTarget3;
        action.r  = 0f;
        action.suction = suctionStatus;

        actionList.Add(action);
    }

    // Alur Sekuensial Berbasis Coroutine (State Machine)
    private IEnumerator ExecutePickAndPlaceRoutine(Transform boxAmbil, Transform boxTaruh)
    {
        isProcessing = true;
        actionList.Clear(); // Bersihkan list lama sebelum mulai merekam tugas baru
        Debug.Log("[AUTO] Memulai proses pemindahan dan perekaman 8 langkah...");

        // Hitung posisi koordinat batang
        Vector3 posisiAtasAmbil = HitungPosisiBatang(boxAmbil) + new Vector3(0, 0.12f, 0); // Naikkan 5cm untuk posisi "di atas"
        Vector3 posisiPasAmbil   = HitungPosisiBatang(boxAmbil);
        Vector3 posisiAtasTaruh = HitungPosisiBatang(boxTaruh) + new Vector3(0, 0.12f, 0); // Naikkan 5cm untuk posisi "di atas"
        Vector3 posisiPasTaruh   = HitungPosisiBatang(boxTaruh);

        // --- URUTAN 1: POSISI DI ATAS BOX AMBIL ---
        yield return StartCoroutine(MoveRobotLerp(posisiAtasAmbil));
        RecordState(0);

        // --- URUTAN 2: POSISI BOX AMBIL ---
        yield return StartCoroutine(MoveRobotLerp(posisiPasAmbil));
        RecordState(0);

        // --- URUTAN 3: SUCTION ON ---
        SetBoxParent(boxAmbil, true);
        yield return new WaitForSeconds(actionTime);
        RecordState(1); // Rekam dengan status suction aktif (1)

        // --- URUTAN 4: POSISI ATAS BOX AMBIL ---
        yield return StartCoroutine(MoveRobotLerp(posisiAtasAmbil));
        RecordState(1);

        // --- URUTAN 5: POSISI ATAS BOX TARUH ---
        yield return StartCoroutine(MoveRobotLerp(posisiAtasTaruh));
        RecordState(1);

        // --- URUTAN 6: POSISI BOX TARUH ---
        yield return StartCoroutine(MoveRobotLerp(posisiPasTaruh));
        RecordState(1);

        // --- URUTAN 7: SUCTION OFF ---
        SetBoxParent(boxAmbil, false);
        yield return new WaitForSeconds(actionTime);
        RecordState(0); // Rekam dengan status suction mati (0)

        // --- URUTAN 8: POSISI ATAS BOX TARUH ---
        yield return StartCoroutine(MoveRobotLerp(posisiAtasTaruh));
        RecordState(0);

        Debug.Log("[AUTO] Perekaman 8 langkah selesai. Siap mengirim data...");

        isProcessing = false;
    }

    // Fungsi pembantu agar kode perulangan Lerp tidak ditulis berulang-ulang
    private IEnumerator MoveRobotLerp(Vector3 targetPos)
    {
        Vector3 startPos = ikController.ikTarget.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = Mathf.Max(distance / moveSpeed, 0.0001f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            ikController.MoveToPosition(currentPos);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
    }

    /// Fungsi Matematika Vektor untuk Menghitung Kompensasi Posisi Ujung Batang
    public Vector3 HitungPosisiBatang(Transform boxTarget)
    {
        // 1. Dapatkan posisi permukaan atas box (Target Akhir TCP)
        float halfBoxHeight = boxTarget.localScale.y / 2f;
        Vector3 posisiAtasBox = boxTarget.position + new Vector3(0, halfBoxHeight, 0);

        // 2. Hitung arah horizontal dari Box menuju ke Base/Pusat Robot
        Vector3 arahKeRobot = robotBase.position - posisiAtasBox;
        arahKeRobot.y = 0; // Kunci sumbu Y (kalkulasi murni 2D horizontal X dan Z)
        arahKeRobot.Normalize(); // Ubah menjadi vektor satuan (panjang = 1)

        // 3. Hitung pergeseran mundur secara horizontal dan naik secara vertikal
        Vector3 pergeseranHorizontal = arahKeRobot * offsetHorizontal;
        Vector3 pergeseranVertikal = new Vector3(0, offsetVertikal, 0);

        // 4. Gabungkan seluruh komponen vektor
        Vector3 posisiBatangBayangan = posisiAtasBox + pergeseranHorizontal + pergeseranVertikal;

        return posisiBatangBayangan;
    }

    private void SetBoxParent(Transform box, bool attachToRobot)
    {
        if (box == null) return;

        if (attachToRobot)
        {
            // Jadikan box sebagai anak dari target IK (ujung robot) agar otomatis ikut bergerak
            box.SetParent(TargetEndEffector, true);
        }
        else
        {
            // Kembalikan box ke kontainer AR utama saat dilepas
            box.SetParent(boxManager.boxContainer, true);
        }
    }

    public void SendActionListAsJson()
    {
        if (webSocketClient == null || actionList.Count == 0) return;

        // Membuat format array JSON secara manual agar ringan dan presisi
        string jsonArrayStr = "[";
        
        for (int i = 0; i < actionList.Count; i++)
        {
            string j1Str = actionList[i].j1.ToString("F2").Replace(",", ".");
            string j2Str = actionList[i].j2.ToString("F2").Replace(",", ".");
            string j3Str = actionList[i].j3.ToString("F2").Replace(",", ".");
            string rStr  = actionList[i].r.ToString("F2").Replace(",", ".");
            int suc      = actionList[i].suction;

            jsonArrayStr += "{\"j1\":" + j1Str + ",\"j2\":" + j2Str + ",\"j3\":" + j3Str + ",\"r\":" + rStr + ",\"suction\":" + suc + "}";
            
            // Tambahkan koma pemisah jika bukan elemen terakhir
            if (i < actionList.Count - 1) jsonArrayStr += ",";
        }
        
        jsonArrayStr += "]";

        // Bungkus dengan header utama "teleop" sesuai protokol komunikasimu
        string finalJson = "{\"box\":" + jsonArrayStr + "}";

        // Kirim data utuh ke Python via WebSocket
        webSocketClient.SendPlayerData(finalJson);
        Debug.Log("[JSON MACRO] Berhasil mengirim 8 urutan aksi: " + finalJson);
    }
}
