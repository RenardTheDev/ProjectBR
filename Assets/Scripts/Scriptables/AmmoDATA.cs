using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "caliber", menuName = "Scriptables/Caliber")]
public class AmmoDATA : ScriptableObject
{
    public string Name = "caliber";

    public float maxTravel;

    public Color color;
    public Material boxMat;
}