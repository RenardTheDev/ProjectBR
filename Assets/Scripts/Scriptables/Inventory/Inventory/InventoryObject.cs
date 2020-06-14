using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryObject
{
    public List<InventorySlot> container = new List<InventorySlot>();
    public void AddItem(ItemObject _item, int _amount)
    {
        if (_item.canStack)
        {
            bool hasItem = false;
            for (int i = 0; i < container.Count; i++)
            {
                if (container[i].item == _item)
                {
                    container[i].AddAmount(_amount);
                    hasItem = true;
                    break;
                }
            }

            if (!hasItem)
            {
                container.Add(new InventorySlot(_item, _amount));
            }
        }
        else
        {
            container.Add(new InventorySlot(_item, 1));
        }
    }

    public void RemoveItem(ItemObject _item, int _amount)
    {
        for (int i = 0; i < container.Count; i++)
        {
            if (container[i].item == _item)
            {
                container[i].RemoveAmount(_amount);
                if (container[i].amount <= 0)
                {
                    container.RemoveAt(i);
                }
                break;
            }
        }
    }

    public int GetQuantity(ItemObject _item)
    {
        int value = 0;

        for (int i = 0; i < container.Count; i++)
        {
            if (container[i].item == _item)
            {
                value += container[i].amount;
                if (_item.canStack) break;
            }
        }

        return value;
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemObject item;
    public int amount;

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
}