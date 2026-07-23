using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageListener : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    public MoveRobotController2 robotController;

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // ================= ADDED =================
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log("🟢 [ADDED] Image detected: " + trackedImage.referenceImage.name);
            AssignObject(trackedImage);
        }

        // ================= UPDATED =================
        foreach (var trackedImage in eventArgs.updated)
        {
            Debug.Log("🟡 [UPDATED] Image: " + trackedImage.referenceImage.name +
                      " | Tracking: " + trackedImage.trackingState);

            AssignObject(trackedImage);
        }

        // ================= REMOVED =================
        foreach (var trackedImage in eventArgs.removed)
        {
            Debug.Log("🔴 [REMOVED] Image lost: " + trackedImage.referenceImage.name);

            if (robotController != null)
            {
                robotController.robotBase = null;
            }
        }
    }

    void AssignObject(ARTrackedImage trackedImage)
    {
        Transform parent = trackedImage.transform.parent;

        if (robotController != null)
        {
            robotController.robotBase = parent;
        }
    }
}