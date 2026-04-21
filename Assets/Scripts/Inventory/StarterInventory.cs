using System.Collections.Generic;
using UnityEngine;

public class StarterInventory : MonoBehaviour
{
    [System.Serializable]
    public class StarterItem
    {
        public ItemData itemData;
        public int amount = 1;
    }

    [Header("References")]
    public PlayerInventory playerInventory;
    public ActiveWeapon activeWeapon;

    [Header("Starting Inventory")]
    public List<StarterItem> startingItems = new List<StarterItem>();

    [Header("Auto Equip")]
    public bool autoEquipFirstWeapon = true;
    public ItemData weaponToAutoEquip; // optional, leave null to equip first weapon found

    void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        if (activeWeapon == null)
        {
            activeWeapon = GetComponentInChildren<ActiveWeapon>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("StarterInventory could not find a PlayerInventory.");
            return;
        }

        ItemData firstWeaponFound = null;

        for (int i = 0; i < startingItems.Count; i++)
        {
            StarterItem entry = startingItems[i];

            if (entry == null || entry.itemData == null || entry.amount <= 0)
            {
                continue;
            }

            bool added = playerInventory.TryAddToInventory(entry.itemData, entry.amount);

            if (!added)
            {
                Debug.LogWarning("Could not add starting item: " + entry.itemData.itemName);
                continue;
            }

            if (firstWeaponFound == null && entry.itemData.itemType == ItemType.Weapon)
            {
                firstWeaponFound = entry.itemData;
            }
        }

        if (!autoEquipFirstWeapon || activeWeapon == null)
        {
            return;
        }

        ItemData itemToEquip = weaponToAutoEquip != null ? weaponToAutoEquip : firstWeaponFound;

        if (itemToEquip == null)
        {
            return;
        }

        if (itemToEquip.itemType != ItemType.Weapon || itemToEquip.weaponData == null)
        {
            Debug.LogWarning("Auto-equip item is not a valid weapon.");
            return;
        }

        if (itemToEquip.weaponData.weaponPrefab == null)
        {
            Debug.LogWarning("WeaponData has no weaponPrefab assigned for " + itemToEquip.itemName);
            return;
        }

        RayCastWeapon newWeapon = Instantiate(
            itemToEquip.weaponData.weaponPrefab,
            activeWeapon.transform.position,
            activeWeapon.transform.rotation
        );

        activeWeapon.Equip(newWeapon);
    }
}