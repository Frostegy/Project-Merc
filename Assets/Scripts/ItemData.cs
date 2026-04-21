using UnityEngine;

public enum ItemType
{
    Ammo,
    Weapon,
    Healing
}

[CreateAssetMenu(menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public ItemType itemType;

    public int width = 1;
    public int height = 1;

    public bool stackable = false;
    public int maxStack = 1;

    public WeaponData weaponData; // Only used if itemType is Weapon
    public AmmoType ammoType; // Only used if itemType is Ammo
    public int ammoAmount = 0; // Only used if itemType is Ammo

    public int healAmount = 0; // Only used if itemType is Healing
}