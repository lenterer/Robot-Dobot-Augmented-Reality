using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Waypoint
{
    public Vector3 visualPosition;
    public Vector3 robotPosition;
    public float r; // rotasi sederhana / radius

    // Tambahan joint
    public float joint1;
    public float joint2;
    public float joint3;

    public bool suctionOn;
}

public class TeachingPlayback : MonoBehaviour
{
    [Header("Data")]
    public List<Waypoint> waypoints = new List<Waypoint>();

    [Header("Debug Position")]
    public Vector3 debugPosition;

    [Header("Visual")]
    public GameObject markerPrefab;
    public Transform markerParent;
    public LineRenderer lineRenderer;

    [Header("Suction")]
    public bool currentSuctionState = false;

    [Header("UI Suction")]
    public TMP_Text suctionText;

    [HideInInspector]
    public Transform targetSource1;
    [HideInInspector]
    public Transform targetSource2;
    [HideInInspector]
    public Transform robotBase;
    [HideInInspector]
    public SimpleJointController jointController;
    public IKButtonController ikController;
    public WebSocketClient wsClient;

    private List<GameObject> markers = new List<GameObject>();
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // =========================
    // ADD WAYPOINT (DARI BUTTON)
    // =========================
    public void AddWaypointFromTarget()
    {
       if (targetSource1 == null || targetSource2 == null || robotBase == null)
        {
            Debug.LogWarning("Referensi belum lengkap!");
            return;
        }

        Vector3 visualRel = robotBase.InverseTransformPoint(targetSource1.position);
        Vector3 robotRel  = robotBase.InverseTransformPoint(targetSource2.position);

        float j1 = jointController.targetAngles[0];
        float j2 = jointController.targetAngles[1];
        float j3 = jointController.elbowGlobalLock;

        AddWaypoint(visualRel, robotRel, j1, j2, j3);
    }

    void Start()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }

    // =========================
    // ADD WAYPOINT (MANUAL)
    // =========================
    public void AddWaypoint(Vector3 visualPos, Vector3 robotPos, float j1, float j2, float j3)
    {
        Waypoint wp = new Waypoint();
        wp.visualPosition = visualPos;
        wp.robotPosition = robotPos;
        wp.r = 0f;

        // simpan joint
        wp.joint1 = Mathf.Round((j1 - 45f) * 100f) / 100f;
        wp.joint2 = Mathf.Round(j2 * 100f) / 100f;
        wp.joint3 = Mathf.Round((j3 + 5f) * 100f) / 100f;

        wp.suctionOn = currentSuctionState;

        waypoints.Add(wp);

        Vector3 worldVisual = robotBase.TransformPoint(visualPos);
        GameObject marker = Instantiate(markerPrefab, worldVisual, Quaternion.identity, markerParent);
        markers.Add(marker);

        UpdateLine();

        Debug.Log($"Waypoint {waypoints.Count} ditambahkan | Joints: {j1}, {j2}, {j3}");
    }

    public void ToggleSuction()
    {
        // balik state
        currentSuctionState = !currentSuctionState;

        // update tulisan
        if (suctionText != null)
        {
            suctionText.text = currentSuctionState ? "Suction Cup: On" : "Suction Cup: Off";
        }

        Debug.Log("Suction: " + (currentSuctionState ? "ON" : "OFF"));
        AddWaypointFromTarget();
    }

    // =========================
    // DELETE LAST (UNDO)
    // =========================
    public void DeleteLastWaypoint()
    {
        if (waypoints.Count == 0)
        {
            Debug.Log("Tidak ada waypoint!");
            return;
        }

        // hapus data
        waypoints.RemoveAt(waypoints.Count - 1);

        // hapus marker
        Destroy(markers[markers.Count - 1]);
        markers.RemoveAt(markers.Count - 1);

        // update garis
        UpdateLine();

        Debug.Log("Waypoint dihapus. Sisa: " + waypoints.Count);
    }

    // =========================
    // CLEAR SEMUA
    // =========================
    public void ClearAllWaypoints()
    {
        waypoints.Clear();

        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
        markers.Clear();

        UpdateLine();

        Debug.Log("Semua waypoint dihapus");
    }

    public void PlayWaypoints()
    {
        if (ikController == null)
        {
            Debug.LogWarning("IK Controller belum di-assign!");
            return;
        }

        Debug.Log("[PLAY] robotBase: " + robotBase.position);
        StartCoroutine(PlaybackCoroutine());
    }

    IEnumerator PlaybackCoroutine()
    {
        float speed = 0.1f; // meter per detik

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 startPos = targetSource2.position;
            Vector3 targetPos = robotBase.TransformPoint(waypoints[i].robotPosition);

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = Mathf.Max(distance / speed, 0.0001f);

            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;

                Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
                ikController.MoveToPosition(pos);

                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    // =========================
    // UPDATE LINE RENDERER
    // =========================
    void UpdateLine()
    {
        if (lineRenderer == null)
            return;

        if (waypoints.Count < 2)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = waypoints.Count;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 worldPos = robotBase.TransformPoint(waypoints[i].visualPosition);
            lineRenderer.SetPosition(i, worldPos);
        }
    }

    // =========================
    // DEBUG (OPTIONAL)
    // =========================
    public void PrintWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            Debug.Log($"Point {i} VISUAL: {waypoints[i].visualPosition}");
            Debug.Log($"Point {i} ROBOT : {waypoints[i].robotPosition}");
        }
    }

    public void SetTarget(Transform target1, Transform target2, Transform target3, SimpleJointController target4)
    {
        targetSource1 = target1;
        targetSource2 = target2;
        robotBase = target3;
        jointController = target4;
        Debug.Log("[WaypointManager] Target diterima");
    }

    // KIRIM KE WEBSOCKET
    // public void SendWaypointToWebSocket()
    // {
    //     if (wsClient == null)
    //     {
    //         Debug.LogWarning("WebSocket belum di-assign!");
    //         return;
    //     }

    //     if (waypoints.Count == 0)
    //     {
    //         Debug.LogWarning("Waypoint kosong!");
    //         return;
    //     }

    //     wsClient.SendWaypointList(waypoints);
    // }
}