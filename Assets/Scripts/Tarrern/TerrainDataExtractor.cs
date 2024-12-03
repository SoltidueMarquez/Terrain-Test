using UnityEngine;

/// <summary>
/// 用于输出地形数据参数
/// </summary>
public class TerrainDataExtractor : MonoBehaviour
{
    public Terrain terrain;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        // 获取高度图
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        // 保存为文本文件
        string filePath = Application.dataPath + "/TerrainHeights.txt";
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    writer.Write(heights[y, x] + "\t");
                }
                writer.WriteLine();
            }
        }

        Debug.Log("Terrain data exported to: " + filePath);
    }
}
