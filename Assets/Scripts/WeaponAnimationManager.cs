using UnityEngine;

public class WeaponAnimationManager : MonoBehaviour
{
    Animator weaponAnimator;

    [Header("Weapon FX")]
    public GameObject weaponMuzzleFlashFX; // the muzzzle flash FX that is instantiated when the weapon is fired
    public GameObject weaponBulletCaseFX; //the bullet case FX that is instantiated when the weapon is fired

    [Header("Weapon FX Transforms")]
    public Transform weaponMuzzleFlashTransform; // the loaction where the muzzle flash FX is instantiated
    public Transform weaponBulletCaseTransform; // the location where the bullet case FX is instantiated

    private void Awake()
    {
        weaponAnimator = GetComponentInChildren<Animator>();
    }

    public void ShootWeapon(CameraController playerCamera)
    {
        weaponAnimator.Play("Shoot"); // animate the weapon firing

        GameObject muzzleFlash = Instantiate(weaponMuzzleFlashFX, weaponMuzzleFlashTransform); // instantiate the muzzle flash FX at the correct location
        muzzleFlash.transform.parent = null; // unparent the muzzle flash FX so it doesn't move with the weapon
        GameObject bulletCase = Instantiate(weaponBulletCaseFX, weaponBulletCaseTransform); // instantiate the bullet case FX at the correct location
        bulletCase.transform.parent = null; // unparent the bullet case FX so it doesn't move with the weapon

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.cameraObject.transform.position, playerCamera.cameraObject.transform.forward, out hit))
        {
            Debug.Log(hit.transform.gameObject.name); 
        }
    }
}
