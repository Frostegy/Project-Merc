using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image backgroundImage;
    public Image iconImage;
    public TMP_Text amountText;

    InventoryUI inventoryUI;
    InventoryItem item;

    CanvasGroup canvasGroup;
    RectTransform rectTransform;

    Vector2 dragOffset;
    Vector2Int grabbedCellOffset;

    public void Setup(InventoryUI ui, InventoryItem inventoryItem)
    {
        inventoryUI = ui;
        item = inventoryItem;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1f, 1f, 1f, 0.2f);
        }

        if (iconImage != null && item != null && item.itemData != null)
        {
            iconImage.sprite = item.itemData.icon;
            iconImage.preserveAspect = true;

            if (item.rotated)
            {
                iconImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
            }
            else
            {
                iconImage.rectTransform.localEulerAngles = Vector3.zero;
            }
        }

        if (amountText != null && item != null && item.itemData != null)
        {
            if (item.itemData.itemType == ItemType.Ammo)
            {
                amountText.text = item.ammoInItem.ToString();
            }
            else if (item.itemData.stackable && item.amount > 1)
            {
                amountText.text = item.amount.ToString();
            }
            else
            {
                amountText.text = "";
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryUI == null || rectTransform == null || item == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float fullCellSize = inventoryUI.cellSize + inventoryUI.spacing;
            int cellX = Mathf.Clamp(Mathf.FloorToInt(localPoint.x / fullCellSize), 0, Mathf.Max(0, item.Width - 1));
            int cellY = Mathf.Clamp(Mathf.FloorToInt(-localPoint.y / fullCellSize), 0, Mathf.Max(0, item.Height - 1));
            grabbedCellOffset = new Vector2Int(cellX, cellY);
        }
        else
        {
            grabbedCellOffset = Vector2Int.zero;
        }

        transform.SetParent(inventoryUI.dragParent, true);
        transform.SetAsLastSibling();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.85f;
        }

        dragOffset = (Vector2)rectTransform.position - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.position = eventData.position + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        if (inventoryUI != null)
        {
            inventoryUI.TryDropItem(item, eventData.position, grabbedCellOffset, eventData.pressEventCamera);
        }

        Destroy(gameObject);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI == null || item == null)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryUI.RotateItem(item);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2)
        {
            inventoryUI.UseItem(item);
        }
    }
}