using UnityEngine;

public class PlayerLocomotionManager : MonoBehaviour
{
    InputManager inputManager;
    PlayerManager playerManager;
    public Transform cameraHolderTransform;

    public float rotationSpeed = 3.5f;

    Quaternion targetRotation; //the place we want to rotate to
    Quaternion playerRotation; //the place we are rotating now

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        playerManager = GetComponent<PlayerManager>();
    }

    public void HandleAllLocomotion()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (playerManager.isAiming)
        {
            targetRotation = Quaternion.Euler(0, cameraHolderTransform.eulerAngles.y, 0);
            playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = playerRotation;
        }
        else
        {
            targetRotation = Quaternion.Euler(0, cameraHolderTransform.eulerAngles.y, 0);
            playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (inputManager.verticalMovementInput != 0 || inputManager.horizontalMovementInput != 0)
            {
                transform.rotation = playerRotation;
            }
        }




       
    }
}
