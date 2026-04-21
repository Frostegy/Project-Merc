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

    public bool shootPressedInput;
    public bool reloadPressedInput;
    public bool holsterPressedInput;
    public bool slot1PressedInput;
    public bool slot2PressedInput;
    public bool inventoryPressedInput;
    public bool breakFreePressedInput;

    public bool inputLocked;

    bool previousShootInput;

    bool reloadQueued;
    bool holsterQueued;
    bool slot1Queued;
    bool slot2Queued;
    bool inventoryQueued;
    bool breakFreeQueued;

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

            playerControls.PlayerActions.Reload.performed += i => reloadQueued = true;
            playerControls.PlayerActions.Holster.performed += i => holsterQueued = true;
            playerControls.PlayerActions.Slot1.performed += i => slot1Queued = true;
            playerControls.PlayerActions.Slot2.performed += i => slot2Queued = true;
            playerControls.PlayerActions.Inventory.performed += i => inventoryQueued = true;
            playerControls.PlayerActions.BreakFree.performed += i => breakFreeQueued = true;
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
        // these still need to work while inputLocked is true
        inventoryPressedInput = inventoryQueued;
        breakFreePressedInput = breakFreeQueued;

        inventoryQueued = false;
        breakFreeQueued = false;

        if (inputLocked)
        {
            horizontalMovementInput = 0f;
            verticalMovementInput = 0f;
            horizontalCameraInput = 0f;
            verticalCameraInput = 0f;

            runInput = false;
            aimingInput = false;
            shootInput = false;
            shootPressedInput = false;
            reloadPressedInput = false;
            holsterPressedInput = false;
            slot1PressedInput = false;
            slot2PressedInput = false;

            previousShootInput = false;
            return;
        }

        horizontalMovementInput = movementInput.x;
        verticalMovementInput = movementInput.y;

        horizontalCameraInput = cameraInput.x;
        verticalCameraInput = cameraInput.y;

        shootPressedInput = shootInput && !previousShootInput;
        previousShootInput = shootInput;

        reloadPressedInput = reloadQueued;
        holsterPressedInput = holsterQueued;
        slot1PressedInput = slot1Queued;
        slot2PressedInput = slot2Queued;

        reloadQueued = false;
        holsterQueued = false;
        slot1Queued = false;
        slot2Queued = false;
    }
}

