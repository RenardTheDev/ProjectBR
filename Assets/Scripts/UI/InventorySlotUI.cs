using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
{
    RectTransform _rt;
    public InventorySlot slot;

    public Text nameLabel;
    public Text amountLabel;
    public Image icon;
    public Image selection;

    public Color[] selColor;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    public void AssignInfo(InventorySlot slot)
    {
        this.slot = slot;
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        nameLabel.text = slot.item.Name;

        amountLabel.enabled = slot.item.canStack;
        selection.enabled = slot.item is WeaponObject;

        if (slot.item.canStack)
        {
            if (!amountLabel.enabled) amountLabel.enabled = true;
            amountLabel.text = slot.amount.ToString("0");
        }
        else
        {
            if (amountLabel.enabled) amountLabel.enabled = false;
        }

        icon.sprite = slot.item.icon;

        UpdateSelection();
    }

    public void UpdateSelection()
    {
        if (slot.item is WeaponObject && slot.equipped)
        {
            if (!selection.enabled) selection.enabled = true;
            selection.color = slot.selected ? selColor[1] : selColor[0];
        }
        else
        {
            if (selection.enabled) selection.enabled = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryUI.current.OnItemClicked(slot);
    }

    public RectTransform rt()
    {
        if (_rt == null)
        {
            _rt = GetComponent<RectTransform>();

        }
        return _rt;
    }
}
