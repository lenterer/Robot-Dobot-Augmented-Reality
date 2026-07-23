using UnityEngine;
using UnityEngine.UI;

public class TeleopButton : MonoBehaviour
{
    [Header("Referensi UI")]
    public Image buttonImage; // Tarik komponen Image button ke sini
    
    [Header("Daftar Gambar")]
    public Sprite imageA; // Gambar saat On / Aktif
    public Sprite imageB; // Gambar saat Off / Mati

    public bool isOn = false;

    public void ToggleButton()
    {
        // Balik status boolean
        isOn = !isOn;

        if (isOn)
        {
            buttonImage.sprite = imageA;
            Debug.Log("Status: ON");
            // Panggil fungsi aktif di sini (misal: nyalakan Robot)
        }
        else
        {
            buttonImage.sprite = imageB;
            Debug.Log("Status: OFF");
            // Panggil fungsi mati di sini
        }
    }

    // Fungsi tambahan untuk mengecek status dari skrip lain
    public bool IsActive()
    {
        return isOn;
    }
}