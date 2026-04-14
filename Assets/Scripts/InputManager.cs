using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;

    public float verticalMovementInput;
    public float horizontalMovementInput;
    Vector2 movementInput;

    public float verticalCameraInput;
    public float horizontalCameraInput;
    Vector2 cameraInput;

    public bool runInput;
    public bool aimingInput;
    public bool shootInput;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Movement.canceled += i => movementInput = Vector2.zero;

            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Camera.canceled += i => cameraInput = Vector2.zero;

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
        if (playerControls != null)
        {
            playerControls.Disable();
        }
    }

    public void HandleAllInputs()
    {
        horizontalMovementInput = movementInput.x;
        verticalMovementInput = movementInput.y;

        horizontalCameraInput = cameraInput.x;
        verticalCameraInput = cameraInput.y;
    }
}