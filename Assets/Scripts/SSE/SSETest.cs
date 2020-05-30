using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SSETest : MonoBehaviour
{
    public Material effect;
    Camera cam;

    public DepthTextureMode depthTextureMode = DepthTextureMode.None;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effect.SetMatrix("_ViewProjection", cam.projectionMatrix);
        effect.SetMatrix("_InverseViewProjection", cam.projectionMatrix.inverse);
        Graphics.Blit(source, destination, effect);
    }

    private void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        cam.depthTextureMode = depthTextureMode;
    }
}
