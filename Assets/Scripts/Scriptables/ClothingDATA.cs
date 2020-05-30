using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "rags", menuName = "Scriptables/Clothing")]
public class ClothingDATA : ScriptableObject
{
    public string ClothName;

    public SkinnedMeshRenderer originalSkin;
    public ClothingType type;

    public ClothingDATA arms;
}

public enum ClothingType
{
    essentials,
    feet,
    legs,
    torso,
    arms,
    head,
    hair,
    hat
}