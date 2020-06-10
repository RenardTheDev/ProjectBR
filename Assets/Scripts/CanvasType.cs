using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasType : MonoBehaviour
{
    public Canvas canvas;
    public Color OverlayColor;

    private void OnValidate()
    {
        canvas = GetComponent<Canvas>();
    }
}