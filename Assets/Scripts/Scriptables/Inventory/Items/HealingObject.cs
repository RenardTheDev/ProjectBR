using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Item", menuName = "Scriptables/Inventory/Healing")]
public class HealingObject : ItemObject
{
    public int value;
    private void Awake()
    {
        type = ItemType.Healing;
    }
}
