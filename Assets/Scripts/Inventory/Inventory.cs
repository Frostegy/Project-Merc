using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Grid Size")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    [Header("Items")]
    public List<InventoryItem> items = new List<InventoryItem>();

    public event Action OnInventoryChanged;

    public bool TryAddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        bool changed = false;

        if (itemData.stackable)
        {
            int leftToAdd = amount;

            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem item = items[i];

                if (item.itemData != itemData)
                {
                    continue;
                }

                if (item.amount >= itemData.maxStack)
                {
                    continue;
                }

                int spaceLeft = itemData.maxStack - item.amount;
                int amountToAdd = Mathf.Min(spaceLeft, leftToAdd);

                item.amount += amountToAdd;
                leftToAdd -= amountToAdd;
                changed = true;

                if (leftToAdd <= 0)
                {
                    NotifyChanged();
                    return true;
                }
            }

            amount = leftToAdd;
        }

        while (amount > 0)
        {
            int stackAmount = itemData.stackable ? Mathf.Min(itemData.maxStack, amount) : 1;
            InventoryItem newItem = new InventoryItem(itemData, stackAmount);

            if (!TryFindSpace(newItem, out Vector2Int foundPos))
            {
                if (changed)
                {
                    NotifyChanged();
                }
                return false;
            }

            newItem.position = foundPos;
            items.Add(newItem);
            amount -= stackAmount;
            changed = true;
        }

        if (changed)
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryMoveItem(InventoryItem item, Vector2Int newPos)
    {
        if (item == null || !items.Contains(item))
        {
            return false;
        }

        Vector2Int oldPos = item.position;
        item.position = newPos;

        if (CanPlaceItem(item, item))
        {
            NotifyChanged();
            return true;
        }

        item.position = oldPos;
        return false;
    }

    public bool TryRotateItem(InventoryItem item)
    {
        if (item == null || !items.Contains(item))
        {
            return false;
        }

        bool oldRotated = item.rotated;
        Vector2Int oldPos = item.position;

        item.rotated = !item.rotated;

        if (CanPlaceItem(item, item))
        {
            NotifyChanged();
            return true;
        }

        int clampedX = Mathf.Clamp(item.position.x, 0, Mathf.Max(0, gridWidth - item.Width));
        int clampedY = Mathf.Clamp(item.position.y, 0, Mathf.Max(0, gridHeight - item.Height));
        item.position = new Vector2Int(clampedX, clampedY);

        if (CanPlaceItem(item, item))
        {
            NotifyChanged();
            return true;
        }

        for (int y = Mathf.Max(0, clampedY - 1); y <= Mathf.Min(gridHeight - item.Height, clampedY + 1); y++)
        {
            for (int x = Mathf.Max(0, clampedX - 1); x <= Mathf.Min(gridWidth - item.Width, clampedX + 1); x++)
            {
                item.position = new Vector2Int(x, y);

                if (CanPlaceItem(item, item))
                {
                    NotifyChanged();
                    return true;
                }
            }
        }

        item.rotated = oldRotated;
        item.position = oldPos;
        return false;
    }

    public InventoryItem GetItemAt(Vector2Int cellPos)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (ItemOccupiesCell(items[i], cellPos))
            {
                return items[i];
            }
        }

        return null;
    }

    public void RemoveItem(InventoryItem item)
    {
        if (item != null && items.Remove(item))
        {
            NotifyChanged();
        }
    }

    public void MarkDirty()
    {
        NotifyChanged();
    }

    public bool CanPlaceItem(InventoryItem item, InventoryItem ignoreItem = null)
    {
        if (item == null || item.itemData == null)
        {
            return false;
        }

        if (item.position.x < 0 || item.position.y < 0)
        {
            return false;
        }

        if (item.position.x + item.Width > gridWidth)
        {
            return false;
        }

        if (item.position.y + item.Height > gridHeight)
        {
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem other = items[i];

            if (other == ignoreItem)
            {
                continue;
            }

            if (ItemsOverlap(item, other))
            {
                return false;
            }
        }

        return true;
    }

    bool TryFindSpace(InventoryItem item, out Vector2Int foundPos)
    {
        item.rotated = false;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                item.position = new Vector2Int(x, y);

                if (CanPlaceItem(item))
                {
                    foundPos = item.position;
                    return true;
                }
            }
        }

        item.rotated = true;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                item.position = new Vector2Int(x, y);

                if (CanPlaceItem(item))
                {
                    foundPos = item.position;
                    return true;
                }
            }
        }

        foundPos = Vector2Int.zero;
        return false;
    }

    bool ItemOccupiesCell(InventoryItem item, Vector2Int cell)
    {
        return cell.x >= item.position.x &&
               cell.x < item.position.x + item.Width &&
               cell.y >= item.position.y &&
               cell.y < item.position.y + item.Height;
    }

    bool ItemsOverlap(InventoryItem a, InventoryItem b)
    {
        return a.position.x < b.position.x + b.Width &&
               a.position.x + a.Width > b.position.x &&
               a.position.y < b.position.y + b.Height &&
               a.position.y + a.Height > b.position.y;
    }

    void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}
