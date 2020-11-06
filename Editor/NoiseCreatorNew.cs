using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using static Unity.Mathematics.math;
using System.IO;
using System;

interface INoiseCreator
{
    string name { get; }
}

public abstract class NoiseCreator2D : INoiseCreator
{
    [Range(4, 2048)]
    [LabelText("尺寸")]
    public int width;

    public abstract string name { get; }

    public abstract Color[] GenerateColorData();

    [Button("创建噪声")]
    public virtual void CreateNoise()
    {
        Texture2D res = new Texture2D(width, width, TextureFormat.ARGB32, false);
        res.SetPixels(GenerateColorData());
        res.Apply();
        tex = res;
        NoiseCreatorWindow.CreateTexture(tex, name);
    }

    [VerticalGroup, InlineEditor(InlineEditorModes.LargePreview), ShowInInspector]
    public Texture2D tex;
}

public abstract class NoiseCreator3D : INoiseCreator
{
    [Range(4, 2048)]
    [LabelText("XZ平面尺寸")]
    public int width;

    [Range(1, 2048)]
    [LabelText("Y轴高度")]
    public int height;

    public abstract string name { get; }

    public abstract Color[] GenerateColorData(int y);

    [Button("测试创建噪声（y=0）")]
    public virtual void TestNoise()
    {
        Texture2D res = new Texture2D(width, width, TextureFormat.ARGB32, false);
        res.SetPixels(GenerateColorData(0));
        res.Apply();
        tex = res;
    }

    [Button("创建噪声")]
    public virtual void CreateTextures()
    {
        Texture2D t = new Texture2D(width, width, TextureFormat.ARGB32, false);
        for (int i = 0; i < height; i++)
        {
            EditorUtility.DisplayProgressBar("", "", i / (float)height);
            t.SetPixels(GenerateColorData(i));
            t.Apply();
            NoiseCreatorWindow.CreateTextures(t, name, i, height);
        }
        EditorUtility.ClearProgressBar();
    }

    [VerticalGroup, InlineEditor(InlineEditorModes.LargePreview), ShowInInspector]
    public Texture2D tex;
}



public class NoiseCreatorWindow : OdinMenuEditorWindow
{
    private readonly List<INoiseCreator> creators = new List<INoiseCreator>()
    {
        new WhiteNoise2D(),
        new PerlinNoise2D(),
        new ValueNoise2D(),
        new WorleyNoise2D(),
        // Custom Noise
        new CloudNoise(),
    };

    [MenuItem("Tools/Noise Create New")]
    private static void OpenWindow()
    {
        var window = GetWindow<NoiseCreatorWindow>();
        var rect = GUIHelper.GetEditorWindowRect();
        rect.width = 400;
        rect.height = 600;
        window.position = rect;
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(supportsMultiSelect: false);
        tree.Add("Setting", new NoiseSetting());
        creators.ForEach((c) => { tree.Add(c.name, c); });
        return tree;
    }

    public static void CreateTexture(Texture2D tex, string name)
    {
        if (NoiseSetting.isSave)
            _NS.Create2D(tex, Path.Combine(NoiseSetting.SingleTexturePath, name + ".png"));
        AssetDatabase.Refresh();
    }

    internal static void CreateTextures(Texture2D tex, string name, int y, int height)
    {
        _NS.Create2D(tex, Path.Combine(NoiseSetting.SingleTexturePath, string.Format("{0}_{2}-{1}.png", name, y, height)));
        if (y >= height - 1)
            AssetDatabase.Refresh();
    }
}

public class NoiseSetting
{
    public static string SingleTexturePath = "Resources";
    [OnValueChanged("PathChange"), LabelText("输出目标文件夹路径")]
    public string _SingleTexturePath = "Resources";

    public static bool isSave = true;
    [OnValueChanged("PathChange"), LabelText("是否输出文件")]
    public bool _IsSave = true;

    private void PathChange()
    {
        SingleTexturePath = Path.Combine(Application.dataPath, _SingleTexturePath);
        isSave = _IsSave;
    }
}