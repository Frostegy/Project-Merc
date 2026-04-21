using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int amount;
    public Vector2Int position;
    public bool rotated;

    // Used only for Ammo items.
    // This lets each ammo box keep track of how many rounds are still left in it.
    public int ammoInItem;

    public int Width
    {
        get
        {
            if (itemData == null) return 0;
            return rotated ? itemData.height : itemData.width;
        }
    }

    public int Height
    {
        get
        {
            if (itemData == null) return 0;
            return rotated ? itemData.width : itemData.height;
        }
    }

    public InventoryItem(ItemData data, int startAmount)
    {
        itemData = data;
        amount = startAmount;
        position = Vector2Int.zero;
        rotated = false;

        if (itemData != null && itemData.itemType == ItemType.Ammo)
        {
            ammoInItem = itemData.ammoAmount;
        }
        else
        {
            ammoInItem = 0;
        }
    }
}
