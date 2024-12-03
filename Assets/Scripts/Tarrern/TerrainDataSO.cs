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
    [Tooltip("噪声缩放比例")] public float noiseScale = 0.01f;
    [Tooltip("噪声层数")] public int octaves = 4;
    [Tooltip("噪声衰减因子")] public float persistence = 0.5f;
    // [Tooltip("噪声频率增长速率")] public float lacunarity = 2.0f;

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

