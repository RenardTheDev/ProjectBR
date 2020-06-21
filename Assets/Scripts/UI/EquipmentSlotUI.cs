using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Name plate")]
    public Text nameLabel;
    public RectTransform namePlateRect;

    [Header("Ammo plate")]
    public Text ammoLabel;
    public RectTransform ammoPlateRect;

    [Header("Icons")]
    public Image icon;
    public Image selection;

    ActorWeapon currActor;
    WeaponSlot currSlot;
    WeaponDATA currData;

    public Color[] selColors;

    public void AssignActor(ActorWeapon actw)
    {
        currActor = actw;
        AssignSlot(actw.currSlot);
    }

    public void AssignSlot(WeaponSlot slot)
    {
        currSlot = slot;
        AssignWeapon(slot.entity.data);
    }

    public void AssignWeapon(WeaponDATA data)
    {
        currData = data;
        UpdateSlotPalte();
    }

    public void UpdateSlotPalte()
    {
        if (currSlot.IsEmpty())
        {

        }
        else
        {
            nameLabel.text = currData.Name;
            nameLabel.rectTransform.sizeDelta = new Vector2(nameLabel.preferredWidth, 32);
            namePlateRect.sizeDelta = new Vector2(nameLabel.preferredWidth + 8, 20);

            ammoLabel.text = currSlot.entity.clip + "/" + currActor.GetCurrentAmmo();
            ammoLabel.rectTransform.sizeDelta = new Vector2(ammoLabel.preferredWidth, 32);
            ammoPlateRect.sizeDelta = new Vector2(ammoLabel.preferredWidth + 8, 20);

            icon.sprite = currData.icon;

            if (selColors != null && selColors.Length > 1)
                selection.color = currSlot.isSelected ? selColors[1] : selColors[0];
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }
}
