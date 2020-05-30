using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponModule : ScriptableObject
{
    //public string name;
}

[CreateAssetMenu(fileName = "EndlessAmmo", menuName = "Scriptables/Weapon module/Endless ammo")]
public class module_EndlessAmmo : WeaponModule
{

}

[CreateAssetMenu(fileName = "fireRate", menuName = "Scriptables/Weapon module/Fire rate mod")]
public class module_FireRateMod : WeaponModule
{
    public float value = 1.0f;  //multiplayer

    public module_FireRateMod(float value)
    {
        this.value = value;
    }
}

[CreateAssetMenu(fileName = "clipSize", menuName = "Scriptables/Weapon module/Clip size mod")]
public class module_ClipSizeMod : WeaponModule
{
    public int value = 0;  //addition

    public module_ClipSizeMod(int value)
    {
        this.value = value;
    }
}