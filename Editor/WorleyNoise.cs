using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public class WorleyNoise2D : NoiseCreator2D
{
    public override string name => "2D Worley Noise";
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
    [LabelText("随机强度"), Range(0, 1)]
    public float randomStrength;

    public override Color[] GenerateColorData()
    {
        Color[] colors = new Color[width * width];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                float r =
                    isFractal ?
                    (FractalWorleyNoise1D(j / (float)(width - 1), i / (float)(width - 1))) :
                    (WorleyNoise1D(j / (float)(width - 1), i / (float)(width - 1)));
                colors[j + i * width] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private float2 GetRandomVector(float2 p)
    {
        if (!isSeamless)
            return _NS.random2(p);
        float4 np = float4(
                cos(p.x * 2 * PI / period),
                sin(p.x * 2 * PI / period),
                cos(p.y * 2 * PI / period),
                sin(p.y * 2 * PI / period));
        return _NS.random4(np).xy;
    }

    public float WorleyNoise1D(float x, float y)
    {
        float2 p = float2(x, y) * period;
        float2 p_int0 = floor(p);
        float minDist = period * 3f;
        for (int m = -1; m <= 2; m++)
            for (int n = -1; n <= 2; n++)
            {
                float2 np_int0 = p_int0 + float2(m, n);
                np_int0 += (GetRandomVector(np_int0) * 2 - 1) * randomStrength;
                float dist = distance(np_int0, p);
                if (dist < minDist)
                    minDist = dist;
            }
        return 1 - minDist;
    }

    public float FractalWorleyNoise1D(float x, float y)
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
                total += WorleyNoise1D(x * frequency, y * frequency) * amplitude;
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
                total += WorleyNoise1D(x, y) * amplitude;
                period = period * 2;
                t += amplitude;
            }
            total /= t;
            period = operiod;
        }
        return total;
    }
}
