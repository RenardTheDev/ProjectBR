using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorInventory : MonoBehaviour
{
    Actor actor;
    public InventoryObject inventory;

    public bool generateUniqContainer = true;

    private void Awake()
    {
        actor = GetComponent<Actor>();

        if (generateUniqContainer)
        {
            inventory = new InventoryObject();
        }
    }

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
        inventory.AddItem(_item.item, _item.amount);
        Destroy(_item.gameObject);
    }

    private void OnApplicationQuit()
    {
        inventory.container.Clear();
    }
}
