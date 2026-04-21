using UnityEngine;


public class AnimatorManager : MonoBehaviour
{
    public Animator animator;
    float snappedHorizontal;
    float snappedVertical;

    private void Awake()
    {
        animator = GetComponent<Animator>();
      
    }

    public void HandleAnimatorValues(float horizontalMovement, float verticalMovement, bool isRunning)
    {
        if (horizontalMovement > 0)
        {
            snappedHorizontal = 1;
        }
        else if (horizontalMovement < 0)
        {
            snappedHorizontal = -1;
        }
        else
        {
            snappedHorizontal = 0;
        }

        if (verticalMovement > 0)
        {
            snappedVertical = 1;
        }
        else if (verticalMovement < 0)
        {
            snappedVertical = -1;
        }
        else
        {
            snappedVertical = 0;
        }

        if(isRunning && snappedVertical > 0) //no running if going backwards
        {
            snappedVertical = 2;
        }

        animator.SetFloat("Horizontal", snappedHorizontal, 0.1f, Time.deltaTime);
        animator.SetFloat("Vertical", snappedVertical, 0.1f, Time.deltaTime);
    }

    
}
