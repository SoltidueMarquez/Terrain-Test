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
    
    [Header("树木和植物参数")]
    [Tooltip("树木数量")] public int treeCount = 100;
    [Tooltip("树木生成范围")] public Vector2 treeArea = new Vector2(0.2f, 0.8f);
    [Tooltip("草密度")] public int grassDensity = 5;

}

[System.Serializable]
public class TerrainTextureLayer
{
    public string name;
    public Texture2D diffuseTexture; // 纹理贴图
    public Vector2 tileSize;         // 纹理平铺大小
    public float minBlend;           // 最小混合比例
    public float maxBlend;           // 最大混合比例
}