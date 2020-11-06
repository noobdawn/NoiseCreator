using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public class ValueNoise2D : NoiseCreator2D
{
    public override string name => "2D Value Noise";
    [Range(1, 256)]
    [LabelText("周期")]
    public float period;
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

    public override Color[] GenerateColorData()
    {
        Color[] colors = new Color[width * width];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                float r =
                    isFractal ?
                    (FractalValueNoise1D(j / (float)(width - 1), i / (float)(width - 1))) :
                    (ValueNoise1D(j / (float)(width - 1), i / (float)(width - 1)));
                colors[j + i * width] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private float GetRandomValue(float2 p)
    {
        if (!isSeamless)
            return _NS.random2(p).x;
        float4 np = float4(
                cos(p.x * 2 * PI / period),
                sin(p.x * 2 * PI / period),
                cos(p.y * 2 * PI / period),
                sin(p.y * 2 * PI / period));
        return _NS.random4(np).x;
    }

    private float ValueNoise1D(float x, float y)
    {
        float2 p = float2(x * period, y * period);
        //获取诸方格顶点
        float2 p_int0 = floor(p);
        float2 p_int1 = p_int0 + 1;
        //取各网格顶点的伪随机值
        var d00 = GetRandomValue(p_int0);
        var d11 = GetRandomValue(p_int1);
        var d01 = GetRandomValue(float2(p_int0.x, p_int1.y));
        var d10 = GetRandomValue(float2(p_int1.x, p_int0.y));
        var dx0 = _NS.EaseCurveInterpolate(d00, d10, p.x - p_int0.x);
        var dx1 = _NS.EaseCurveInterpolate(d01, d11, p.x - p_int0.x);
        return _NS.EaseCurveInterpolate(dx0, dx1, p.y - p_int0.y);
    }

    private float FractalValueNoise1D(float x, float y)
    {
        var total = 0f;
        if (!isSeamless)
        {
            float t = 0;
            for (int i = 0; i < octaves; i++)
            {
                var frequency = Mathf.Pow(2, i);
                var amplitude = Mathf.Pow(persistence, i);
                t += amplitude;
                total += ValueNoise1D(x * frequency, y * frequency) * amplitude;
            }
            total /= t;
        }
        else
        {
            var operiod = period;
            float t = 0;
            for (int i = 0; i < octaves; i++)
            {
                var amplitude = Mathf.Pow(persistence, i);
                total += ValueNoise1D(x, y) * amplitude;
                period = period * 2;
                t += amplitude;
            }
            total /= t;
            period = operiod;
        }
        return total;
    }
}
