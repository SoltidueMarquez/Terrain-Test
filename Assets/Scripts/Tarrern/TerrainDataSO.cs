using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainDataSo : ScriptableObject
{
    [Header("分辨率设置")]
    [Tooltip("高度图分辨率")] public int heightmapResolution = 513;
    [Tooltip("混合贴图分辨率")] public int alphamapResolution = 512;
    [Tooltip("细节图块的分辨率")] public int detailResolution = 16;
    [Tooltip("细节图块的分辨率")] public Vector3 dataSize = new Vector3(1000, 200, 1000);

    [Header("高度图/地形设置")]
    public List<NoiseLayer> noiseLayers = new List<NoiseLayer>();
    
    [Header("地形纹理参数")]
    public TerrainTextureLayer[] textureLayers;
    
    [Header("植物参数")]
    public Plant[] plants;
    [Tooltip("草密度")] public int grassDensity = 5;

}

[System.Serializable]
public class Plant
{
    public GameObject prefab;         // 植物预设
    public Vector3[] area;            // 多个圆形区域，使用多个Vector3表示（x, y为圆心坐标，z为半径）
    public Vector2 elevationRange;    // 海拔比例范围（最小比例到最大比例）
    public float density;               // 每单位面积的植物密度
    public Vector2 widthScaleRange;   // 宽度缩放比例范围
    public Vector2 heightScaleRange;  // 高度缩放比例范围
    public float minSlope;
    public float maxSlope;
}

[System.Serializable]
public class TerrainTextureLayer
{
    public Texture2D diffuseTexture;
    public Vector2 tileSize;
    public float minHeight;
    public float maxHeight;
    public float minSlope;
    public float maxSlope;
    public bool useNoise;
    public float noiseScale;
    public float minBlend;
    public float maxBlend;
}

[System.Serializable]
public class NoiseLayer
{
    //public enum NoiseType { Perlin, Simplex, Voronoi }
    public enum NoiseType { Perlin }
    public bool enabled = true;
    public NoiseType type;
    public float scale = 1.0f;
    public float persistence = 0.5f;
    public float frequency = 1.0f;
    public int octaves = 1;

    public float Evaluate(float x, float y)
    {
        float noiseValue = 0f;
        float amplitude = 1f;
        float localFrequency = frequency;

        for (int i = 0; i < octaves; i++)
        {
            switch (type)
            {
                case NoiseType.Perlin:
                    noiseValue += Mathf.PerlinNoise(x * scale * localFrequency, y * scale * localFrequency) * amplitude;
                    break;
                //case NoiseType.Simplex:
                    // Simplex noise implementation goes here
                    break;
                //case NoiseType.Voronoi:
                    // Voronoi noise implementation goes here
                    break;
            }
            amplitude *= persistence;
            localFrequency *= 2;
        }

        return noiseValue;
    }
}

