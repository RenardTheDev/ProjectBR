using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Mobile pipeline")]
public class MobileRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    public bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true, useDynamicSun = true;
    [SerializeField]
    public Color sunColor;
    [SerializeField]
    public Vector3 sunDirection;

    protected override RenderPipeline CreatePipeline()
    {
        Shader.SetGlobalColor("_DirectionalLightColor", sunColor);
        Shader.SetGlobalVector("_DirectionalLightDirection", sunDirection);

        Debug.Log("Render pipeline built");

        return new MobileRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, useDynamicSun);
    }
}