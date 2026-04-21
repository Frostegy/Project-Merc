using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public ActiveWeapon activeWeapon;
    public RectTransform cellParent;
    public RectTransform itemParent;
    public RectTransform dragParent;
    public GameObject cellPrefab;
    public GameObject itemPrefab;
    public GameObject inventoryWindow;

    [Header("Layout")]
    public int cellSize = 50;
    public int spacing = 0;

    [Header("Controls")]
    public PlayerHealth playerHealth;

    bool builtGrid;
    InputManager inputManager;

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += RefreshUI;
        }

        RefreshUI();
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshUI;
        }

        if (inputManager != null)
        {
            inputManager.inputLocked = false;
        }

        GamePause.IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        inputManager = FindFirstObjectByType<InputManager>();

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(false);
        }

        GamePause.IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ResizeGridParents();
        BuildGrid();
        RefreshUI();
    }

    void Update()
    {
        if (inputManager != null && inputManager.inventoryPressedInput)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryWindow == null)
        {
            return;
        }

        bool isOpen = !inventoryWindow.activeSelf;
        inventoryWindow.SetActive(isOpen);

        if (inputManager != null)
        {
            inputManager.inputLocked = isOpen;
        }

        GamePause.IsPaused = isOpen;

        if (isOpen)
        {
            RefreshUI();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UseItem(InventoryItem item)
    {
        if (item == null || item.itemData == null)
        {
            return;
        }

        switch (item.itemData.itemType)
        {
            case ItemType.Weapon:
                EquipWeaponItem(item);
                break;

            case ItemType.Ammo:
                Debug.Log("Ammo use not implemented yet.");
                break;

            case ItemType.Healing:
                UseHealingItem(item);
                break;
        }
    }

    void UseHealingItem(InventoryItem item)
    {
        if (playerHealth == null || item == null || item.itemData == null)
        {
            return;
        }

        if (item.itemData.healAmount <= 0)
        {
            return;
        }

        if (playerHealth.currentHealth >= playerHealth.maxHealth)
        {
            Debug.Log("Player is already at max health.");
            return;
        }

        playerHealth.Heal(item.itemData.healAmount);

        if (item.amount > 1)
        {
            item.amount--;
            inventory.MarkDirty();
        }
        else
        {
            inventory.RemoveItem(item);
        }
    }

    void EquipWeaponItem(InventoryItem item)
    {
        if (activeWeapon == null)
        {
            Debug.LogWarning("InventoryUI has no ActiveWeapon assigned.");
            return;
        }

        if (item.itemData == null || item.itemData.weaponData == null)
        {
            Debug.LogWarning("Weapon item has no WeaponData assigned.");
            return;
        }

        WeaponData weaponData = item.itemData.weaponData;

        if (weaponData.weaponPrefab == null)
        {
            Debug.LogWarning("WeaponData has no weaponPrefab assigned for " + weaponData.weaponName);
            return;
        }

        RayCastWeapon newWeapon = Instantiate(
            weaponData.weaponPrefab,
            activeWeapon.transform.position,
            activeWeapon.transform.rotation
        );

        activeWeapon.Equip(newWeapon);

        Debug.Log("Equipped " + weaponData.weaponName + " into " + newWeapon.weaponSlot + " slot.");
    }

    void BuildGrid()
    {
        if (builtGrid || inventory == null || cellParent == null || cellPrefab == null)
        {
            return;
        }

        for (int y = 0; y < inventory.gridHeight; y++)
        {
            for (int x = 0; x < inventory.gridWidth; x++)
            {
                GameObject cell = Instantiate(cellPrefab, cellParent);
                RectTransform rect = cell.GetComponent<RectTransform>();

                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = GetCellPosition(new Vector2Int(x, y));
            }
        }

        builtGrid = true;
    }

    public void RefreshUI()
    {
        if (inventory == null || itemParent == null || itemPrefab == null)
        {
            return;
        }

        for (int i = itemParent.childCount - 1; i >= 0; i--)
        {
            Destroy(itemParent.GetChild(i).gameObject);
        }

        if (dragParent != null)
        {
            for (int i = dragParent.childCount - 1; i >= 0; i--)
            {
                if (dragParent.GetChild(i).GetComponent<InventoryItemUI>() != null)
                {
                    Destroy(dragParent.GetChild(i).gameObject);
                }
            }
        }

        for (int i = 0; i < inventory.items.Count; i++)
        {
            InventoryItem item = inventory.items[i];

            GameObject itemObj = Instantiate(itemPrefab, itemParent);
            RectTransform rect = itemObj.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(
                item.Width * cellSize + (item.Width - 1) * spacing,
                item.Height * cellSize + (item.Height - 1) * spacing
            );
            rect.anchoredPosition = GetCellPosition(item.position);

            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(this, item);
            }
        }
    }

    public Vector2 GetCellPosition(Vector2Int gridPos)
    {
        float x = gridPos.x * (cellSize + spacing);
        float y = -gridPos.y * (cellSize + spacing);
        return new Vector2(x, y);
    }

    public void TryDropItem(InventoryItem item, Vector2 pointerScreenPosition, Vector2Int grabbedCellOffset, Camera eventCamera)
    {
        if (inventory == null || item == null)
        {
            RefreshUI();
            return;
        }

        if (TryGetGridPositionFromScreenPoint(pointerScreenPosition, eventCamera, out Vector2Int hoveredCell))
        {
            Vector2Int targetTopLeft = hoveredCell - grabbedCellOffset;
            inventory.TryMoveItem(item, targetTopLeft);
        }

        RefreshUI();
    }

    bool TryGetGridPositionFromScreenPoint(Vector2 screenPosition, Camera eventCamera, out Vector2Int gridPos)
    {
        gridPos = Vector2Int.zero;

        if (cellParent == null)
        {
            return false;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(cellParent, screenPosition, eventCamera))
        {
            return false;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(cellParent, screenPosition, eventCamera, out Vector2 localPoint);

        float fullCellSize = cellSize + spacing;
        int x = Mathf.FloorToInt(localPoint.x / fullCellSize);
        int y = Mathf.FloorToInt(-localPoint.y / fullCellSize);

        gridPos = new Vector2Int(x, y);
        return true;
    }

    public void RotateItem(InventoryItem item)
    {
        if (inventory == null || item == null)
        {
            return;
        }

        inventory.TryRotateItem(item);
        RefreshUI();
    }

    void ResizeGridParents()
    {
        if (inventory == null)
        {
            return;
        }

        float width = inventory.gridWidth * cellSize + (inventory.gridWidth - 1) * spacing;
        float height = inventory.gridHeight * cellSize + (inventory.gridHeight - 1) * spacing;

        if (cellParent != null)
        {
            cellParent.anchorMin = new Vector2(0f, 1f);
            cellParent.anchorMax = new Vector2(0f, 1f);
            cellParent.pivot = new Vector2(0f, 1f);
            cellParent.sizeDelta = new Vector2(width, height);
        }

        if (itemParent != null)
        {
            itemParent.anchorMin = new Vector2(0f, 1f);
            itemParent.anchorMax = new Vector2(0f, 1f);
            itemParent.pivot = new Vector2(0f, 1f);
            itemParent.sizeDelta = new Vector2(width, height);
        }
    }
}
