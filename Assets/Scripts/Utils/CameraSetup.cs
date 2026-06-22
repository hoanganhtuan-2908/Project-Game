using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] private Transform focusTarget;
    [SerializeField] private float blackCameraYawOffset = 180f;

    private Vector3 whiteCameraPosition;
    private Quaternion whiteCameraRotation;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            whiteCameraPosition = mainCamera.transform.position;
            whiteCameraRotation = mainCamera.transform.rotation;

            // Automatically add FMOD Studio Listener if not present
            if (mainCamera.GetComponent<FMODUnity.StudioListener>() == null)
            {
                mainCamera.gameObject.AddComponent<FMODUnity.StudioListener>();
            }
        }
    }

    public void SetupCamera(TeamColor team)
    {
        if (mainCamera == null)
            return;

        mainCamera.transform.SetPositionAndRotation(whiteCameraPosition, whiteCameraRotation);

        if (team == TeamColor.Black)
            RotateCameraAroundBoard(blackCameraYawOffset);
    }

    private void RotateCameraAroundBoard(float yaw)
    {
        mainCamera.transform.RotateAround(GetFocusPosition(), Vector3.up, yaw);
    }

    private Vector3 GetFocusPosition()
    {
        if (focusTarget != null)
            return focusTarget.position;

        Board board = FindObjectOfType<Board>();
        if (board != null)
            return board.GetBoardCenter();

        return Vector3.zero;
    }
}
