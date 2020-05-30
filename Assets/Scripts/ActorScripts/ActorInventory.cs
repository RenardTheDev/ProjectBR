using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorInventory : MonoBehaviour
{
    public InventoryObject inventory;

    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponentInParent<Item>();
        if (item)
        {
            PickUpItem(item);
        }
    }

    public void PickUpItem(Item _item)
    {
        inventory.AddItem(_item.item, 1);
        Destroy(_item.gameObject);
    }

    private void OnApplicationQuit()
    {
        inventory.container.Clear();
    }
}
