using UnityEngine;

public class RobotReferenceFinder : MonoBehaviour
{
    public RobotTransformInfo infoScript;
    public string targetPartName = "robot_base"; // nama bagian robot

    void Start()
    {
        Transform part = transform.Find(targetPartName);

        if (part != null && infoScript != null)
        {
            infoScript.targetPart = part;
        }
    }
}