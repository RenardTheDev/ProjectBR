using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Default Item", menuName = "Scriptables/Inventory/Default Item")]
public class DefaultObject : ItemObject
{
    private void Awake()
    {
        type = ItemType.Default;
    }
}
