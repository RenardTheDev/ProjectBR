using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Scriptables/Inventory/Weapon")]
public class WeaponObject : ItemObject
{
    public WeaponDATA weapon;
    public GameObject prefab_eq;
    private void Awake()
    {
        type = ItemType.Weapon;
        prefab_eq = weapon.prefab;
        icon = weapon.icon;
    }

    private void OnValidate()
    {
        prefab_eq = weapon.prefab;
        icon = weapon.icon;
        weapon.icon = icon;
    }
}
