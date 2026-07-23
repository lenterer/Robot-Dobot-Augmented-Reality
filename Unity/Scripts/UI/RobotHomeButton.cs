using UnityEngine;

public class RobotHomeButton : MonoBehaviour
{
    [Header("Referensi Spasial & Kontrol")]
    public MultiJointControl jointController; // Tarik objek yang memegang skrip MultiJointControl
    public WebSocketClient webSocketClient;   // Tarik objek yang memegang skrip WebSocketClient

    // Target sudut home yang diinginkan (-45, 0, 0)
    private const float homeJ1 = 0f;
    private const float homeJ11 = -35f;
    private const float homeJ2 = 0f;
    private const float homeJ22 = 5f;
    private const float homeJ3 = 0f;
    private const float homeR  = 0f;

    /// <summary>
    /// Fungsi utama yang dipanggil saat tombol Home di klik pada UI Canvas
    /// </summary>
    public void GoToHomePosition()
    {
        // 1. Validasi komponen sebelum eksekusi
        if (jointController == null)
        {
            Debug.LogError("[HOME] Error: Variabel 'jointController' belum dipasang di Inspector!");
            return;
        }

        // 2. Gerakkan Lengan Robot Virtual di Unity
        // Mengubah realTarget langsung agar model robot di AR langsung bergerak ke posisi home
        jointController.target1 = homeJ1;
        jointController.target2 = homeJ2;
        jointController.SetTarget(2, homeJ3);

        Debug.Log($"[HOME] Robot virtual bergerak ke posisi awal: J1:{homeJ1}, J2:{homeJ2}, J3:{homeJ3}");

        // 3. Kirim Koordinat Home ke Dobot Fisik melalui WebSocket
        if (webSocketClient != null)
        {
            // Format string disesuaikan dengan struktur "teleop" tunggal yang dibaca Python
            string j1Str = homeJ11.ToString("F2").Replace(",", ".");
            string j2Str = homeJ22.ToString("F2").Replace(",", ".");
            string j3Str = homeJ3.ToString("F2").Replace(",", ".");

            string jsonHome = "{\"teleop\":{\"target1\":" + j1Str + ",\"target2\":" + j2Str + ",\"target3\":" + j3Str + "}}";
            
            webSocketClient.SendPlayerData(jsonHome);
            Debug.Log("[HOME] Berhasil mengirim koordinat home ke Python: " + jsonHome);
        }
        else
        {
            Debug.LogWarning("[HOME] WebSocketClient tidak terdeteksi, gerakan hanya terjadi di simulasi virtual.");
        }
    }
}