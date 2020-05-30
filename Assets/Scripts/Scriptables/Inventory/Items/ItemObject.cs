using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : ScriptableObject
{
    public GameObject prefab_w;

    public ItemType type;
    public float weight;
    public bool canStack;

    [TextArea(15, 20)]
    public string description;
}

public enum ItemType
{
    Default,
    Weapon,
    Ammo,
    Healing
}