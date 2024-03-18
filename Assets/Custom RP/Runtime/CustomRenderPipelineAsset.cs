using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 自定义渲染管线资产
/// 为 Unity 提供一种方法来获取负责渲染的管线对象实例
/// 资产本身只是一个句柄和存储设置的位置
/// </summary>
[CreateAssetMenu(menuName ="渲染/自定义渲染管线")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}
