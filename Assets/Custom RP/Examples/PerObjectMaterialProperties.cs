using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 每个对象的材质属性
/// </summary>
[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    // 着色器属性的标识
    static int
        baseColorId = Shader.PropertyToID("_BaseColor"),
        cutoffId = Shader.PropertyToID("_Cutoff"),
        metallicId = Shader.PropertyToID("_Metallic"),
        smoothnessId = Shader.PropertyToID("_Smoothness");
    // 材质块
    static MaterialPropertyBlock block;

    [SerializeField]
    Color baseColor = Color.white;
    [SerializeField, Range(0f, 1f)]
    float alphaCutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, alphaCutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }
}
