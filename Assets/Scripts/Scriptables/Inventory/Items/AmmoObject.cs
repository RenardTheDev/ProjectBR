using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo Item", menuName = "Scriptables/Inventory/Ammo")]
public class AmmoObject : ItemObject
{
    public AmmoDATA ammoType;
    private void Awake()
    {
        type = ItemType.Ammo;
    }
}
