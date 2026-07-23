using UnityEngine;
using System;

// Struktur data untuk JSON
[Serializable]
public class JointData2
{
    public float target1;
    public float target2;
    public float target3;
}

public class IKButtonController_2 : MonoBehaviour
{
    [Header("Target Referensi")]
    public Transform ikTarget;
    public MultiJointControl jointControl;
    public WebSocketClient webSocketClient;
    public TeleopButton Teleop;
    public TeachingPlayback_2 playbackSystem;

    [Header("Settings")]
    public float moveSpeed = 0.005f;    // Meter per detik
    public float rotateSpeed = 30f;   // Derajat per detik (jika target butuh rotasi)

    private Vector3 currentMoveDirection = Vector3.zero;
    private float currentRotateDirection = 0f;

    private float currentJ4Direction = 0f; // 1 untuk plus, -1 untuk minus
    private float j4DeltaAccumulated = 0f;

    void Update()
    {
        if (ikTarget == null || jointControl == null) return;

        // 1. SIMPAN POSISI SEBELUM BERGERAK
        Vector3 previousPosition = ikTarget.position;
        Quaternion previousRotation = ikTarget.rotation;

        // LOGIKA PERGERAKAN (MOVE)
        if (currentMoveDirection != Vector3.zero)
        {
            Vector3 localMoveVector = ikTarget.TransformDirection(currentMoveDirection);

            ikTarget.position += 
                localMoveVector * moveSpeed * Time.deltaTime;
        }

        // LOGIKA ROTASI (ROTATE)
        if (currentRotateDirection != 0f)
        {
            ikTarget.Rotate(Vector3.up, 
                currentRotateDirection * rotateSpeed * Time.deltaTime);
        }

        // LOGIKA AKUMULASI J4 HOLD
        if (currentJ4Direction != 0f)
        {
            j4DeltaAccumulated += currentJ4Direction * rotateSpeed * Time.deltaTime;
            j4DeltaAccumulated = Mathf.Clamp(j4DeltaAccumulated, -140, 140);
            playbackSystem.joint4 += currentJ4Direction * rotateSpeed * Time.deltaTime;
            playbackSystem.joint4 = Mathf.Clamp(playbackSystem.joint4, -140, 140);
        }

        // 2. VALIDASI BATAS KERJA ROBOT
        float minT3_Dynamic = 0f - jointControl.target2;
        float maxT3_Dynamic = 80f - jointControl.target2;

        bool isOvershoot = 
            jointControl.target1 < -80f || jointControl.target1 > 80f ||
            jointControl.target2 < 0f   || jointControl.target2 > 55f ||
            jointControl.target3 < minT3_Dynamic || jointControl.target3 > maxT3_Dynamic;

        if (isOvershoot)
        {
            // 3. ROLLBACK KEDUA: Kembalikan ke posisi aman frame sebelumnya
            ikTarget.position = previousPosition;
            ikTarget.rotation = previousRotation;

            // 4. ANTI-STUCK: Berikan dorongan kecil ke arah KEBALIKAN dari tombol yang sedang ditekan
            // Ini agar target keluar dari area 'dinding pembatas' dan tombol tidak mengunci
            if (currentMoveDirection != Vector3.zero)
            {
                // Mendorong mundur target sejauh 2 milimeter dari arah tabrakan
                ikTarget.position -= currentMoveDirection * 0.002f; 
            }

            // 5. LOCKING: Amankan nilai variabel joint agar sinkron
            jointControl.target1 = Mathf.Clamp(jointControl.target1, -80f, 80f);
            jointControl.target2 = Mathf.Clamp(jointControl.target2, 0f, 55f);
            
            minT3_Dynamic = 0f - jointControl.target2;
            maxT3_Dynamic = 80f - jointControl.target2;
            
            float clampedT3 = Mathf.Clamp(jointControl.target3, minT3_Dynamic, maxT3_Dynamic);
            jointControl.target3 = clampedT3;
            jointControl.SetTarget(2, clampedT3);

            // 6. STOP MOTION: Paksa pergerakan berhenti sejenak pada frame ini 
            // agar user harus melepas atau menekan tombol arah lain
            currentMoveDirection = Vector3.zero;
            currentRotateDirection = 0f;
            currentJ4Direction = 0f;
        }
    }

    // --- FUNGSI START MOVE (Dipanggil onHoldStart) ---

    public void StartMoveRight()    => currentMoveDirection = Vector3.right;
    public void StartMoveLeft()     => currentMoveDirection = Vector3.left;
    public void StartMoveForward()  => currentMoveDirection = Vector3.forward;
    public void StartMoveBackward() => currentMoveDirection = Vector3.back;
    public void StartMoveUp()       => currentMoveDirection = Vector3.up;
    public void StartMoveDown()     => currentMoveDirection = Vector3.down;
    public void StartRotateRight()  => currentRotateDirection = 1f;
    public void StartRotateLeft()   => currentRotateDirection = -1f;

    // --- FUNGSI START HOLD J4 ---
    public void StartJ4_Plus()
    {
        currentJ4Direction = 1f;
        j4DeltaAccumulated = 0f; // Reset delta awal setiap kali mulai ditekan baru
    }

    public void StartJ4_Minus()
    {
        currentJ4Direction = -1f;
        j4DeltaAccumulated = 0f; // Reset delta awal setiap kali mulai ditekan baru
    }

    // --- FUNGSI STOP (Dipanggil onHoldEnd) ---
    public void StopMove()
    {
        currentMoveDirection = Vector3.zero;
        currentRotateDirection = 0f;
        currentJ4Direction = 0f;

        if(Teleop.isOn)
        {
            SendJointData();
        }
    }

    private void SendJointData()
    {
        if (jointControl == null || webSocketClient == null) return;

        // Memformat angka langsung menjadi string dengan 2 desimal
        // "F2" artinya Float dengan 2 angka di belakang koma
        string t1Str = jointControl.realTarget1.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string t2Str = jointControl.realTarget2.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string t3Str = jointControl.realTarget3.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string t4Str = playbackSystem.joint4.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        // Susun string JSON dengan tanda koma ( , ) asli sebagai pemisah objek yang sah
        string customJson = "{\"teleop\":{\"target1\":" + t1Str + ",\"target2\":" + t2Str + ",\"target3\":" + t3Str + ",\"target4\":" + t4Str + "}}";

        webSocketClient.SendPlayerData(customJson);
        Debug.Log("[JSON FIXED] " + customJson);
        j4DeltaAccumulated = 0f;
    }

    // --- INTEGRASI LISTENER ---

    public void SetTarget(Transform target)
    {
        ikTarget = target;
        Debug.Log("IK Target terhubung ke UI Mover");
    }

    public void MoveToPosition(Vector3 worldPosition)
    {
        if (ikTarget == null) return;

        // Langsung pindahkan posisi target ke koordinat yang diminta
        ikTarget.position = worldPosition;
    }
}