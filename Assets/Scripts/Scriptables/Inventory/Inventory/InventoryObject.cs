using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryObject
{
    public event Action InventoryUpdate;

    public List<InventorySlot> container = new List<InventorySlot>();
    public InventorySlot AddItem(ItemObject _item, int _amount)
    {
        InventorySlot newSlot = null;

        if (_item.canStack)
        {
            bool hasItem = false;
            for (int i = 0; i < container.Count; i++)
            {
                if (container[i].item == _item)
                {
                    container[i].AddAmount(_amount);
                    hasItem = true;
                    newSlot = container[i];
                    break;
                }
            }

            if (!hasItem)
            {
                newSlot = new InventorySlot(_item, _amount);
                container.Add(newSlot);
            }
        }
        else
        {
            newSlot = new InventorySlot(_item, 1);
            container.Add(newSlot);
        }

        InventoryUpdate?.Invoke();
        return newSlot;
    }
    public InventorySlot AddStandaloneItem(ItemObject _item, int _amount)
    {
        InventorySlot newSlot = null;

        newSlot = new InventorySlot(_item, _item.canStack ? _amount : 1);
        container.Add(newSlot);

        InventoryUpdate?.Invoke();
        return newSlot;
    }

    public void RemoveSlot(InventorySlot slot)
    {
        container.Remove(slot);

        InventoryUpdate?.Invoke();
    }

    public void RemoveItem(ItemObject _item, int _amount)
    {
        InventorySlot slot = container.Find(x => x.item == _item);
        if (slot != null)
        {
            slot.RemoveAmount(_amount);
            if (slot.amount <= 0)
            {
                container.Remove(slot);
            }
        }
        else
        {
            Debug.LogError("RemoveItem(\'" + _item.Name + "\', " + _amount + ") cant find that item.");
        }

        InventoryUpdate?.Invoke();
    }

    public bool ContainsSlot(InventorySlot slot)
    {
        return container.Find(x => x == slot) != null;
    }

    public InventorySlot ContainsItem(ItemObject _item)
    {
        InventorySlot slot = container.Find(x => x.item == _item);
        if (slot != null)
        {
            Debug.Log("ContainsItem(\'" + _item.Name + "\') found " + slot.amount + " of that.");
            return slot;
        }
        else
        {
            Debug.LogError("ContainsItem(\'" + _item.Name + "\') cant find that item.");
            return null;
        }
    }

    public int GetQuantity(ItemObject _item)
    {
        int value = 0;

        var item = container.Find(x => x.item == _item);
        if (item!=null)
        {
            value = item.amount;
        }
        else
        {
            //Debug.LogError("GetQuantity(\'" + _item.Name + "\') cant find that item.");
        }

        return value;
    }

    public bool GetSelection(ItemObject _item)
    {
        bool value = false;

        var item = container.Find(x => x.item == _item);
        if (item != null)
        {
            value = item.selected;
        }
        else
        {
            Debug.LogError("GetSelection(\'" + _item.Name + "\') cant find that item.");
        }

        return value;
    }

    public bool GetEquipment(ItemObject _item)
    {
        bool value = false;

        var item = container.Find(x => x.item == _item);
        if (item != null)
        {
            value = item.equipped;
        }
        else
        {
            Debug.LogError("GetEquipment(\'" + _item.Name + "\') cant find that item.");
        }

        return value;
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemObject item;
    public int amount;
    public bool selected;
    public bool equipped;

    public InventorySlot(ItemObject item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }

    public void RemoveAmount(int value)
    {
        amount -= value;
    }

    public void MarkSelection(bool state)
    {
        selected = state && item is WeaponObject;
    }

    public void MarkEquipment(bool state)
    {
        equipped = state && item is WeaponObject;
    }
}