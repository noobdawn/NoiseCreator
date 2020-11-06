using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEditor;

public class NoiseCreator : EditorWindow
{

    private string s_path = @"C:\1.png";
    private int s_width = 128;
    private int s_height = 128;
    private float s_persistence = 0.5f;
    private string str_persistence = "0.5";
    private int s_octaves = 4;
    private bool isSmooth = true;
    private bool isPreviewOnly = true;
    private Vector2[] s_randomGrads;
    private int[] s_randomIndex;
    private int s_seed = 99997;
    private int s_Proportion = 10;
    private Texture2D s_tex = null;
    private int s_maxPoint = 10;
    private float s_worleyAmp = 4f;
    [MenuItem("Tools/Noise Creator")]
    private static void ShowWindow()
    {
        GetWindow<NoiseCreator>();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("宽:", GUILayout.Width(50));
        s_width = int.Parse(GUILayout.TextField(s_width.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("高:", GUILayout.Width(50));
        s_height = int.Parse(GUILayout.TextField(s_height.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("路径:", GUILayout.Width(50));
        s_path = GUILayout.TextField(s_path);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("倍频(分形用):", GUILayout.Width(150));
        s_octaves = int.Parse(GUILayout.TextField(s_octaves.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("持续度(分形用):", GUILayout.Width(150));
        str_persistence = GUILayout.TextField(str_persistence);
        float.TryParse(str_persistence, out s_persistence);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("随机梯度种子(Perlin用):", GUILayout.Width(150));
        s_seed = int.Parse(GUILayout.TextField(s_seed.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("网格比例(Perlin\\Worley用):", GUILayout.Width(150));
        s_Proportion = int.Parse(GUILayout.TextField(s_Proportion.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("随机点数(Worley用):", GUILayout.Width(150));
        s_maxPoint = int.Parse(GUILayout.TextField(s_maxPoint.ToString()));
        GUILayout.Label("随机点数(Worley用):", GUILayout.Width(150));
        s_worleyAmp = float.Parse(GUILayout.TextField(s_worleyAmp.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        isSmooth = GUILayout.Toggle(isSmooth, "光滑(分形用)");
        isPreviewOnly = GUILayout.Toggle(isPreviewOnly, "只预览");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建白噪声纹理"))
        {
            Create2D(WhiteNoise());
        }
        if (GUILayout.Button("创建分形白噪声纹理"))
        {
            Create2D(FractalWhiteNoise(s_width, s_height));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建Perlin噪声纹理"))
        {
            Create2D(PerlinNoise(s_width, s_height));
        }
        if (GUILayout.Button("创建分形Perlin噪声纹理"))
        {
            Create2D(FractalPerlinNoise(s_width, s_height));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建Value噪声纹理"))
        {
            Create2D(ValueNoise(s_width, s_height));
        }
        if (GUILayout.Button("创建分形Value噪声纹理"))
        {
            Create2D(FractalValueNoise(s_width, s_height));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建Simplex噪声纹理"))
        {
            Create2D(SimplexNoise(s_width, s_height));
        }
        if (GUILayout.Button("创建分形Simplex噪声纹理"))
        {
            Create2D(FractalSimplexNoise(s_width, s_height));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建Worley噪声纹理"))
        {
            Create2D(WorleyNoise(s_width, s_height));
        }
        if (GUILayout.Button("创建Perlin-Worley噪声纹理"))
        {
            Create2D(FractalWorleyNoise(s_width, s_height));
        }
        GUILayout.EndHorizontal();
        if (s_tex != null)
        {
            GUILayout.BeginArea(new Rect(0, 280, s_width, s_height), s_tex);
            GUILayout.EndArea();
        }
    }


    #region 不同维度下的基本噪声函数
    private float Random1D()
    {
        return Random.Range(0f, 1f);
    }
    private float Random1D(int px)
    {
        int n = px;
        n = (n << 13) ^ n;
        return (1f - (n * (n * n * 15731 + 789221) + 1376312589 & 0x7fffffff) / 1073741824f);
    }
    private float Random1D(int x, int y)
    {
        int n = 12211 * x + 7549 * y;
        n = (n << 13) ^ n;
        return (1f - (n * (n * n * 15731 + 789221) + 1376312589 & 0x7fffffff) / 1073741824f);
    }
    private Vector2 Random2D(int x, int y)
    {
        Vector2 res = new Vector2();
        int n = 49993 * x + 17 * x * y;
        n = (n << 13) ^ n;
        res.x = (1f - (n * (n * n * 15731 + 789221) + 1376312589 & 0x7fffffff) / 1073741824f);
        n = 35279 * y + 23 * x * y;
        n = (n << 13) ^ n;
        res.y = (1f - (n * (n * n * 15731 + 789221) + 1376312589 & 0x7fffffff) / 1073741824f);
        return res;
    }
    #endregion
    #region 白噪声函数
    /// <summary>
    /// 对2维基本噪声函数进行光滑处理
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float Smooth1D(int x, int y)
    {
        float res = Random1D(x, y) / 4f;
        res += (Random1D(x - 1, y) + Random1D(x + 1, y) + Random1D(x, y - 1) + Random1D(x, y + 1)) / 8f;
        res += (Random1D(x - 1, y - 1) + Random1D(x + 1, y - 1) + Random1D(x - 1, y + 1) + Random1D(x + 1, y + 1)) / 16f;
        return res;
    }

    /// <summary>
    /// 余弦插值函数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private float cosine_interpolate(float a, float b, float x)
    {
        float ft = x * Mathf.PI;
        float f = (1 - Mathf.Cos(ft)) * 0.5f;
        return a * (1 - f) + b * f;
    }
    //插值函数
    private float InterpolatedNoise(float x, float y)
    {
        int ix = Mathf.FloorToInt(x);
        int iy = Mathf.FloorToInt(y);
        float fx = x - ix;
        float fy = y - iy;
        var v1 = isSmooth ? Smooth1D(ix, iy) : Random1D(ix, iy);
        var v2 = isSmooth ? Smooth1D(ix + 1, iy) : Random1D(ix + 1, iy);
        var v3 = isSmooth ? Smooth1D(ix, iy + 1) : Random1D(ix, iy + 1);
        var v4 = isSmooth ? Smooth1D(ix + 1, iy + 1) : Random1D(ix + 1, iy + 1);
        var i1 = cosine_interpolate(v1, v2, fx);
        var i2 = cosine_interpolate(v3, v4, fx);
        return cosine_interpolate(i1, i2, fy);
    }
    /// <summary>
    /// 分形白噪声函数，来源 http://m.blog.csdn.net/mahabharata_/article/details/54743672
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float FractalWhiteNoise1D(float x, float y)
    {
        var total = 0f;
        for (int i = 0; i < s_octaves; i++)
        {
            var frequency = Mathf.Pow(2, i);
            var amplitude = Mathf.Pow(s_persistence, i);
            total += InterpolatedNoise(x * frequency, y * frequency) * amplitude;
        }
        return total;
    }
    #endregion
    #region Perlin函数
    private void InitGradArray(int seed)
    {
        int max = Mathf.Max(s_width, s_height);
        s_randomGrads = new Vector2[max + 1];
        s_randomIndex = new int[max + 1];
        for (int i = 0; i < max + 1; i++)
        {
            s_randomGrads[i] = Random2D(i, seed).normalized;
            s_randomIndex[i] = i;
        }
        //对随机排列表进行洗牌
        int idx = max;
        int temp = 0;
        while (idx > 0)
        {
            int r = Random.Range(0, max);
            temp = s_randomIndex[r];
            s_randomIndex[r] = s_randomIndex[idx];
            s_randomIndex[idx] = temp;
            idx--;
        }
        //for (int i = 0; i < max + 1; i++)
        //{
        //    Debug.Log(s_randomIndex[i]);
        //}
    }

    /// <summary>
    /// 缓和曲线插值，用的是传统的方法
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="x">比例，需在0~1之间</param>
    /// <returns></returns>
    private float EaseCurveInterpolate(float a, float b, float x)
    {
        if (x >= 1) return b;
        if (x <= 0) return a;
        return a + (b - a) * ((3 - 2 * x) * x * x);
    }

    /// <summary>
    /// 柏林噪音，原理来自 http://www.twinklingstar.cn/2015/2581/classical-perlin-noise/
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    private float PerlinNoise1D(float x, float y)
    {
        int max = Mathf.Max(s_width, s_height);
        //获取诸方格顶点
        int x0 = Mathf.FloorToInt(x) % max;
        int x1 = (Mathf.FloorToInt(x) + 1) % max;
        int y0 = Mathf.FloorToInt(y) % max;
        int y1 = (Mathf.FloorToInt(y) + 1) % max;
        Vector2 v00 = new Vector2(x - x0, y - y0);
        Vector2 v10 = new Vector2(x - x1, y - y0);
        Vector2 v01 = new Vector2(x - x0, y - y1);
        Vector2 v11 = new Vector2(x - x1, y - y1);
        var s = Vector2.Dot(s_randomGrads[(x0 + s_randomIndex[y0]) % max], v00);
        var t = Vector2.Dot(s_randomGrads[(x1 + s_randomIndex[y0]) % max], v10);
        var u = Vector2.Dot(s_randomGrads[(x0 + s_randomIndex[y1]) % max], v01);
        var v = Vector2.Dot(s_randomGrads[(x1 + s_randomIndex[y1]) % max], v11);
        var a = s + (t - s) * EaseCurveInterpolate(0, 1, x - x0);
        var b = u + (v - u) * EaseCurveInterpolate(0, 1, x - x0);
        return a + (b - a) * EaseCurveInterpolate(0, 1, y - y0);
    }

    private float FractalPerlinNoise1D(float x, float y)
    {
        var total = 0f;
        for (int i = 0; i < s_octaves; i++)
        {
            var frequency = Mathf.Pow(2, i);
            var amplitude = Mathf.Pow(s_persistence, i);
            total += PerlinNoise1D(x * frequency, y * frequency) * amplitude;
        }
        return total;
    }
    #endregion
    #region Value噪声函数
    private float ValueNoise1D(float x, float y)
    {
        int max = Mathf.Max(s_width, s_height);
        //获取诸方格顶点
        int x0 = Mathf.FloorToInt(x);
        int x1 = x0 + 1;
        int y0 = Mathf.FloorToInt(y);
        int y1 = y0 + 1;
        //取各网格顶点的伪随机值
        var s = Random1D(x0 * (1993 * x0), y0 * (1993 * y0));
        var t = Random1D(x1 * (1993 * x1), y0 * (1993 * y0));
        var u = Random1D(x0 * (1993 * x0), y1 * (1993 * y1));
        var v = Random1D(x1 * (1993 * x1), y1 * (1993 * y1));
        var a = s + (t - s) * EaseCurveInterpolate(0, 1, x - x0);
        var b = u + (v - u) * EaseCurveInterpolate(0, 1, x - x0);
        return a + (b - a) * EaseCurveInterpolate(0, 1, y - y0);
    }

    private float FractalValueNoise1D(float x, float y)
    {
        var total = 0f;
        for (int i = 0; i < s_octaves; i++)
        {
            var frequency = Mathf.Pow(2, i);
            var amplitude = Mathf.Pow(s_persistence, i);
            total += ValueNoise1D(x * frequency, y * frequency) * amplitude;
        }
        return total;
    }
    #endregion
    #region Simplex噪声函数
    /// <summary>
    /// 原理来自http://m.blog.csdn.net/xfgryujk/article/details/64133242
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private float SimplexNoise1D(Vector2 p)
    {
        int max = Mathf.Max(s_width, s_height);
        float K1 = (Mathf.Sqrt(3f) - 1) / 2f; // 0.366025404f; // (sqrt(3)-1)/2;
        float K2 = (3 - Mathf.Sqrt(3f)) / 6f; // 0.211324865f; // (3-sqrt(3))/6;
        //这里的P是2d单形网格里的输入点，而newx、newy是超立方体网格中的0，0点
        int newx = Mathf.FloorToInt(p.x + (p.x + p.y) * K1);
        int newy = Mathf.FloorToInt(p.y + (p.x + p.y) * K1);
        //将超立方体中的00点变换回2d网格中
        float oldx = newx - (newx + newy) * K2;
        float oldy = newy - (newx + newy) * K2;
        //拿到2d网格中，输入点和00点的差
        Vector2 d = new Vector2(p.x - oldx, p.y - oldy);
        //拿到第二个点
        Vector2 secVertice = d.x > d.y ? new Vector2(1, 0) : new Vector2(0, 1);
        //准备好梯度向量表所需要的索引
        int xIndex = newx % max;
        int yIndex = newy % max;
        //s_randomIndex是洗好的随机次序表
        int vIndex1 = s_randomIndex[(xIndex + s_randomIndex[yIndex]) % max];
        int vIndex2 = s_randomIndex[(xIndex + (int)secVertice.x + s_randomIndex[yIndex + (int)secVertice.y]) % max];
        int vIndex3 = s_randomIndex[(xIndex + 1 + s_randomIndex[yIndex + 1]) % max];
        //计算贡献度
        float t = 0.5f - d.sqrMagnitude;
        //s_randomGrads是随机归一化后的随机梯度表
        float res1 = Mathf.Max(0f, t * t * t * t * Vector2.Dot(d, s_randomGrads[vIndex1]));
        Vector2 d2 = new Vector2(
            d.x - secVertice.x + K2,
            d.y - secVertice.y + K2
            );
        t = 0.5f - d2.sqrMagnitude;
        res1 += Mathf.Max(0f, t * t * t * t * Vector2.Dot(d2, s_randomGrads[vIndex2]));
        Vector2 d3 = new Vector2(
            d.x - 1f + 2f * K2,
            d.y - 1f + 2f * K2
            );
        t = 0.5f - d3.sqrMagnitude;
        res1 += Mathf.Max(0f, t * t * t * t * Vector2.Dot(d3, s_randomGrads[vIndex3]));
        return res1 * 70f;
    }

    private float FractalSimplexNoise1D(Vector2 p)
    {
        var total = 0f;
        for (int i = 0; i < s_octaves; i++)
        {
            var frequency = Mathf.Pow(2, i);
            var amplitude = Mathf.Pow(s_persistence, i);
            total += SimplexNoise1D(p) * amplitude;
        }
        return total;
    }
    #endregion
    #region Worley噪声
    ///https://aftbit.com/cell-noise-2/

    private List<Vector2> randomPoint = new List<Vector2>();
    private Vector2 worleySpace = Vector2.one;
    private int CountOfRandomPoint = 0;
    ///准备好空间中的随机点
    private void PrepareForWorley(float xMax, float yMax, int point)
    {
        randomPoint.Clear();
        worleySpace = new Vector2(xMax, yMax);
        CountOfRandomPoint = point;
        for (int i = 0; i < CountOfRandomPoint; i++)
        {
            randomPoint.Add(new Vector2(
                Random.Range(0f, xMax),
                Random.Range(0f, yMax)
                ));
        }
    }

    private float WorleyNoise1D(Vector2 p)
    {
        float res = worleySpace.magnitude;
        foreach (Vector2 rp in randomPoint)
        {
            var distanceRp = (p - rp).magnitude;
            res = distanceRp < res ? distanceRp : res;
        }
        return 1 - 4f * res / worleySpace.magnitude;
    }

    #endregion
    private Color[] WhiteNoise()
    {
        Color[] colors = new Color[s_width * s_height];
        for (int i = 0; i < s_height; i++)
            for (int j = 0; j < s_width; j++)
            {
                float r = (Random1D(j, i) + 1) / 2f;
                colors[j + i * s_width] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] FractalWhiteNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = (FractalWhiteNoise1D(j, i) + 1) / 2f;
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] PerlinNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        InitGradArray(s_seed);
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = (PerlinNoise1D(j / (float)s_Proportion, i / (float)s_Proportion) + 1) / 2f;
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] FractalPerlinNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        InitGradArray(s_seed);
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = (FractalPerlinNoise1D(j / (float)s_Proportion, i / (float)s_Proportion) + 1) / 2f;
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] ValueNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = (ValueNoise1D(j / (float)s_Proportion, i / (float)s_Proportion) + 1) / 2f;
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] FractalValueNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = (FractalValueNoise1D(j / (float)s_Proportion, i / (float)s_Proportion) + 1) / 2f;
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] SimplexNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        InitGradArray(s_seed);
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = SimplexNoise1D(new Vector2(j, i));
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] FractalSimplexNoise(int w, int h)
    {
        Color[] colors = new Color[w * h];
        InitGradArray(s_seed);
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = FractalSimplexNoise1D(new Vector2(j / (float)s_Proportion, i / (float)s_Proportion));
                colors[j + i * w] = new Color(r, r, r, 1);

            }
        return colors;
    }

    private Color[] WorleyNoise(int w, int h)
    {
        PrepareForWorley(w / 100f, h / 100f, s_maxPoint);
        Color[] colors = new Color[w * h];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = WorleyNoise1D(new Vector2(j / 100f, i / 100f));
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private Color[] FractalWorleyNoise(int w, int h)
    {
        PrepareForWorley(w / 100f, h / 100f, s_maxPoint);
        Color[] colors = new Color[w * h];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                float r = WorleyNoise1D(new Vector2(j / 100f, i / 100f)) + (FractalPerlinNoise1D(j / (float)s_Proportion, i / (float)s_Proportion));
                colors[j + i * w] = new Color(r, r, r, 1);
            }
        return colors;
    }

    private void Create2D(Color[] colors)
    {
        s_tex = new Texture2D(s_width, s_height, TextureFormat.ARGB32, false);
        s_tex.SetPixels(colors);
        s_tex.Apply();
        if (isPreviewOnly == false)
        {
            byte[] bytes = s_tex.EncodeToPNG();
            FileStream fs = new FileStream(s_path, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(bytes);
            fs.Close();
            bw.Close();
        }
    }
}