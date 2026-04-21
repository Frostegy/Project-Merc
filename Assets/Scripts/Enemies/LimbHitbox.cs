using UnityEngine;


public enum ZombieLimb
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
}
public class LimbHitbox : MonoBehaviour
{
    public ZombieLimb  limb;
    public ZombieManager zombieManager;

    private void Reset()
    {
        if(zombieManager == null)
        {

            zombieManager = GetComponentInParent<ZombieManager>();


        }


    }

    public void TakeHit(int Damage)
    {
        if (zombieManager != null)
        {
            zombieManager.TakeDamage(limb, Damage);
        }
    }
}
