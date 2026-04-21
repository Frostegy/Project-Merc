using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHud : MonoBehaviour
{
    public ActiveWeapon activeWeapon;

    public Image weaponIconImage;
    public TMP_Text ammoText;

    void Update()
    {
        UpdateHud();
    }

    void UpdateHud()
    {
        if (activeWeapon == null || activeWeapon.CurrentWeapon == null || activeWeapon.CurrentWeapon.weaponData == null)
        {
            if (weaponIconImage != null)
            {
                weaponIconImage.enabled = false;
            }

            if (ammoText != null)
            {
                ammoText.text = "";
            }

            return;
        }

        WeaponData data = activeWeapon.CurrentWeapon.weaponData;

        if (weaponIconImage != null)
        {
            weaponIconImage.enabled = data.weaponIcon != null;
            weaponIconImage.sprite = data.weaponIcon;
        }

        if (ammoText != null)
        {
            ammoText.text = activeWeapon.GetBulletsInMagazine() + "/" + activeWeapon.GetReserveAmmo();
        }
    }
}
