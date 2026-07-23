using System;
using UnityEngine;
using NativeWebSocket;
using System.Collections.Generic;
using System.Globalization;

public class WebSocketClient : MonoBehaviour
{
    [Header("Reference UI")]
    public RobotTransformInfo_2 statusUI;

    private WebSocket websocket;

    private List<string> serverIPs = new List<string>()
    {
        "ws://192.168.200.147:8765", // utama
        "ws://192.168.137.1:8765",   // cadangan
        "ws://localhost:8765",
    };

    async void Awake()
    {
        // 🔥 DEBUG AWAL
        if (statusUI != null)
        {
            Debug.Log("[WebSocket] statusUI terhubung via Inspector ✅");
        }
        else
        {
            Debug.LogError("[WebSocket] statusUI BELUM di-drag ❌");
        }

        await ConnectToAvailableServer();
    }

    async System.Threading.Tasks.Task ConnectToAvailableServer()
    {
        foreach (string ip in serverIPs)
        {
            Debug.Log("[WebSocket] Coba connect ke: " + ip);

            websocket = new WebSocket(ip);

            bool connected = false;

            websocket.OnOpen += () =>
            {
                Debug.Log("[WebSocket] Connected ke: " + ip);
                connected = true;

                if (statusUI != null)
                {
                    statusUI.SetConnected();
                }
            };

            websocket.OnError += (e) =>
            {
                Debug.Log("[WebSocket] Error: " + e);
                if (statusUI != null)
                {
                    statusUI.SetDisconnected();
                }
            };

            websocket.OnClose += (e) =>
            {
                Debug.Log("[WebSocket] Closed");
                if (statusUI != null)
                {
                    statusUI.SetDisconnected();
                }
            };

            websocket.OnMessage += (bytes) =>
            {
                string msg = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("[WebSocket] Recv: " + msg);
            };

            await websocket.Connect();

            // ⏳ tunggu cek berhasil atau tidak
            float timeout = 3f;
            float timer = 0f;

            while (!connected && timer < timeout)
            {
                await System.Threading.Tasks.Task.Delay(100);
                timer += 0.1f;
            }

            if (connected)
            {
                return; // stop kalau berhasil
            }
            else
            {
                Debug.LogWarning("[WebSocket] Gagal: " + ip);
            }
        }

        // semua gagal
        Debug.LogError("[WebSocket] Semua IP gagal!");
    }

    public async void Reconnect()
    {
        // Cek apakah saat ini sedang terhubung atau sedang mencoba connect
        if (websocket != null && (websocket.State == WebSocketState.Open || websocket.State == WebSocketState.Connecting))
        {
            Debug.LogWarning("[WebSocket] Sudah terhubung atau sedang mencoba menyambung. Reconnect dibatalkan.");
            return;
        }

        Debug.Log("[WebSocket] Mencoba menghubungkan kembali secara manual via tombol...");
        
        // Tutup koneksi lama jika ada sisa instance sebelum membuat yang baru
        if (websocket != null)
        {
            await websocket.Close();
        }

        // Panggil fungsi pencarian IP server kembali
        await ConnectToAvailableServer();
    }

    public async void SendDelta(float dj1, float dj2, float dj3)
    {
        if (websocket.State == WebSocketState.Open)
        {
            string json = "{\"dj1\":" + dj1 + ",\"dj2\":" + dj2 + ",\"dj3\":" + dj3 + "}";
            await websocket.SendText(json);
        }
    }

    public async void SendWaypointList(List<Waypoint_2> waypoints)
    {
        string json = "{ \"waypoints\": [";

        for (int i = 0; i < waypoints.Count; i++)
        {
            Waypoint_2 wp = waypoints[i];

            json += $"{{\"j1\":{wp.joint1.ToString(CultureInfo.InvariantCulture)},"
            + $"\"j2\":{wp.joint2.ToString(CultureInfo.InvariantCulture)},"
            + $"\"j3\":{wp.joint3.ToString(CultureInfo.InvariantCulture)},"
            + $"\"j4\":{wp.r.ToString(CultureInfo.InvariantCulture)},"
            + $"\"suction\":{(wp.suctionOn ? "true" : "false")}}}";

            if (i < waypoints.Count - 1)
                json += ",";
        }

        json += "]}";
        Debug.Log(json);

        if (websocket.State != WebSocketState.Open)
        {
            Debug.LogWarning("WebSocket belum connect!");
            return;
        }


        await websocket.SendText(json);

        Debug.Log("WebSocket: waypoint list dikirim");
    }

    public async void SendPlayerData(string jsonContent)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(jsonContent);
        }
        else
        {
            Debug.LogWarning("Gagal mengirim data: WebSocket tidak aktif atau terputus.");
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}