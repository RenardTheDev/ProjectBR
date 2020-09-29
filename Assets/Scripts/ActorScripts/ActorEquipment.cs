using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorEquipment : MonoBehaviour
{
    Actor actor;
    ActorEvents events;
    ActorMotor motor;
    ActorLook look;
    ActorWeapon weapon;
    ActorInventory inv;

    Animator anim;

    public WeaponSlot[] slots;
    public int currSlotID;
    public WeaponSlot currSlot;
    public Transform WeaponHolder;
    public Transform WeaponPivot;
    Transform rh;

    public bool isArmed;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        look = GetComponent<ActorLook>();
        motor = GetComponent<ActorMotor>();
        inv = GetComponent<ActorInventory>();
        weapon = GetComponent<ActorWeapon>();

        anim = GetComponent<Animator>();

        rh = anim.GetBoneTransform(HumanBodyBones.RightHand);
    }

    private void Start()
    {
        currSlot = slots[currSlotID];
    }

    public void ChangeSlot(int id)
    {
        if (id == -1)
        {
            if (isArmed)
            {
                HolsterCurrentWeapon();
            }
            else
            {
                DrawWeaponSlot(currSlot);
            }
        }
        else
        {
            if (id < slots.Length)
            {
                if (id != currSlotID)
                {
                    DrawWeapon(id);
                }
            }
        }
    }

    void DrawWeapon(int slot)
    {
        currSlotID = slot;
        DrawWeaponSlot(slots[currSlotID]);
    }

    void DrawWeaponSlot(int slotID)
    {
        DrawWeaponSlot(slots[slotID]);
    }

    void DrawWeaponSlot(WeaponSlot slot)
    {
        HolsterCurrentWeapon();

        currSlot = slot;

        currSlot.go.SetActive(true);
        currSlot.entity.Draw();

        isArmed = true;

        OnSlotDraw?.Invoke(currSlot);
    }

    void HolsterCurrentWeapon()
    {
        if (isArmed)
        {
            isArmed = false;
            currSlot.entity.Holster();
            currSlot.go.SetActive(false);

            OnSlotHolster?.Invoke(currSlot);
        }
    }

    public WeaponSlot AssignWeaponToSlot(int slot, WeaponDATA data)
    {
        if (slots[slot].entity != null)
        {
            slots[slot].ClearWeapon();
        }

        var wEnt = EquipmentManager.current.SpawnEQP(data, WeaponHolder);

        wEnt.AssignHandler(actor, motor, weapon, this, events);
        slots[slot].AssignWeapon(wEnt);

        if (currSlot == slots[slot] && !isArmed)
        {
            DrawWeaponSlot(currSlot);
        }
        else
        {
            slots[slot].go.SetActive(false);
            wEnt.Holster();
        }

        return slots[slot];
    }

    public int GetCurrentAmmo()
    {
        return GetAmmo(currSlot);
    }

    public int GetAmmo(WeaponSlot slot)
    {
        if (slot.caliber != null)
        {
            return inv.inventory.GetQuantity(slot.caliber.invItem);
        }
        else
        {
            return 0;
        }
    }

    public void RemoveCurrentAmmo(int amount)
    {
        inv.inventory.RemoveItem(currSlot.caliber.invItem, amount);
    }

    public event Action<WeaponSlot> OnSlotHolster;
    public event Action<WeaponSlot> OnSlotDraw;
}

[System.Serializable]
public class WeaponSlot
{
    public GameObject go;
    public WeaponEntity entity;

    public bool isSelected;

    //--- shortcuts ---
    public int clip { get => entity.clip; }
    public WeaponDATA data { get => entity.data; }
    public AmmoDATA caliber { get => entity.data.ammoType; }
    public bool chambered { get => entity.isChambered; }
    public bool isEmpty { get => entity == null; }

    public void AssignWeapon(WeaponEntity newEntity)
    {
        entity = newEntity;
        go = entity.gameObject;
    }

    public void ClearWeapon()
    {
        entity = null;
        EquipmentManager.current.HideEQP(go);
    }
}