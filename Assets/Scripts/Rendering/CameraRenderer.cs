using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;

    const string bufferName = "Render camera";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    CullingResults cullingResults;

    //static ShaderTagId unlitShaderTagID = new ShaderTagId("SRPDefaultUnlit");

    static ShaderTagId[] unlitShaderTagIDs = {
        new ShaderTagId("SRPDefaultUnlit"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("CustomLit")
    };

    Lighting lighting = new Lighting();

    public bool useDynamicSun;
    public void UpdateDirectionalLight()
    {
        lighting.Setup(context, cullingResults);
    }

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, bool useDynamicSun)
    {
        this.context = context;
        this.camera = camera;
        this.useDynamicSun = useDynamicSun;

        PrepareBuffer();
        PrepareForSceneWindow();

        if (!Cull())
        {
            return;
        }

        Setup();
        if (useDynamicSun) lighting.Setup(context, cullingResults);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor : Color.clear
            );
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //---OPAQUE---

        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        //var drawingSettings = new DrawingSettings(unlitShaderTagID, sortingSettings);
        var drawingSettings = new DrawingSettings(unlitShaderTagIDs[0], sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        for (int i = 0; i < unlitShaderTagIDs.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, unlitShaderTagIDs[i]);
        }
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        //---SKYBOX---

        //context.DrawSkybox(camera);

        //---TRANSPARENT---

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}