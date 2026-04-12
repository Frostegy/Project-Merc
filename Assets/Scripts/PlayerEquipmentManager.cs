using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    AnimatorManager animatorManager;
    WeaponLoaderSlot weaponLoaderSlot;

    [Header("Current Equipment")]
    public WeaponItem weapon;
    public WeaponAnimationManager weaponAnimator;
    RightHandIKTarget rightHandIK;
    LeftHandIKTarget leftHandIK;
    //public SubWeaponItem subWeapon; subweapon; //knife, grenade, etc.

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        LoadWeaponLoaderSlots();
    }

    private void Start()
    {
        LoadCurrentWeapon();
    }

    private void LoadWeaponLoaderSlots()
    {
        //back Slot
        //hip Slot
        weaponLoaderSlot = GetComponentInChildren<WeaponLoaderSlot>();
    }

    private void LoadCurrentWeapon()
    {
       weaponLoaderSlot.LoadWeaponModel(weapon);
       animatorManager.animator.runtimeAnimatorController = weapon.weaponAnim;
       weaponAnimator = weaponLoaderSlot.currentWeaponModel.GetComponentInChildren<WeaponAnimationManager>();
       rightHandIK = weaponLoaderSlot.currentWeaponModel.GetComponentInChildren<RightHandIKTarget>();
       leftHandIK = weaponLoaderSlot.currentWeaponModel.GetComponentInChildren<LeftHandIKTarget>();
       animatorManager.AssignHandIK(rightHandIK,leftHandIK);
    } 
}
