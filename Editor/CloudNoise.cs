using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using System;
using System.IO;

public class CloudNoise : NoiseCreator3D
{
    public override string name => "Cloud Noise";

    [Range(1, 256)]
    [LabelText("Perlin周期")]
    public float pperiod = 8;
    [Range(1, 256)]
    [LabelText("Worley周期")]
    public float wperiod = 4;
    [LabelText("Worley距离")]
    public float wdistance = 1;
    [LabelText("Worley幂次")]
    public float wpower = 1;
    [Range(1, 8)]
    [LabelText("倍频")]
    public int octaves = 3;
    [Range(0, 1)]
    [LabelText("持续度")]
    public float persistence = 0.5f;


    [LabelText("随机强度"), Range(0, 1)]
    public float randomStrength;

    float3[] randomGrads = new float3[]{
        int3(-1, -1, 0), int3(-1, 1, 0), int3(1, -1, 0), int3(1, 1, 0),
        int3(-1, 0, -1), int3(-1, 0, 1), int3(1, 0, -1), int3(1, 0, 1),
        int3(0, -1, -1), int3(0, -1, 1), int3(0, 1, -1), int3(0, 1, 1),
        int3(1, 1, 0), int3(-1, 1, 0), int3(0, -1, 1), int3(0, -1, -1)
    };

    private float3 randomGrad5(float3 p)
    {
        float[] np = new float[] {
            cos(p.x * 2 * PI / pperiod),
            sin(p.x * 2 * PI / pperiod),
            cos(p.z * 2 * PI / pperiod),
            sin(p.z * 2 * PI / pperiod),
            p.y
        };
        // 娘的，血坑……
        for (int k = 0; k < 5; k++)
            if (Mathf.Abs(np[k]) <= 0.00001f)
                np[k] = 0;
        var i = _NS.random5(np);
        float r = i[0] - Mathf.Floor(i[0]);
        r *= randomGrads.Length;
        //Debug.LogFormat("{0}, np:{1},{2},{3},{4},{5}, i:{6},{7},{8},{9},{10}",
        //    p,
        //    np[0], np[1], np[2], np[3], np[4],
        //    i[0], i[1], i[2], i[3], i[4]);
        return randomGrads[Mathf.FloorToInt(r)];
    }

    public override Color[] GenerateColorData(int y)
    {
        Color[] colors = new Color[width * width];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                float r = CloudNoise1D(j / (float)(width), y / (float)(width), i / (float)(width));
                colors[j + i * width] = new Color(r, r, r, 1);
            }
        for (int j = 0; j < width; j++)
        {
            var i0 = CloudNoise1D(j / (float)(width), y / (float)(width), 0);
            var i1 = CloudNoise1D(j / (float)(width), y / (float)(width), 1 - 1f / (float)(width));
            if (Mathf.Abs(i0 - i1) >= 0.05f)
            {
                Debug.LogFormat("{0},y={2}",
                    j / (float)(width),
                    j / (float)(width),
                    Mathf.Abs(i0 - i1));
            }
        }
        return colors;
    }

    private float3 GetRandomVector(float3 p)
    {
        float[] np = new float[] {
            cos(p.x * 2 * PI / wperiod),
            sin(p.x * 2 * PI / wperiod),
            cos(p.z * 2 * PI / wperiod),
            sin(p.z * 2 * PI / wperiod),
            p.y
        };
        // 娘的，血坑……
        for (int k = 0; k < 5; k++)
            if (Mathf.Abs(np[k]) <= 0.00001f)
                np[k] = 0;
        var i = _NS.random5(np);
        return float3(i[0], i[1], i[2]);
    }

    //[Button("Test")]
    //void Test()
    //{
    //    Debug.Log(FractalCloudPerlin(0.07f, 0, 0) * 0.5f + 0.5f);
    //    Debug.Log(FractalCloudPerlin(0.07f, 0, 0.01f) * 0.5f + 0.5f);
    //    Debug.Log(FractalCloudPerlin(0.07f, 0, 0.99f) * 0.5f + 0.5f);
    //    Debug.Log(FractalCloudPerlin(0.07f, 0, 1f) * 0.5f + 0.5f);
    //}

    public float CloudWorley(float x, float y, float z)
    {
        float3 p = float3(x, y, z) * wperiod;
        float3 p_int0 = floor(p);
        float minDist = wperiod * 4f;
        for (int m = -1; m <= 2; m++)
            for (int n = -1; n <= 2; n++)
                for (int o = -1; o <= 2; o++)
                {
                    float3 np_int0 = p_int0 + float3(m, n, o);
                    np_int0 += (GetRandomVector(np_int0) * 2 - 1) * randomStrength;
                    float dist = distance(np_int0, p);
                    if (dist < minDist)
                        minDist = dist;
                }
        return wdistance - minDist;
    }

    private float CloudNoise1D(float x, float y, float z)
    {
        var w = CloudWorley(x, y, z);
        var p = FractalCloudPerlin(x, y, z) * 0.5f + 0.5f;
        return wpower == 1 ? w * p : Mathf.Pow(w, wpower) * p;
    }

    private float FractalCloudPerlin(float x, float y, float z)
    {
        var total = 0f;
        var t = 0f;
        var operiod = pperiod;
        for (int i = 0; i < octaves; i++)
        {
            var amplitude = Mathf.Pow(persistence, i);
            total += CloudPerlin(x, y, z) * amplitude;
            pperiod = pperiod * 2;
            t += amplitude;
        }
        total /= t;
        pperiod = operiod;
        return total;
    }

    private float CloudPerlin(float x, float y, float z)
    {
        float3 p = float3(x, y, z) * pperiod;
        // 整理出晶胞位置
        float3 p_int0 = floor(p);
        float3 p_int1 = p_int0 + 1;
        // 整理出小数位
        float3 p_f0 = p - p_int0;
        float3 p_f1 = p - p_int1;

        // 整理距离矢量
        float3 v000 = p_f0;
        float3 v111 = p_f1;
        float3 v001 = float3(p_f0.x, p_f0.y, p_f1.z);
        float3 v010 = float3(p_f0.x, p_f1.y, p_f0.z);
        float3 v100 = float3(p_f1.x, p_f0.y, p_f0.z);
        float3 v011 = float3(p_f0.x, p_f1.y, p_f1.z);
        float3 v101 = float3(p_f1.x, p_f0.y, p_f1.z);
        float3 v110 = float3(p_f1.x, p_f1.y, p_f0.z);
        // 获得点积
        float _d000 = dot(v000, randomGrad5(p_int0));
        float _d111 = dot(v111, randomGrad5(p_int1));
        float _d001 = dot(v001, randomGrad5(float3(p_int0.x, p_int0.y, p_int1.z)));
        float _d010 = dot(v010, randomGrad5(float3(p_int0.x, p_int1.y, p_int0.z)));
        float _d100 = dot(v100, randomGrad5(float3(p_int1.x, p_int0.y, p_int0.z)));
        float _d011 = dot(v011, randomGrad5(float3(p_int0.x, p_int1.y, p_int1.z)));
        float _d101 = dot(v101, randomGrad5(float3(p_int1.x, p_int0.y, p_int1.z)));
        float _d110 = dot(v110, randomGrad5(float3(p_int1.x, p_int1.y, p_int0.z)));
        // 用样条函数捏合
        float _dx00 = _NS.plerp(_d000, _d100, p_f0.x);
        float _dx01 = _NS.plerp(_d001, _d101, p_f0.x);
        float _dx10 = _NS.plerp(_d010, _d110, p_f0.x);
        float _dx11 = _NS.plerp(_d011, _d111, p_f0.x);
        float _dxy0 = _NS.plerp(_dx00, _dx10, p_f0.y);
        float _dxy1 = _NS.plerp(_dx01, _dx11, p_f0.y);
        float _dxyz = _NS.plerp(_dxy0, _dxy1, p_f0.z);
        return _dxyz;
    }

    [Button("创建噪声")]
    public override void CreateTextures()
    {
        Texture2D t = new Texture2D(width, width, TextureFormat.ARGB32, false);
        for (int i = 0; i < height; i++)
        {
            UnityEditor.EditorUtility.DisplayProgressBar("", "", i / (float)height);
            t.SetPixels(GenerateColorData(i));
            t.Apply();
            _NS.Create2D(t, Path.Combine(Application.dataPath, string.Format("Resources\\CloudTex\\{0:D4}.png", i)));
            if (i >= height - 1)
                UnityEditor.AssetDatabase.Refresh();
        }
        UnityEditor.EditorUtility.ClearProgressBar();
    }
}
