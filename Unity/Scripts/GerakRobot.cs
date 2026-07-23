using UnityEngine;
using System.Collections.Generic;

public class GerakRobot : MonoBehaviour
{
    [Header("Konfigurasi Robot")]
    [Tooltip("Masukkan semua ArticulationBody yang ingin digerakkan di sini")]
    public List<ArticulationBody> joints;

    [Header("Pengaturan Gerak")]
    public float speed = 300.0f; // Kecepatan putaran

    private int currentJointIndex = 0; // Indeks sendi yang sedang aktif

    void Update()
    {
        // 1. Ganti Sendi Aktif dengan tombol TAB
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchJoint();
        }

        // 2. Baca Input Gerak (Panah Kiri/Kanan atau A/D)
        float moveInput = Input.GetAxis("Horizontal");

        // Gunakan threshold kecil agar tidak bergerak sendiri (deadzone)
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            MoveCurrentJoint(moveInput);
        }
    }

    void SwitchJoint()
    {
        if (joints.Count == 0) return;
        
        currentJointIndex++;
        if (currentJointIndex >= joints.Count)
        {
            currentJointIndex = 0; // Kembali ke awal jika sudah di sendi terakhir
        }
    }

    void MoveCurrentJoint(float direction)
    {
        if (joints.Count == 0) return;

        // Ambil joint yang sedang aktif
        ArticulationBody body = joints[currentJointIndex];

        // Ambil konfigurasi xDrive saat ini (copy struct)
        var drive = body.xDrive;

        // Hitung target posisi baru
        float currentTarget = drive.target;
        float newTarget = currentTarget + (direction * speed * Time.deltaTime);

        // --- PERBAIKAN DI SINI ---
        // Kita langsung clamp saja nilainya. 
        // Jika robot di-import dari URDF, lowerLimit & upperLimit pasti sudah terisi otomatis.
        // Jika sendi tipe 'Continuous' (bebas), limit biasanya diset Infinity, tapi untuk Dobot biasanya 'Revolute' (terbatas).
        
        if (drive.lowerLimit < drive.upperLimit) // Cek sederhana untuk memastikan limit valid
        {
             newTarget = Mathf.Clamp(newTarget, drive.lowerLimit, drive.upperLimit);
        }
        
        // Terapkan target baru
        drive.target = newTarget;
        body.xDrive = drive;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        // Tambahkan background hitam transparan agar tulisan terbaca
        GUI.Box(new Rect(10, 10, 450, 100), ""); 

        if (joints.Count > 0)
        {
            // Cek index agar tidak error jika list berubah
            if(currentJointIndex < joints.Count) 
            {
                string jointName = joints[currentJointIndex].name;
                GUI.Label(new Rect(20, 20, 400, 50), "Sendi Aktif: " + jointName + " (Tekan TAB)", style);
                GUI.Label(new Rect(20, 50, 400, 50), "Gerak: Panah Kiri / Kanan", style);
            }
        }
        else
        {
            GUI.Label(new Rect(20, 20, 400, 50), "List 'Joints' masih kosong di Inspector!", style);
        }
    }
}