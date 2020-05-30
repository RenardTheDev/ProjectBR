using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SSELowHealth : MonoBehaviour
{
    public Material effect;
    Camera cam;

    [Range(0,1)]public float power = 0.0f;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effect.SetFloat("_Amount", power);
        Graphics.Blit(source, destination, effect);
    }
}
