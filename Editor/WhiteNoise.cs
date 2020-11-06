using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public class WhiteNoise2D : NoiseCreator2D
{
    public override string name => "2D White Noise";

    [LabelText("分形")]
    public bool isFractal;
    [Range(1, 8)]
    [LabelText("倍频"), ShowIf("isFractal")]
    public int octaves;
    [Range(0, 1)]
    [LabelText("持续度"), ShowIf("isFractal")]
    public float persistence;
    [LabelText("是否使用光滑后的白噪声"), ShowIf("isFractal")]
    public bool isSmooth;

    public override Color[] GenerateColorData()
    {
        Color[] colors = new Color[width * width];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                float r = isFractal ?
                    (FractalWhiteNoise1D(j, i, isSmooth) + 1) / 2f :
                    (_NS.random2(float2(j, i)).x + 1) / 2f;
                colors[j + i * width] = new Color(r, r, r, 1);
            }
        return colors;
    }

    /// <summary>
    /// 对2维基本噪声函数进行光滑处理
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float Smooth1D(int x, int y)
    {
        float res = _NS.random2(float2(x, y)).x / 4f;
        res += (_NS.random2(float2(x - 1, y)) + _NS.random2(float2(x + 1, y)) + _NS.random2(float2(x, y - 1)) + _NS.random2(float2(x, y + 1))).x / 8f;
        res += (_NS.random2(float2(x - 1, y - 1)) + _NS.random2(float2(x + 1, y - 1)) + _NS.random2(float2(x - 1, y + 1)) + _NS.random2(float2(x + 1, y + 1))).x / 16f;
        return res;
    }

    private float InterpolatedNoise(float x, float y, bool isSmooth)
    {
        int ix = Mathf.FloorToInt(x);
        int iy = Mathf.FloorToInt(y);
        float fx = x - ix;
        float fy = y - iy;
        var v1 = isSmooth ? Smooth1D(ix, iy) : _NS.random2(float2(ix, iy)).x;
        var v2 = isSmooth ? Smooth1D(ix + 1, iy) : _NS.random2(float2(ix + 1, iy)).x;
        var v3 = isSmooth ? Smooth1D(ix, iy + 1) : _NS.random2(float2(ix, iy + 1)).x;
        var v4 = isSmooth ? Smooth1D(ix + 1, iy + 1) : _NS.random2(float2(ix + 1, iy + 1)).x;
        var i1 = _NS.CosInterpolate(v1, v2, fx);
        var i2 = _NS.CosInterpolate(v3, v4, fx);
        return _NS.CosInterpolate(i1, i2, fy);
    }

    /// <summary>
    /// 分形白噪声函数，来源 http://m.blog.csdn.net/mahabharata_/article/details/54743672
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float FractalWhiteNoise1D(float x, float y, bool isSmooth)
    {
        var total = 0f;
        float t = 0;
        for (int i = 0; i < octaves; i++)
        {
            var frequency = Mathf.Pow(2, i);
            var amplitude = Mathf.Pow(persistence, i);
            t += amplitude;
            total += InterpolatedNoise(x * frequency, y * frequency, isSmooth) * amplitude;
        }
        total /= t;
        return total;
    }
}