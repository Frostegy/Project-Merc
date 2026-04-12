using UnityEngine;

public class WeaponLoaderSlot : MonoBehaviour
{
    public GameObject currentWeaponModel;

    private void UnloadAndDestroyWeapon()
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            
        }
    }

    public void LoadWeaponModel(WeaponItem weapon)
    {
        UnloadAndDestroyWeapon();

        if(weapon == null)
        {
            return;
        }

        GameObject weaponModel = Instantiate(weapon.itemModel, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.identity;
        weaponModel.transform.localScale = Vector3.one;
        currentWeaponModel = weaponModel;






    }
}
