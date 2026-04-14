using UnityEngine;

public class WeaponAnimationManager : MonoBehaviour
{
    Animator weaponAnimator;

    [Header("Weapon FX")]
    public GameObject weaponMuzzleFlashFX;
    public GameObject weaponBulletCaseFX;

    [Header("Weapon FX Transforms")]
    public Transform weaponMuzzleFlashTransform;
    public Transform weaponBulletCaseTransform;

    [Header("Shooting")]
    public bool useProjectile;
    public Transform bulletProjectile;
    public Transform bulletSpawnPosition;
    public LayerMask shootLayerMask;
    public float shootDistance = 999f;

    private void Awake()
    {
        weaponAnimator = GetComponentInChildren<Animator>();
    }

    public void ShootWeapon(Camera gameplayCamera, Vector3 aimWorldPosition)
    {
        weaponAnimator.Play("Shoot");

        GameObject muzzleFlash = Instantiate(weaponMuzzleFlashFX, weaponMuzzleFlashTransform);
        muzzleFlash.transform.parent = null;

        GameObject bulletCase = Instantiate(weaponBulletCaseFX, weaponBulletCaseTransform);
        bulletCase.transform.parent = null;

        Transform shootFrom = bulletSpawnPosition != null ? bulletSpawnPosition : weaponMuzzleFlashTransform;
        Vector3 aimDirection = (aimWorldPosition - shootFrom.position).normalized;

        if (useProjectile && bulletProjectile != null)
        {
            Instantiate(bulletProjectile, shootFrom.position, Quaternion.LookRotation(aimDirection, Vector3.up));
        }
        else
        {
            if (Physics.Raycast(shootFrom.position, aimDirection, out RaycastHit hit, shootDistance, shootLayerMask))
            {
                Debug.Log(hit.transform.gameObject.name);
            }
        }
    }
}