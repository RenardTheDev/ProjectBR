using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Inventory", menuName = "Scriptables/Inventory/Container")]
public class InventoryObject : ScriptableObject
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
}