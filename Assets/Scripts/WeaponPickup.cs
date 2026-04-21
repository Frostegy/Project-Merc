using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RayCastWeapon weaponFab;

    private void OnTriggerEnter(Collider other)
    {
        ActiveWeapon activeWeapon = other.GetComponent<ActiveWeapon>();
        if(activeWeapon)
        {
            RayCastWeapon newWeapon = Instantiate(weaponFab);
            activeWeapon.Equip(newWeapon);
        }
    }
}
