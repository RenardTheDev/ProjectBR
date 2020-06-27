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

    ActorEquipment currEqp;
    WeaponSlot currSlot;
    //WeaponDATA currData;

    public Color[] selColors;

    public void AssignActor(ActorEquipment eqp)
    {
        currEqp = eqp;
        AssignSlot(eqp.currSlot);
    }

    public void AssignSlot(WeaponSlot slot)
    {
        currSlot = slot;
        //if (!slot.IsEmpty()) AssignWeapon(slot.entity.data);

        ToggleSlotUI(true);
    }

    /*public void AssignWeapon(WeaponDATA data)
    {
        currData = data;

        namePlateRect.gameObject.SetActive(true);
        ammoPlateRect.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);
        mark.gameObject.SetActive(true);

        UpdateSlotPalte();
    }*/

    public void ClearWeapon()
    {
        currSlot = null;
        //currData = null;

        ToggleSlotUI(false);
    }

    private void Update()
    {
        if (InventoryUI.current.isActive()) UpdateSlotPlate();
    }

    void ToggleSlotUI(bool state)
    {
        namePlateRect.gameObject.SetActive(state);
        ammoPlateRect.gameObject.SetActive(state);
        icon.gameObject.SetActive(state);
        mark.gameObject.SetActive(state);
    }

    public void UpdateSlotPlate()
    {
        if (currSlot.IsEmpty())
        {
            ToggleSlotUI(false);
        }
        else
        {
            ToggleSlotUI(true);

            nameLabel.text = currSlot.entity.data.Name;
            nameLabel.rectTransform.sizeDelta = new Vector2(nameLabel.preferredWidth, 32);
            namePlateRect.sizeDelta = new Vector2(nameLabel.preferredWidth + 8, 20);

            ammoLabel.text = currSlot.entity.clip + "/" + currEqp.GetCurrentAmmo();
            ammoLabel.rectTransform.sizeDelta = new Vector2(ammoLabel.preferredWidth, 32);
            ammoPlateRect.sizeDelta = new Vector2(ammoLabel.preferredWidth + 8, 20);

            icon.sprite = currSlot.entity.data.icon;

            if (selColors != null && selColors.Length > 1)
                mark.color = currSlot.isSelected ? selColors[1] : selColors[0];
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
