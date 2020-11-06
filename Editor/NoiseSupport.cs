using System.IO;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public static class _NS
{
    public static float dot5(float[] p1, float[] p2)
    {
        float sum = 0;
        for (int i = 0; i < 5; i++)
        {
            sum += p1[i] * p2[i];
        }
        return sum;
    }

    public static float[] random5(float[] p)
    {
        float[] result = new float[5];
        float[] l1 = new float[]{ 114.5f, 141.9f, 198.1f, 175.5f, 215f };
        float[] l2 = new float[] { 364.3f, 648.8f, 946.4f, 431.7f, 878.6f };
        float[] l3 = new float[] { 190.3f, 233.5f, 716.9f, 362.0f, 723.9f };
        float[] l4 = new float[] { 273.1f, 558.4f, 113.5f, 285.4f, 516.7f };
        float[] l5 = new float[] { 798.4f, 268.1f, 315.6f, 175.5f, 667.1f };

        result[0] = frac(sin(dot5(p, l1)) * 643.1f);
        result[1] = frac(sin(dot5(p, l2)) * 643.1f);
        result[2] = frac(sin(dot5(p, l3)) * 643.1f);
        result[3] = frac(sin(dot5(p, l4)) * 643.1f);
        result[4] = frac(sin(dot5(p, l5)) * 643.1f);
        return result;
    }

    public static float4 random4(float4 p)
    {
        return frac(sin(float4(
            dot(p, float4(114.5f, 141.9f, 198.1f, 175.5f)),
            dot(p, float4(364.3f, 648.8f, 946.4f, 431.7f)),
            dot(p, float4(190.3f, 233.5f, 716.9f, 362.0f)),
            dot(p, float4(273.1f, 558.4f, 113.05f, 285.4f))
            )) * 643.1f);
    }

    public static float3 random3(float3 p)
    {
        return frac(sin(float3(
            dot(p, float3(114.5f, 141.9f, 198.1f)),
            dot(p, float3(364.3f, 648.8f, 946.4f)),
            dot(p, float3(190.3f, 233.5f, 716.9f))
            )) * 643.1f);
    }

    public static float2 random2(float2 p)
    {
        return frac(sin(float2(
            dot(p, float2(114.5f, 141.9f)),
            dot(p, float2(364.3f, 648.8f))
            )) * 643.1f);
    }

    public static float random1(float p)
    {
        return frac(sin(p * 114.514f + 1919.810f) * 643.1f);
    }

    public static float Random1D()
    {
        return UnityEngine.Random.Range(0f, 1f);
    }

    /// <summary>
    /// 余弦插值函数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static float CosInterpolate(float a, float b, float x)
    {
        float ft = x * Mathf.PI;
        float f = (1 - Mathf.Cos(ft)) * 0.5f;
        return a * (1 - f) + b * f;
    }

    /// <summary>
    /// 缓和曲线插值，用的是传统的方法
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="x">比例，需在0~1之间</param>
    /// <returns></returns>
    public static float EaseCurveInterpolate(float a, float b, float x)
    {
        if (x >= 1) return b;
        if (x <= 0) return a;
        return a + (b - a) * ((3 - 2 * x) * x * x);
    }

    public static float plerp(float a, float b, float t)
    {
        float t3 = t * t * t;
        float finalT = 6 * t3 * t * t - 15 * t3 * t + 10 * t3;
        return a * (1 - finalT) + b * finalT;
    }

    /// <summary>
    /// 创建成文件
    /// </summary>
    /// <param name="colors"></param>
    public static void Create2D(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        FileStream fs = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(bytes);
        fs.Close();
        bw.Close();
    }
}
