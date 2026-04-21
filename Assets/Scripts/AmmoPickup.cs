using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public ItemData ammoItemData;
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();

        if (inventory == null || ammoItemData == null)
        {
            return;
        }

        bool added = inventory.TryAddToInventory(ammoItemData, amount);

        if (!added)
        {
            Debug.Log("Could not pick up ammo item.");
            return;
        }

        Debug.Log("Picked up " + amount + " x " + ammoItemData.itemName);
        Destroy(gameObject);
    }
}