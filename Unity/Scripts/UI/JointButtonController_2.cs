using UnityEngine;
using System;

// Struktur data untuk JSON
[Serializable]
public class JointData
{
    public float target1;
    public float target2;
    public float target3;
}

public class JointButtonController_2 : MonoBehaviour
{
    public MultiJointControl jointControl;
    public WebSocketClient webSocketClient;
    public TeleopButton Teleop;
    public TeachingPlayback_2 playbackSystem;

    [Header("Movement Settings")]
    public float rotateSpeed = 30f; // Derajat per detik saat ditahan

    private int activeJoint = 0;      // 1, 2, atau 3
    private float currentDirection = 0f; // 1 untuk plus, -1 untuk minus
    private bool isHolding = false;
    private float j4DeltaAccumulated = 0f;

    void Update()
    {
        if (!isHolding || jointControl == null) return;

        float moveAmount = currentDirection * rotateSpeed * Time.deltaTime;

        // Eksekusi pergerakan berdasarkan joint yang dipilih
        switch (activeJoint)
        {
            case 1:
                jointControl.target1 += moveAmount;
                jointControl.target1 = Mathf.Clamp(jointControl.target1, -80f, 80f);
                break;
            case 2:
                jointControl.target2 += moveAmount;
                jointControl.target2 = Mathf.Clamp(jointControl.target2, 0f, 55f);
                break;
            case 3:
                // Menggunakan SetTarget sesuai struktur aslimu untuk Joint 3
                float minT3 = 0f - jointControl.target2;
                float maxT3 = 70f - jointControl.target2;

                float newTarget3 = jointControl.target3 + moveAmount;
                newTarget3 = Mathf.Clamp(newTarget3, minT3, maxT3);

                // PERBAIKAN: Masukkan variabel 'newTarget3' yang sudah aman ke fungsi SetTarget
                jointControl.target3 = newTarget3; // Perbarui variabel lokal di Unity
                jointControl.SetTarget(2, newTarget3);
                break;
            case 4:
                j4DeltaAccumulated += moveAmount;
                j4DeltaAccumulated = Mathf.Clamp(j4DeltaAccumulated, -140, 140);
                playbackSystem.joint4 += moveAmount;
                playbackSystem.joint4 = Mathf.Clamp(playbackSystem.joint4, -140, 140);
                break;
        }
    }

    // --- FUNGSI START (Dipanggil oleh onHoldStart) ---

    public void StartJ1_Plus()  { SetMove(1, 1f); }
    public void StartJ1_Minus() { SetMove(1, -1f); }
    
    public void StartJ2_Plus()  { SetMove(2, 1f); }
    public void StartJ2_Minus() { SetMove(2, -1f); }
    
    public void StartJ3_Plus()  { SetMove(3, 1f); }
    public void StartJ3_Minus() { SetMove(3, -1f); }

    public void StartJ4_Plus()  { SetMove(4, 1f); }
    public void StartJ4_Minus() { SetMove(4, -1f); }

    private void SetMove(int jointIndex, float direction)
    {
        activeJoint = jointIndex;
        currentDirection = direction;
        isHolding = true;

        if (jointIndex == 4) j4DeltaAccumulated = 0f;
    }

    // --- FUNGSI STOP (Dipanggil oleh onHoldEnd) ---

    public void StopMoving()
    {
        isHolding = false;
        activeJoint = 0;
        currentDirection = 0f;

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
        string t3Str = jointControl.realTarget3FK.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string t4Str = playbackSystem.joint4.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        // Susun string JSON dengan tanda koma ( , ) asli sebagai pemisah objek yang sah
        string customJson = "{\"teleop\":{\"target1\":" + t1Str + ",\"target2\":" + t2Str + ",\"target3\":" + t3Str + ",\"target4\":" + t4Str + "}}";

        webSocketClient.SendPlayerData(customJson);
        Debug.Log("[JSON FIXED] " + customJson);
        j4DeltaAccumulated = 0f;
    }

    // 🔥 Dipanggil dari listener
    public void SetController(MultiJointControl controller)
    {
        jointControl = controller;
        Debug.Log("JointController terhubung ke UI (Hold Mode)");
    }
}