using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SSELight : MonoBehaviour
{
    public Transform lightTrans;
    public Material effect;
    Camera cam;

    public DepthTextureMode depthTextureMode = DepthTextureMode.None;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, effect);
    }

    private void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        cam.depthTextureMode = depthTextureMode;
        effect.SetVector("_LightDir", cam.transform.InverseTransformVector(-lightTrans.forward));
    }
}
