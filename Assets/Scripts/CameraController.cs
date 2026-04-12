using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public InputManager inputManager;
    PlayerManager playerManager;

    public Transform cameraPivot;
    public Camera cameraObject;


    public GameObject player;
    public Transform aimedCameraPosition;

    Vector3 cameraFollowVelocity = Vector3.zero;
    Vector3 targetPosition;
    Vector3 cameraRotation;
    Quaternion targetRotation;

    public float cameraSmoothTime = 0.2f;

    float lookAmountVertical;
    public float lookAmountHorizontal;
    float maximumPivotAngle = 15;
    float minimumPivotAngle = -15;

    private void Awake()
    {
        inputManager = player.GetComponent<InputManager>();
        playerManager = player.GetComponent<PlayerManager>();
    }
    public void HandleAllCameraMovement()
    {
        FollowPlayer();
        RotateCamera();
    }

    private void FollowPlayer()
    {
        if(playerManager.isAiming)
        {
            targetPosition = Vector3.SmoothDamp(transform.position, aimedCameraPosition.transform.position, ref cameraFollowVelocity, cameraSmoothTime * Time.deltaTime);
            transform.position = targetPosition;
        }
        else
        {
            targetPosition = Vector3.SmoothDamp(transform.position, player.transform.position, ref cameraFollowVelocity, cameraSmoothTime * Time.deltaTime);
            transform.position = targetPosition;
        }


    }

    private void RotateCamera()
    {
        lookAmountVertical = lookAmountVertical + (inputManager.horizontalCameraInput);
        lookAmountHorizontal = lookAmountHorizontal - (inputManager.verticalCameraInput);
        lookAmountHorizontal = Mathf.Clamp(lookAmountHorizontal, minimumPivotAngle, maximumPivotAngle);

        cameraRotation = Vector3.zero;
        cameraRotation.y = lookAmountVertical;
        targetRotation = Quaternion.Euler(cameraRotation);
        targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, cameraSmoothTime);
        transform.rotation = targetRotation;

        cameraRotation = Vector3.zero;
        cameraRotation.x = lookAmountHorizontal;
        targetRotation = Quaternion.Euler(cameraRotation);
        targetRotation = Quaternion.Slerp(cameraPivot.localRotation, targetRotation, cameraSmoothTime);
        cameraPivot.localRotation = targetRotation;
    }
        
}

