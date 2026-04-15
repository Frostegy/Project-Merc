using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    InputManager inputManager;
    AnimatorManager animatorManager;
    PlayerEquipmentManager playerEquipmentManager;

    public Camera gameplayCamera;
    public Transform cinemachineCameraTarget;
    public GameObject crossHair;

    [Header("Cameras")]
    public CinemachineCamera normalCamera;
    public CinemachineCamera aimCamera;

    [Header("Cinemachine")]
    public float topClamp = 70f;
    public float bottomClamp = -30f;
    public float cameraAngleOverride = 0f;
    public bool lockCameraPosition = false;
    public float lookSpeed = 1f;

    [Header("Rig Aiming")]
    public Rig aimRig;
    public Transform aimTarget;
    public float aimDuration = 0.3f;

    [Header("Aiming")]
    public LayerMask aimLayerMask;
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

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponent<AnimatorManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();

        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (cinemachineCameraTarget != null)
        {
            cinemachineTargetYaw = cinemachineCameraTarget.rotation.eulerAngles.y;
            cinemachineTargetPitch = 0f;
        }

        if (aimRig != null)
        {
            aimRig.weight = 0f;
        }
    }

    private void Update()
    {
        inputManager.HandleAllInputs();

        HandleCameraRotation();
        HandleAimRaycast();
        HandleAiming();
        HandleCameraState();
        HandleAnimatorValues();
        //HandleHandIK();
        HandleRotation();
        HandleShooting();
    }

    private void HandleCameraRotation()
    {
        if (cinemachineCameraTarget == null || lockCameraPosition)
        {
            return;
        }

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

    private void HandleAimRaycast()
    {
        Vector2 screenCentrePoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = gameplayCamera.ScreenPointToRay(screenCentrePoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
        {
            aimWorldPosition = raycastHit.point;
        }
        else
        {
            aimWorldPosition = ray.GetPoint(50f);
        }

        if (debugTransform != null)
        {
            debugTransform.position = aimWorldPosition;
        }

        if (aimTarget != null)
        {
            aimTarget.position = aimWorldPosition;
        }
    }

    private void HandleAiming()
    {
        isAiming = inputManager.aimingInput;

        if (aimRig != null)
        {
            float targetWeight = isAiming ? 1f : 0f;
            float blendSpeed = aimDuration <= 0f ? 999f : 1f / aimDuration;

            aimRig.weight = Mathf.MoveTowards(
                aimRig.weight,
                targetWeight,
                Time.deltaTime * blendSpeed
            );
        }

        if (crossHair != null)
        {
            crossHair.SetActive(isAiming);
        }
    }

    private void HandleCameraState()
    {
        if (normalCamera == null || aimCamera == null)
        {
            return;
        }

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

    private void HandleAnimatorValues()
    {
        animatorManager.HandleAnimatorValues(
            inputManager.horizontalMovementInput,
            inputManager.verticalMovementInput,
            inputManager.runInput
        );
    }

    private void HandleHandIK()
    {
        if (animatorManager == null || animatorManager.rightHandIK == null || animatorManager.leftHandIK == null)
        {
            return;
        }

        if (isAiming)
        {
            animatorManager.rightHandIK.weight = 1f;
            animatorManager.leftHandIK.weight = 1f;
            return;
        }

        if (inputManager.verticalMovementInput != 0 || inputManager.horizontalMovementInput != 0)
        {
            animatorManager.rightHandIK.weight = 0f;
            animatorManager.leftHandIK.weight = 0f;
        }
        else
        {
            animatorManager.rightHandIK.weight = 1f;
            animatorManager.leftHandIK.weight = 1f;
        }
    }

    private void HandleRotation()
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
            {
                return;
            }

            targetRotation = Quaternion.Euler(0f, cinemachineCameraTarget.eulerAngles.y, 0f);
            playerRotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                movementRotationSpeed * Time.deltaTime
            );

            if (inputManager.verticalMovementInput != 0 || inputManager.horizontalMovementInput != 0)
            {
                transform.rotation = playerRotation;
            }
        }
    }

    private void HandleShooting()
    {
        if (inputManager.shootInput && isAiming)
        {
            inputManager.shootInput = false;

            if (playerEquipmentManager.weaponAnimator != null)
            {
                playerEquipmentManager.weaponAnimator.ShootWeapon(gameplayCamera, aimWorldPosition);
            }
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }

        if (angle > 360f)
        {
            angle -= 360f;
        }

        return Mathf.Clamp(angle, min, max);
    }
}