using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorEquipment : MonoBehaviour
{
    Actor actor;
    ActorEvents events;
    ActorMotor motor;
    ActorLook look;
    Animator anim;
    ActorInventory inv;

    public WeaponSlot[] slots;
    public WeaponSlot currSlot;
    public Transform WeaponHolder;

    public bool isArmed;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        look = GetComponent<ActorLook>();
        motor = GetComponent<ActorMotor>();
        anim = GetComponent<Animator>();
        inv = GetComponent<ActorInventory>();
    }

    private void Start()
    {
        
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
                DrawCurrentWeapon();
            }
        }
        else
        {
            if (id < slots.Length)
            {
                if (isArmed) HolsterCurrentWeapon();
                DrawWeapon(id);
            }
        }
    }

    void DrawCurrentWeapon()
    {

    }

    void DrawWeapon(int slot)
    {
        currSlot = slots[slot];
    }

    void HolsterCurrentWeapon()
    {

    }

    public WeaponSlot AssignWeaponToSlot(int slot, WeaponDATA data)
    {
        if (slots[slot].entity != null)
        {
            Destroy(slots[slot].entity.gameObject);

            slots[slot].ClearWeapon();
        }

        var go = Instantiate(data.prefab);
        slots[slot].AssignWeapon(go.GetComponent<WeaponEntity>());

        go.transform.parent = WeaponHolder;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        return slots[slot];
    }

    public int GetCurrentAmmo()
    {
        if (currSlot.caliber != null)
        {
            return inv.inventory.GetQuantity(currSlot.caliber.invItem);
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
}

[System.Serializable]
public class WeaponSlot
{
    public bool isSelected;
    public WeaponEntity entity;

    //public int clip;

    //--- links ---
    public AmmoDATA caliber;

    public bool IsEmpty()
    {
        return entity == null;
    }

    public void AssignWeapon(WeaponEntity newEntity)
    {
        entity = newEntity;
        caliber = entity.data.ammoType;
    }

    public void ClearWeapon()
    {
        entity = null;
        caliber = null;
    }
}