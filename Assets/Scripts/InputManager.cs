using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    AnimatorManager animatorManager;
    Animator animator;
    PlayerUiManager playerUiManager;
    PlayerManager playerManager;

    public float verticalMovementInput;
    public float horizontalMovementInput;
    private Vector2 movementInput;

    public float verticalCameraInput;
    public float horizontalCameraInput;
    private Vector2 cameraInput;

    
    public bool runInput;
    public bool aimingInput;
    public bool shootInput;

    private void Awake()
    { 
        animatorManager = GetComponent<AnimatorManager>();
        animator = GetComponent<Animator>();
        playerUiManager = GetComponent<PlayerUiManager>();
        playerManager = GetComponent<PlayerManager>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls(); 

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Run.performed += i => runInput = true;
            playerControls.PlayerMovement.Run.canceled += i => runInput = false;
            playerControls.PlayerActions.Aim.performed += i => aimingInput = true;
            playerControls.PlayerActions.Aim.canceled += i => aimingInput = false;
            playerControls.PlayerActions.Shoot.performed += i => shootInput = true;
            playerControls.PlayerActions.Shoot.canceled += i => shootInput = false;
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleCameraInput();
        HandleAimingInput();
        HandleShootingInput();
    }

    private void HandleMovementInput()
    {
        horizontalMovementInput = movementInput.x;
        verticalMovementInput = movementInput.y;
        
        animatorManager.HandleAnimatorValues(horizontalMovementInput, verticalMovementInput, runInput);

        if (verticalMovementInput !=0 || horizontalMovementInput != 0)
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

    private void HandleCameraInput()
    {
        horizontalCameraInput = cameraInput.x;
        verticalCameraInput = cameraInput.y;
    }

    private void HandleAimingInput()
    {
        if (verticalMovementInput != 0 || horizontalMovementInput != 0)
        {
            aimingInput = false;
            animator.SetBool("isAiming", false);
            playerUiManager.crossHair.SetActive(false);
            return;
        }

        if (aimingInput)
        {
            animator.SetBool("isAiming", true);
            playerUiManager.crossHair.SetActive(true);
        }
        else
        {
            animator.SetBool("isAiming", false);
            playerUiManager.crossHair.SetActive(false);
        }

    }

    private void HandleShootingInput()
    {
        if (shootInput && aimingInput)
        {
            shootInput = false;
            Debug.Log("Bang!");
            playerManager.UseCurrentWeapon();

        }
    }
}
