using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 相机渲染器
/// 负责每个相机的渲染
/// 场景中的多个相机按先后顺序渲染
/// 因此只需要一个CameraRenderer实例
/// </summary>
public partial class CameraRenderer
{
    static ShaderTagId 
        unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        litShaderTagId = new ShaderTagId("CustomLit");

    // 本次渲染的状态,绘制命令和摄像机的缓存
    // 每次开始渲染时设置
    ScriptableRenderContext context;
    Camera camera;

    // 相机的剔除结果
    CullingResults cullingResults;

    // 缓冲区的默认名称
    const string bufferName = "Render Camera";

    // 用于缓存渲染命令的命令缓冲区
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    Lighting lighting = new Lighting();



    public void Render(
        ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing)
    {
        // 设置相机和渲染状态,命令缓存
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        // 进行剔除,Cull返回false时相机无法渲染,跳出
        if (!Cull())
        {
            return;
        }

        Setup();
        lighting.Setup(context, cullingResults);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    /// <summary>
    /// 绘制可见的几何体
    /// 负责绘制场景中所有可见的网格
    /// </summary>
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        /*
         * 绘制可见对象需要三个设置
         * SortingSettings 用于描述在渲染过程中对对象进行排序的方法
         * DrawingSettings 介绍如何对可见对象进行排序以及要使用的着色器传递
         * FilteringSettings 描述如何对可见对象进行过滤
         */


        // 在绘制天空盒前绘制不透明物体
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId, sortingSettings
            )
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        // 绘制天空盒
        context.DrawSkybox(camera);

        // 在绘制天空盒后绘制透明物体
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }


    /// <summary>
    /// 设置渲染状态
    /// </summary>
    void Setup()
    {
        // 为着色器设置该相机的视图投影矩阵
        context.SetupCameraProperties(camera);
        // 清理渲染渲染目标中的旧内容
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear);
        // 开始调试分析器采样
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 提交渲染命令
    /// </summary>
    void Submit()
    {
        // 结束调试分析器采样
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        // 提交渲染
        context.Submit();
    }

    /// <summary>
    /// 将命令复制给渲染上下文并清理命令缓冲区以便重用
    /// </summary>
    void ExecuteBuffer()
    {
        // 将缓冲区中的命令复制到渲染上下文
        context.ExecuteCommandBuffer(buffer);
        // 清理缓冲区
        buffer.Clear();
    }

    /// <summary>
    /// 剔除
    /// </summary>
    /// <returns>
    /// 如果相机在尝试获取剔除参数时返回了false,
    /// 这个相机无法渲染,因此也返回false,结束渲染
    /// </returns>
    bool Cull()
    {
        // 获取相机的剔除参数,如果成功的话将结果绑定到渲染上下文
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}
