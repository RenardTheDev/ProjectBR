using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    public int slotID = 0;

    [Header("Name plate")]
    public Text nameLabel;
    public RectTransform namePlateRect;

    [Header("Ammo plate")]
    public Text ammoLabel;
    public RectTransform ammoPlateRect;

    [Header("Icons")]
    public Image icon;
    public Image mark;
    public Image select;

    ActorEquipment eqp;
    WeaponSlot slot;

    bool toggle = false;

    public Color[] selColors;

    private void Awake()
    {
        ToggleSlotUI(false);
    }

    public void AssignActor(ActorEquipment eqp)
    {
        this.eqp = eqp;
        slot = eqp.slots[slotID];
    }

    private void Update()
    {
        if (InventoryUI.current.isActive()) UpdateSlotPlate();
    }

    void ToggleSlotUI(bool state)
    {
        if (toggle != state)
        {
            namePlateRect.gameObject.SetActive(state);
            ammoPlateRect.gameObject.SetActive(state);
            icon.gameObject.SetActive(state);
            mark.gameObject.SetActive(state);

            toggle = state;
        }
    }

    public void UpdateSlotPlate()
    {
        if (slot.isEmpty)
        {
            ToggleSlotUI(false);
        }
        else
        {
            ToggleSlotUI(true);

            nameLabel.text = slot.entity.data.Name;
            nameLabel.rectTransform.sizeDelta = new Vector2(nameLabel.preferredWidth, 32);
            namePlateRect.sizeDelta = new Vector2(nameLabel.preferredWidth + 8, 20);

            ammoLabel.text = slot.entity.clip + "/" + eqp.GetAmmo(slot);
            ammoLabel.rectTransform.sizeDelta = new Vector2(ammoLabel.preferredWidth, 32);
            ammoPlateRect.sizeDelta = new Vector2(ammoLabel.preferredWidth + 8, 20);

            icon.sprite = slot.entity.data.icon;

            if (selColors != null && selColors.Length > 1)
                mark.color = slot.isSelected ? selColors[1] : selColors[0];
        }
    }

    public void ChangeSelection(bool state)
    {
        select.enabled = state;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryUI.current.OnEquipmentSlotClicked(slotID);
    }
}
