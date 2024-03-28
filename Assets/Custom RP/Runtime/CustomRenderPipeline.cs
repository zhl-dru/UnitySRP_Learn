using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 自定义渲染管线实例
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();
    bool useDynamicBatching, useGPUInstancing;

    /*
     * 由于相机数组参数需要在每帧中分配内存
     * 新的Render将传入参数中的相机数组替换为列表
     * 由于兼容性关系保留一个空实现
     */

    [Obsolete]
#pragma warning disable CS0809 // 过时成员重写未过时成员
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
#pragma warning restore CS0809 // 过时成员重写未过时成员
    {

    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            renderer.Render(context, cameras[i], useDynamicBatching, useGPUInstancing);
        }
    }

    public CustomRenderPipeline(
        bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher
    )
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        // 启用SRP批处理
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        // 启用Linear颜色空间
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
}
