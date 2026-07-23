using UnityEngine;

public class MeasureLinks : MonoBehaviour
{
    public Transform link0;   // base
    public Transform link1;   // shoulder
    public Transform link2;   // elbow
    public Transform link3;   // end-effector

    void Start()
    {
        float L1 = Vector3.Distance(link0.position, link1.position);
        float L2 = Vector3.Distance(link1.position, link2.position);
        float L3 = Vector3.Distance(link2.position, link3.position);

        Debug.Log($"L1 = {L1} m");
        Debug.Log($"L2 = {L2} m");
        Debug.Log($"L3 = {L3} m");
    }
}
