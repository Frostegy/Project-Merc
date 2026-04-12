using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    CameraController playerCamera;
    InputManager inputManager;
    Animator animator;
    PlayerLocomotionManager playerLocomotionManager;
    PlayerEquipmentManager playerEquipmentManager;

    public bool isAiming;

    private void Awake()
    {
        playerCamera = FindAnyObjectByType<CameraController>();
        inputManager = GetComponent<InputManager>();
        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        animator = GetComponent<Animator>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
    }

    private void Update()
    {
        inputManager.HandleAllInputs();

        isAiming = animator.GetBool("isAiming");

    }
    private void FixedUpdate()
    {
        playerLocomotionManager.HandleAllLocomotion();
    }

    private void LateUpdate()
    {
        playerCamera.HandleAllCameraMovement();
    }

    public void UseCurrentWeapon()
    {
        //will add option to use knives aswell in futire
        playerEquipmentManager.weaponAnimator.ShootWeapon(playerCamera);
    }

}

