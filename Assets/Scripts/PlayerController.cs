using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    InputManager inputManager;
    AnimatorManager animatorManager;
    Animator animator;
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

    [Header("Aiming")]
    public LayerMask aimLayerMask;
    public Transform debugTransform;
    public float movementRotationSpeed = 3.5f;
    public float aimingRotationSpeed = 20f;

    [Header("Animator")]
    public float aimingLayerBlendSpeed = 13f;

    [Header("Aim Sway")]
    public float aimSwayPositionAmount = 0.005f;
    public float aimSwayRotationAmount = 1.5f;
    public float aimSwaySmoothSpeed = 12f;

    public Transform aimTarget;

    [HideInInspector] public bool isAiming;
    [HideInInspector] public Vector3 aimWorldPosition;

    float cinemachineTargetYaw;
    float cinemachineTargetPitch;

    Quaternion targetRotation;
    Quaternion playerRotation;

    int aimingLayerIndex;

    Transform cachedRightHandTarget;
    Vector3 cachedRightHandLocalPosition;
    Quaternion cachedRightHandLocalRotation;

    const float threshold = 0.01f;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponent<AnimatorManager>();
        animator = GetComponent<Animator>();
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

        aimingLayerIndex = animator.GetLayerIndex("Aiming");

        if (aimingLayerIndex != -1)
        {
            animator.SetLayerWeight(aimingLayerIndex, 0f);
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
        HandleHandIK();
        HandleRotation();
        HandleAimSway();
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

        if (aimingLayerIndex != -1)
        {
            float targetWeight = isAiming ? 1f : 0f;
            float currentWeight = animator.GetLayerWeight(aimingLayerIndex);

            animator.SetLayerWeight(
                aimingLayerIndex,
                Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * aimingLayerBlendSpeed)
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
            animatorManager.rightHandIK.weight = 1;
            animatorManager.leftHandIK.weight = 1;
            return;
        }

        if (inputManager.verticalMovementInput != 0 || inputManager.horizontalMovementInput != 0)
        {
            animatorManager.rightHandIK.weight = 0;
            animatorManager.leftHandIK.weight = 0;
        }
        else
        {
            animatorManager.rightHandIK.weight = 1;
            animatorManager.leftHandIK.weight = 1;
        }
    }

    private void HandleRotation()
    {
        if (isAiming)
        {
            Vector3 worldAimTarget = aimWorldPosition;
            worldAimTarget.y = transform.position.y;

            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            if (aimDirection != Vector3.zero)
            {
                float angleToAim = Vector3.SignedAngle(transform.forward, aimDirection, Vector3.up);

                // let the spine/head rig handle small aim changes
                // only rotate the whole body when the angle gets bigger
                if (Mathf.Abs(angleToAim) > 25f)
                {
                    Quaternion aimRotation = Quaternion.LookRotation(aimDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        aimRotation,
                        Time.deltaTime * aimingRotationSpeed
                    );
                }
            }
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

    private void CacheRightHandTarget()
    {
        if (animatorManager == null || animatorManager.rightHandIK == null)
        {
            return;
        }

        Transform currentTarget = animatorManager.rightHandIK.data.target;

        if (currentTarget == null)
        {
            return;
        }

        if (cachedRightHandTarget != currentTarget)
        {
            cachedRightHandTarget = currentTarget;
            cachedRightHandLocalPosition = cachedRightHandTarget.localPosition;
            cachedRightHandLocalRotation = cachedRightHandTarget.localRotation;
        }
    }

    private void HandleAimSway()
    {
        CacheRightHandTarget();

        if (cachedRightHandTarget == null)
        {
            return;
        }

        Vector3 targetLocalPosition = cachedRightHandLocalPosition;
        Quaternion targetLocalRotation = cachedRightHandLocalRotation;

        if (isAiming)
        {
            float lookX = inputManager.horizontalCameraInput;
            float lookY = inputManager.verticalCameraInput;

            float swayPosX = Mathf.Clamp(-lookX * aimSwayPositionAmount, -aimSwayPositionAmount, aimSwayPositionAmount);
            float swayPosY = Mathf.Clamp(-lookY * aimSwayPositionAmount, -aimSwayPositionAmount, aimSwayPositionAmount);

            float swayRotY = Mathf.Clamp(-lookX * aimSwayRotationAmount, -aimSwayRotationAmount, aimSwayRotationAmount);
            float swayRotX = Mathf.Clamp(lookY * aimSwayRotationAmount, -aimSwayRotationAmount, aimSwayRotationAmount);

            targetLocalPosition += new Vector3(swayPosX, swayPosY, 0f);
            targetLocalRotation *= Quaternion.Euler(swayRotX, swayRotY, 0f);
        }

        cachedRightHandTarget.localPosition = Vector3.Lerp(
            cachedRightHandTarget.localPosition,
            targetLocalPosition,
            Time.deltaTime * aimSwaySmoothSpeed
        );

        cachedRightHandTarget.localRotation = Quaternion.Slerp(
            cachedRightHandTarget.localRotation,
            targetLocalRotation,
            Time.deltaTime * aimSwaySmoothSpeed
        );
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