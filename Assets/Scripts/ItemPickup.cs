using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Data")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Debug")]
    public bool logPickup = true;

    void OnTriggerEnter(Collider other)
    {
        if (itemData == null)
        {
            if (logPickup)
            {
                Debug.LogWarning($"{name}: ItemPickup has no ItemData assigned.", this);
            }
            return;
        }

        PlayerInventory playerInventory = FindPlayerInventory(other);

        if (playerInventory == null)
        {
            if (logPickup)
            {
                Debug.Log($"{name}: Triggered by {other.name}, but no PlayerInventory was found on it or its parents.", this);
            }
            return;
        }

        if (playerInventory.gridInventory == null)
        {
            if (logPickup)
            {
                Debug.LogWarning($"{name}: PlayerInventory was found on {playerInventory.name}, but gridInventory is not assigned.", playerInventory);
            }
            return;
        }

        bool added = playerInventory.TryAddToInventory(itemData, amount);

        if (!added)
        {
            if (logPickup)
            {
                Debug.Log($"{name}: Could not add {amount} x {itemData.itemName}. Inventory may be full or the item may not fit.", this);
            }
            return;
        }

        if (logPickup)
        {
            Debug.Log($"Picked up {amount} x {itemData.itemName}", this);
        }

        Destroy(gameObject);
    }

    PlayerInventory FindPlayerInventory(Collider other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory != null)
        {
            return inventory;
        }

        if (other.attachedRigidbody != null)
        {
            inventory = other.attachedRigidbody.GetComponentInParent<PlayerInventory>();
            if (inventory != null)
            {
                return inventory;
            }
        }

        return null;
    }
}
