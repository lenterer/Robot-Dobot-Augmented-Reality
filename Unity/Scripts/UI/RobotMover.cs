using UnityEngine;

public class RobotMover : MonoBehaviour
{
    Transform currentRobot;
    ArticulationBody rootBody;

    [Header("Speed")]
    public float moveSpeed = 0.2f;   // meter per detik
    public float rotateSpeed = 60f;  // degree per detik

    Vector3 currentMoveDirection = Vector3.zero;
    float currentRotateDirection = 0f;

    public void SetRobot(Transform robot)
    {
        currentRobot = robot;

        rootBody = currentRobot.GetComponent<ArticulationBody>();

        if (rootBody != null)
        {
            Debug.Log("Root Articulation ditemukan: " + rootBody.name);
        }
        else
        {
            Debug.LogError("ArticulationBody tidak ditemukan di root!");
        }
    }

    void Update()
    {
        if (rootBody == null) return;

        Vector3 newPosition = rootBody.transform.position;
        Quaternion newRotation = rootBody.transform.rotation;

        // =========================
        // MOVE
        // =========================
        if (currentMoveDirection != Vector3.zero)
        {
            Vector3 localMoveVector = rootBody.transform.TransformDirection(currentMoveDirection);

            newPosition +=
                localMoveVector *
                moveSpeed *
                Time.deltaTime;
        }

        // =========================
        // ROTATE
        // =========================
        if (currentRotateDirection != 0f)
        {
            newRotation *= Quaternion.Euler(
                0,
                currentRotateDirection *
                rotateSpeed *
                Time.deltaTime,
                0
            );
        }

        rootBody.TeleportRoot(newPosition, newRotation);
    }

    // =========================
    // START MOVE (HOLD)
    // =========================

    public void StartMoveRight()
    {
        currentMoveDirection = Vector3.right;
    }

    public void StartMoveLeft()
    {
        currentMoveDirection = Vector3.left;
    }

    public void StartMoveForward()
    {
        currentMoveDirection = Vector3.forward;
    }

    public void StartMoveBackward()
    {
        currentMoveDirection = Vector3.back;
    }

    public void StartMoveUp()
    {
        currentMoveDirection = Vector3.up;
    }

    public void StartMoveDown()
    {
        currentMoveDirection = Vector3.down;
    }

    // =========================
    // START ROTATE
    // =========================

    public void StartRotateRight()
    {
        currentRotateDirection = 1f;
    }

    public void StartRotateLeft()
    {
        currentRotateDirection = -1f;
    }

    // =========================
    // STOP
    // =========================

    public void StopMove()
    {
        currentMoveDirection = Vector3.zero;
        currentRotateDirection = 0f;
    }
}