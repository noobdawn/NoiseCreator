using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public class PerlinNoise2D : NoiseCreator2D
{
    public override string name => "2D Perlin Noise";
    [Range(1, 256)]
    [LabelText("周期")]
    public float period;
    [LabelText("随机数种子（用于创建随机晶格方向）")]
    public int seed;
    [LabelText("分形")]
    public bool isFractal;
    [LabelText("无缝")]
    public bool isSeamless;
    [Range(1, 8), ShowIf("isFractal")]
    [LabelText("倍频")]
    public int octaves;
    [Range(0, 1), ShowIf("isFractal")]
    [LabelText("持续度")]
    public float persistence;

    public float2[] randomGrads;

    /// <summary>
    /// 初始化随机梯度，用种子洗牌
    /// </summary>
    /// <param name="seed"></param>
    private void InitGradArray(int seed)
    {
        randomGrads = new float2[256];
        List<float2> grads = new List<float2>();
        for (int i = 0; i < 256; i++)
        {
            float rad = (i + seed) * 2f * Mathf.PI / 255f;
            grads.Add(float2(Mathf.Cos(rad), Mathf.Sin(rad)));
        }
        randomGrads = grads.ToArray();
        //UnityEngine.Random.InitState(seed);
        //for (int i = 0; i < 256; i++)
        //{
        //    int idx = UnityEngine.Random.Range(0, grads.Count);
        //    float2 t = grads[idx];
        //    grads.RemoveAt(idx);
        //    randomGrads[i] = t;
        //}
    }

    public override Color[] GenerateColorData()
    {
        Color[] colors = new Color[width * width];
        InitGradArray(seed);
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                float r = 
                    isFractal ?
                    (FractalPerlinNoise1D(j / (float)(width - 1), i / (float)(width - 1)) * 0.5f + 0.5f) :
                    (PerlinNoise1D(j / (float)(width - 1), i / (float)(width - 1)) * 0.5f + 0.5f);
                colors[j + i * width] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private float2 randomGrad(float2 p)
    {
        if (!isSeamless)
        {
            var i = _NS.random2(p);
            float r = i.x - Mathf.Floor(i.x);
            r *= randomGrads.Length;
            return randomGrads[Mathf.FloorToInt(r)];
        }
        else
        {
            float4 np = float4(
                cos(p.x * 2 * PI / period),
                sin(p.x * 2 * PI / period),
                cos(p.y * 2 * PI / period),
                sin(p.y * 2 * PI / period));
            var i = _NS.random4(np);
            float r = i.x - Mathf.Floor(i.x);
            r *= randomGrads.Length;
            return randomGrads[Mathf.FloorToInt(r)];
        }
    }
    /// <summary>
    /// 柏林噪音
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    private float PerlinNoise1D(float x, float y)
    {
        float2 p = float2(x * period, y * period);
        // 整理出晶胞位置
        float2 p_int0 = floor(p);
        float2 p_int1 = p_int0 + 1;        
        // 整理出小数位
        float2 p_f0 = p - p_int0;
        float2 p_f1 = p - p_int1;
        float2 v00 = p_f0;
        float2 v11 = p_f1;
        float2 v01 = float2(p_f0.x, p_f1.y);
        float2 v10 = float2(p_f1.x, p_f0.y);
        float _d00 = dot(v00, randomGrad(p_int0));
        float _d11 = dot(v11, randomGrad(p_int1));
        float _d10 = dot(v10, randomGrad(float2(p_int1.x, p_int0.y)));
        float _d01 = dot(v01, randomGrad(float2(p_int0.x, p_int1.y)));
        float _dx0 = _NS.plerp(_d00, _d10, p_f0.x);
        float _dx1 = _NS.plerp(_d01, _d11, p_f0.x);
        float _dxy = _NS.plerp(_dx0, _dx1, p_f0.y);
        return _dxy;
    }

    private float FractalPerlinNoise1D(float x, float y)
    {
        var total = 0f;
        if (!isSeamless)
        {
            for (int i = 0; i < octaves; i++)
            {
                var frequency = Mathf.Pow(2, i);
                var amplitude = Mathf.Pow(persistence, i);
                total += PerlinNoise1D(x * frequency, y * frequency) * amplitude;
            }
        }
        else
        {
            var operiod = period;
            for (int i = 0; i < octaves; i++)
            {
                var amplitude = Mathf.Pow(persistence, i);
                total += PerlinNoise1D(x, y) * amplitude;
                period = period * 2;
            }
            period = operiod;
        }
        return total;
    }
}