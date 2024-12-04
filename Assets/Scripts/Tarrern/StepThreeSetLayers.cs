using GPT;
using UnityEngine;

namespace Tarrern
{
    public class StepThreeSetLayers : StepPrompt
    {
        
    }

    public partial class TerrainGenerator : MonoBehaviour
    {
        // 设置地形纹理
        // terrainData.terrainLayers：控制地形的纹理层。
        // terrainData.alphamapResolution：控制混合纹理贴图的分辨率。
        // terrainData.GetAlphamaps(x, y, width, height)：获取纹理混合数据。
        // terrainData.SetAlphamaps(x, y, alphaMaps)：设置纹理混合数据
        private void SetLayers(TerrainData terrainData)
        {
            int layerCount = terrainDataSo.textureLayers.Length;
            TerrainLayer[] layers = new TerrainLayer[layerCount];

            for (int i = 0; i < layerCount; i++)
            {
                TerrainTextureLayer soLayer = terrainDataSo.textureLayers[i];
                TerrainLayer layer = new TerrainLayer
                {
                    diffuseTexture = soLayer.diffuseTexture,
                    tileSize = soLayer.tileSize
                };
                layers[i] = layer;
            }

            terrainData.terrainLayers = layers;

            int resolution = terrainData.alphamapResolution;
            float[,,] alphaMaps = new float[resolution, resolution, layerCount];

            float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float height = heights[y, x];
                    float totalWeight = 0;

                    for (int layer = 0; layer < layerCount; layer++)
                    {
                        TerrainTextureLayer currentLayer = terrainDataSo.textureLayers[layer];
                        float weight = 0;

                        // 基于高度分布
                        if (height >= currentLayer.minHeight && height <= currentLayer.maxHeight)
                        {
                            weight = Mathf.InverseLerp(currentLayer.minHeight, currentLayer.maxHeight, height);
                        }

                        // 结合斜率分布（如果需要）
                        float slope = CalculateSlope(terrainData, x, y);
                        if (slope >= currentLayer.minSlope && slope <= currentLayer.maxSlope)
                        {
                            weight *= Mathf.InverseLerp(currentLayer.minSlope, currentLayer.maxSlope, slope);
                        }

                        // 加入噪声控制
                        if (currentLayer.useNoise)
                        {
                            float noise = Mathf.PerlinNoise(x * currentLayer.noiseScale, y * currentLayer.noiseScale);
                            weight *= noise;
                        }

                        // 应用 minBlend 和 maxBlend 限制
                        float clampedWeight = Mathf.Clamp(weight, currentLayer.minBlend, currentLayer.maxBlend);
                        alphaMaps[y, x, layer] = clampedWeight;
                        totalWeight += clampedWeight;
                    }
                
                    // 归一化权重
                    for (int layer = 0; layer < layerCount; layer++)
                    {
                        alphaMaps[y, x, layer] /= totalWeight > 0 ? totalWeight : 1;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        // 计算斜率（角度）
        private float CalculateSlope(TerrainData terrainData, int x, int y)
        {
            Vector3 normal = terrainData.GetInterpolatedNormal((float)x / terrainData.alphamapResolution, (float)y / terrainData.alphamapResolution);
            return Vector3.Angle(normal, Vector3.up);
        }
    }
}