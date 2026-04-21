using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    [Header("References")]
    public Transform crossHairTarget;
    public Transform weaponParent;
    public Transform[] weaponSlots;
    public Animator rigController;
    public PlayerInventory inventory;

    [Header("Animation")]
    [SerializeField] private float animationFade = 0.05f;
    [SerializeField] private string aimLayerName = "Aim Layer";

    public enum WeaponSlot
    {
        Primary = 0,
        Secondary = 1,
    }

    [Header("Current Weapon")]
    public RayCastWeapon[] equippedWeapons = new RayCastWeapon[2];
    [SerializeField] private int activeWeaponIndex;

    private readonly Dictionary<string, int> savedMagazineAmmo = new Dictionary<string, int>();

    private int bulletsInMagazine;
    private float nextShotTime;
    private bool wantsToFire;
    private bool isReloading;
    private bool wasAiming;

    public RayCastWeapon CurrentWeapon => GetWeapon(activeWeaponIndex);

    public bool IsHolstered => rigController != null && rigController.GetBool("Holster");

    private int AimLayerIndex
    {
        get
        {
            if (rigController == null) return -1;
            return rigController.GetLayerIndex(aimLayerName);
        }
    }

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponentInParent<PlayerInventory>();
        }
    }

    private void Start()
    {
        RayCastWeapon[] existingWeapons = GetComponentsInChildren<RayCastWeapon>(true);

        for (int i = 0; i < existingWeapons.Length; i++)
        {
            if (existingWeapons[i] != null)
            {
                Equip(existingWeapons[i]);
            }
        }

        if (CurrentWeapon != null)
        {
            LoadMagazineForCurrentWeapon();
        }

        RefreshWeaponParents();
    }

    private void Update()
    {
        HandleDebugSlotSwitchInput();

        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null)
        {
            return;
        }

        if (isReloading)
        {
            return;
        }

        if (Time.time >= nextShotTime && wantsToFire)
        {
            TryFireRound();
        }
    }

    private RayCastWeapon GetWeapon(int index)
    {
        if (index < 0 || index >= equippedWeapons.Length)
        {
            return null;
        }

        return equippedWeapons[index];
    }

    private string GetAimStateName(RayCastWeapon weapon)
    {
        return "weapon_anim_" + weapon.weaponName + "Aim";
    }

    private string GetShootStateName(RayCastWeapon weapon)
    {
        return "weapon_anim_" + weapon.weaponName + "Shoot";
    }

    private string GetReloadStateName(RayCastWeapon weapon)
    {
        return "weapon_anim_" + weapon.weaponName + "Reload";
    }

    private string GetEquipStateName(RayCastWeapon weapon)
    {
        return "equip_" + weapon.weaponName;
    }

    private void PlayState(string stateName, int layer = 0)
    {
        if (rigController == null || string.IsNullOrEmpty(stateName))
        {
            return;
        }

        rigController.CrossFadeInFixedTime(stateName, animationFade, layer);
    }

    private void PlayReadyPoseForCurrentWeapon()
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || rigController == null)
        {
            return;
        }

        if (IsHolstered)
        {
            return;
        }

        if (rigController.GetBool("IsAiming"))
        {
            int aimLayer = AimLayerIndex;
            if (aimLayer >= 0)
            {
                PlayState(GetAimStateName(weapon), aimLayer);
            }
            else
            {
                PlayState(GetAimStateName(weapon), 0);
            }
        }
        else
        {
            PlayState(GetEquipStateName(weapon), 0);
        }
    }

    private void RefreshWeaponParents()
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            RayCastWeapon weapon = equippedWeapons[i];
            if (weapon == null)
            {
                continue;
            }

            Transform targetParent = (i == activeWeaponIndex) ? weaponParent : null;

            if (targetParent == null && i < weaponSlots.Length)
            {
                targetParent = weaponSlots[i];
            }

            if (targetParent == null)
            {
                continue;
            }

            weapon.transform.SetParent(targetParent);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            if (i == activeWeaponIndex)
            {
                weapon.rayCastDestination = crossHairTarget;
            }
        }
    }

    public void Equip(RayCastWeapon newWeapon)
    {
        if (newWeapon == null)
        {
            return;
        }

        int weaponSlotIndex = (int)newWeapon.weaponSlot;
        if (weaponSlotIndex < 0 || weaponSlotIndex >= equippedWeapons.Length)
        {
            Debug.LogWarning("Invalid weapon slot index " + weaponSlotIndex + " on " + newWeapon.name);
            return;
        }

        SaveCurrentMagazineAmmo();

        RayCastWeapon weaponInSlot = GetWeapon(weaponSlotIndex);
        if (weaponInSlot != null && weaponInSlot != newWeapon)
        {
            Destroy(weaponInSlot.gameObject);
        }

        equippedWeapons[weaponSlotIndex] = newWeapon;

        if (inventory != null && newWeapon.weaponData != null)
        {
            inventory.AddWeapon(newWeapon.weaponData);
        }

        activeWeaponIndex = weaponSlotIndex;
        LoadMagazineForCurrentWeapon();
        wantsToFire = false;
        nextShotTime = 0f;

        if (rigController != null)
        {
            rigController.SetBool("Holster", false);
        }

        RefreshWeaponParents();
        PlayReadyPoseForCurrentWeapon();
    }

    public void SwitchWeapon(WeaponSlot slot)
    {
        SwitchWeapon((int)slot);
    }

    public void SwitchWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedWeapons.Length)
        {
            return;
        }

        if (slotIndex == activeWeaponIndex)
        {
            return;
        }

        RayCastWeapon targetWeapon = GetWeapon(slotIndex);
        if (targetWeapon == null)
        {
            return;
        }

        SaveCurrentMagazineAmmo();
        activeWeaponIndex = slotIndex;
        LoadMagazineForCurrentWeapon();
        wantsToFire = false;
        nextShotTime = 0f;

        if (rigController != null)
        {
            rigController.SetBool("Holster", false);
        }

        RefreshWeaponParents();
        PlayReadyPoseForCurrentWeapon();
    }

    public void SetAiming(bool aiming)
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || rigController == null)
        {
            return;
        }

        if (IsHolstered)
        {
            aiming = false;
        }

        rigController.SetBool("IsAiming", aiming);

        if (aiming == wasAiming)
        {
            return;
        }

        wasAiming = aiming;

        if (isReloading)
        {
            return;
        }

        PlayReadyPoseForCurrentWeapon();
    }

    public void SetFiringInput(bool isAiming, bool shootHeld, bool shootPressed)
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null || rigController == null)
        {
            wantsToFire = false;
            return;
        }

        bool canAttemptFire =
            isAiming &&
            rigController.GetBool("IsAiming") &&
            !rigController.GetBool("Holster") &&
            !isReloading;

        if (!canAttemptFire)
        {
            wantsToFire = false;
            return;
        }

        wantsToFire = weapon.weaponData.allowButtonHold ? shootHeld : shootPressed;
    }

    public void TryReload()
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null || inventory == null || rigController == null)
        {
            return;
        }

        if (isReloading || rigController.GetBool("Holster"))
        {
            return;
        }

        if (bulletsInMagazine >= weapon.weaponData.magazineSize)
        {
            return;
        }

        if (inventory.GetAmmo(weapon.weaponData.ammoType) <= 0)
        {
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    public void ToggleHolster()
    {
        if (rigController == null)
        {
            return;
        }

        SetHolster(!rigController.GetBool("Holster"));
    }

    public void SetHolster(bool holster)
    {
        if (rigController == null)
        {
            return;
        }

        rigController.SetBool("Holster", holster);
        wantsToFire = false;

        if (holster)
        {
            rigController.SetBool("IsAiming", false);
            wasAiming = false;
            return;
        }

        PlayReadyPoseForCurrentWeapon();
    }

    public int GetBulletsInMagazine()
    {
        return bulletsInMagazine;
    }

    public int GetReserveAmmo()
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null || inventory == null)
        {
            return 0;
        }

        return inventory.GetAmmo(weapon.weaponData.ammoType);
    }

    private void TryFireRound()
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null)
        {
            return;
        }

        if (bulletsInMagazine <= 0)
        {
            wantsToFire = false;
            return;
        }

        bulletsInMagazine--;

        int aimLayer = AimLayerIndex;
        if (rigController != null && rigController.GetBool("IsAiming") && aimLayer >= 0)
        {
            PlayState(GetShootStateName(weapon), aimLayer);
        }
        else
        {
            PlayState(GetShootStateName(weapon), 0);
        }

        weapon.FireOnce();

        float fireRate = Mathf.Max(0.01f, weapon.weaponData.fireRate);
        nextShotTime = Time.time + (1f / fireRate);

        if (!weapon.weaponData.allowButtonHold)
        {
            wantsToFire = false;
        }
    }

    private IEnumerator ReloadRoutine()
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null || rigController == null)
        {
            yield break;
        }

        isReloading = true;
        wantsToFire = false;

        bool aiming = rigController.GetBool("IsAiming");
        int aimLayer = AimLayerIndex;

        if (aiming && aimLayer >= 0)
        {
            PlayState(GetReloadStateName(weapon), aimLayer);
        }
        else
        {
            PlayState(GetReloadStateName(weapon), 0);
        }

        yield return new WaitForSeconds(weapon.weaponData.reloadTime);

        weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null || inventory == null)
        {
            isReloading = false;
            yield break;
        }

        int needed = weapon.weaponData.magazineSize - bulletsInMagazine;
        int loaded = inventory.RemoveAmmo(weapon.weaponData.ammoType, needed);
        bulletsInMagazine += loaded;

        isReloading = false;
        PlayReadyPoseForCurrentWeapon();
    }

    private void SaveCurrentMagazineAmmo()
    {
        RayCastWeapon weapon = CurrentWeapon;
        if (weapon == null || weapon.weaponData == null)
        {
            return;
        }

        savedMagazineAmmo[weapon.weaponData.weaponName] = bulletsInMagazine;
    }

    private int GetSavedOrDefaultMagazineAmmoForWeapon(RayCastWeapon weapon)
    {
        if (weapon == null || weapon.weaponData == null)
        {
            return 0;
        }

        if (savedMagazineAmmo.TryGetValue(weapon.weaponData.weaponName, out int savedAmmo))
        {
            return savedAmmo;
        }

        return weapon.weaponData.magazineSize;
    }

    private void LoadMagazineForCurrentWeapon()
    {
        bulletsInMagazine = GetSavedOrDefaultMagazineAmmoForWeapon(CurrentWeapon);
    }

    private void HandleDebugSlotSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(WeaponSlot.Primary);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(WeaponSlot.Secondary);
        }
    }
}
