using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Grid Inventory")]
    public Inventory gridInventory;

    [Header("Owned Weapons")]
    public List<WeaponData> ownedWeapons = new List<WeaponData>();

    public bool TryAddToInventory(ItemData itemData, int amount = 1)
    {
        if (gridInventory == null || itemData == null)
        {
            return false;
        }

        return gridInventory.TryAddItem(itemData, amount);
    }

    public void AddWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            return;
        }

        if (!ownedWeapons.Contains(weaponData))
        {
            ownedWeapons.Add(weaponData);
        }
    }

    public bool HasWeapon(WeaponData weaponData)
    {
        return ownedWeapons.Contains(weaponData);
    }

    public int GetAmmo(AmmoType ammoType)
    {
        if (gridInventory == null)
        {
            return 0;
        }

        int total = 0;

        for (int i = 0; i < gridInventory.items.Count; i++)
        {
            InventoryItem item = gridInventory.items[i];
            if (item == null || item.itemData == null)
            {
                continue;
            }

            if (item.itemData.itemType != ItemType.Ammo)
            {
                continue;
            }

            if (item.itemData.ammoType != ammoType)
            {
                continue;
            }

            total += item.ammoInItem;
        }

        return total;
    }

    public int RemoveAmmo(AmmoType ammoType, int amount)
    {
        if (gridInventory == null || amount <= 0)
        {
            return 0;
        }

        int removed = 0;

        for (int i = gridInventory.items.Count - 1; i >= 0 && removed < amount; i--)
        {
            InventoryItem item = gridInventory.items[i];
            if (item == null || item.itemData == null)
            {
                continue;
            }

            if (item.itemData.itemType != ItemType.Ammo)
            {
                continue;
            }

            if (item.itemData.ammoType != ammoType)
            {
                continue;
            }

            int take = Mathf.Min(item.ammoInItem, amount - removed);
            item.ammoInItem -= take;
            removed += take;

            if (item.ammoInItem <= 0)
            {
                gridInventory.RemoveItem(item);
            }
        }

        gridInventory.MarkDirty();
        return removed;
    }
}
