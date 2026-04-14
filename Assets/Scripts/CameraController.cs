using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public InputManager inputManager;
    public PlayerController playerController;

    [Header("Target Offset")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    [Header("Camera Look")]
    public float normalLookSpeed = 120f;
    public float aimLookSpeed = 80f;
    public float minimumPivot = -35f;
    public float maximumPivot = 60f;

    float lookAngle;
    float pivotAngle;
    Vector3 cameraRotation;

    private void Start()
    {
        if (player != null)
        {
            lookAngle = player.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (player == null || inputManager == null)
        {
            return;
        }

        transform.position = player.position + targetOffset;

        float currentLookSpeed = normalLookSpeed;

        if (playerController != null && playerController.isAiming)
        {
            currentLookSpeed = aimLookSpeed;
        }

        lookAngle += inputManager.horizontalCameraInput * currentLookSpeed * Time.deltaTime;
        pivotAngle -= inputManager.verticalCameraInput * currentLookSpeed * Time.deltaTime;
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

        cameraRotation = Vector3.zero;
        cameraRotation.x = pivotAngle;
        cameraRotation.y = lookAngle;

        transform.rotation = Quaternion.Euler(cameraRotation);
    }
}
