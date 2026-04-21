using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    InputManager inputManager;
    AnimatorManager animatorManager;
    ActiveWeapon activeWeapon;

    public Camera gameplayCamera;
    public Transform cinemachineCameraTarget;
    public GameObject crossHair;

    [Header("Cameras")]
    public CinemachineCamera normalCamera;
    public CinemachineCamera aimCamera;

    public bool isGrabbed;
    private Transform grabTarget;

    [Header("Cinemachine")]
    public float topClamp = 70f;
    public float bottomClamp = -30f;
    public float cameraAngleOverride = 0f;
    public bool lockCameraPosition = false;
    public float lookSpeed = 1f;

    [Header("Aiming")]
    public LayerMask aimLayerMask;
    public Transform aimTarget;
    public Transform debugTransform;
    public float movementRotationSpeed = 3.5f;
    public float aimingRotationSpeed = 20f;

    [HideInInspector] public bool isAiming;
    [HideInInspector] public Vector3 aimWorldPosition;

    float cinemachineTargetYaw;
    float cinemachineTargetPitch;

    Quaternion targetRotation;
    Quaternion playerRotation;

    const float threshold = 0.01f;

    void Awake()
    {
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponent<AnimatorManager>();
        activeWeapon = GetComponentInChildren<ActiveWeapon>();
       

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;
    }

    void Start()
    {
        if (cinemachineCameraTarget != null)
        {
            cinemachineTargetYaw = cinemachineCameraTarget.rotation.eulerAngles.y;
            cinemachineTargetPitch = 0f;
        }
    }

    void Update()
    {
        inputManager.HandleAllInputs();

        HandleCameraRotation();
        HandleAimRaycast();
        HandleShooting();
        HandleAiming();
        HandleCameraState();
        HandleAnimatorValues();
        HandleRotation();
    }

    void HandleCameraRotation()
    {
        if (cinemachineCameraTarget == null || lockCameraPosition)
            return;

        Vector2 lookInput = new Vector2(inputManager.horizontalCameraInput, inputManager.verticalCameraInput);

        if (lookInput.sqrMagnitude >= threshold)
        {
            cinemachineTargetYaw += inputManager.horizontalCameraInput * lookSpeed;
            cinemachineTargetPitch -= inputManager.verticalCameraInput * lookSpeed;
        }

        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        cinemachineCameraTarget.rotation = Quaternion.Euler(
            cinemachineTargetPitch + cameraAngleOverride,
            cinemachineTargetYaw,
            0f
        );
    }

    void HandleAimRaycast()
    {
        Ray ray = new Ray(gameplayCamera.transform.position, gameplayCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            aimWorldPosition = raycastHit.point;
        else
            aimWorldPosition = ray.GetPoint(50f);

        if (debugTransform != null)
            debugTransform.position = aimWorldPosition;

        if (aimTarget != null)
            aimTarget.position = aimWorldPosition;
    }

    void HandleAiming()
    {
        bool canAim = inputManager.aimingInput;

        if (activeWeapon != null && activeWeapon.IsHolstered)
        {
            canAim = false;
        }

        isAiming = canAim;

        if (activeWeapon != null)
            activeWeapon.SetAiming(isAiming);

        if (crossHair != null)
            crossHair.SetActive(isAiming);
    }

    void HandleCameraState()
    {
        if (normalCamera == null || aimCamera == null)
            return;

        if (isAiming)
        {
            aimCamera.Priority = 20;
            normalCamera.Priority = 10;
        }
        else
        {
            aimCamera.Priority = 10;
            normalCamera.Priority = 20;
        }
    }

    void HandleAnimatorValues()
    {
        animatorManager.HandleAnimatorValues(
            inputManager.horizontalMovementInput,
            inputManager.verticalMovementInput,
            inputManager.runInput
        );
    }

    void HandleRotation()
    {
        if (isAiming)
        {
            float yaw = gameplayCamera.transform.eulerAngles.y;
            Quaternion aimRotation = Quaternion.Euler(0f, yaw, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                aimRotation,
                Time.deltaTime * aimingRotationSpeed
            );
        }
        else
        {
            if (cinemachineCameraTarget == null)
                return;

            targetRotation = Quaternion.Euler(0f, cinemachineCameraTarget.eulerAngles.y, 0f);
            playerRotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                movementRotationSpeed * Time.deltaTime
            );

            if (inputManager.verticalMovementInput != 0 || inputManager.horizontalMovementInput != 0)
                transform.rotation = playerRotation;
        }
    }

    void HandleShooting()
    {
        if (activeWeapon == null)
        {
            return;
        }

        activeWeapon.SetFiringInput(
            isAiming,
            inputManager.shootInput,
            inputManager.shootPressedInput
        );

        if (inputManager.reloadPressedInput)
        {
            activeWeapon.TryReload();
        }

        if (inputManager.holsterPressedInput)
        {
            activeWeapon.ToggleHolster();
        }
    }


    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    public void BeginGrab(Transform attachPoint)
    {
        isGrabbed = true;
        grabTarget = attachPoint;

        if (inputManager != null)
            inputManager.inputLocked = true;

        if (activeWeapon != null)
        {
            activeWeapon.SetAiming(false);
            activeWeapon.SetFiringInput(false, false, false);
        }

        if (crossHair != null)
            crossHair.SetActive(false);
    }

    public void EndGrab()
    {
        isGrabbed = false;
        grabTarget = null;

        if (inputManager != null)
            inputManager.inputLocked = false;
    }
}