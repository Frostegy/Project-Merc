using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;
    public AmmoType ammoType;

    [Header("Prefab")]
    public RayCastWeapon weaponPrefab;

    [Header("Shooting")]
    public int damage = 10;
    public float fireRate = 5f;
    public float spread = 0f;
    public float range = 100f;
    public int pelletsPerShot = 1;
    public bool allowButtonHold = true;

    [Header("Ammo")]
    public int magazineSize = 10;
    public float reloadTime = 1.5f;

    [Header("Inventory")]
    public Vector2Int inventorySize = new Vector2Int(2, 1);
}
