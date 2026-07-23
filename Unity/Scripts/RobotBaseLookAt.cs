using UnityEngine;

public class RobotBaseLookAt : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.magnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);

            // offset 90 derajat
            rot *= Quaternion.Euler(0, -90, 0);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                Time.deltaTime * 5f
            );
        }
    }
}