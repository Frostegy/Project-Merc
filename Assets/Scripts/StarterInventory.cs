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

    public PlayerInventory playerInventory;
    public List<StarterItem> startingItems = new List<StarterItem>();

    void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("InventoryStarter could not find a PlayerInventory.");
            return;
        }

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
            }
        }
    }
}
