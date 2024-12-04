using GPT;
using UnityEngine;

namespace Tarrern
{
    public class StepTwoSetHeight : StepPrompt
    {
        
    }

    public partial class TerrainGenerator : MonoBehaviour
    {
        private void SetHeight(TerrainData terrainData)
        {
            var heights = GenerateTerrainNoise(
                terrainDataSo.heightmapResolution,
                terrainDataSo.heightmapResolution,
                terrainDataSo
            );
            terrainData.SetHeights(0, 0, heights);
        }
        private float[,] GenerateTerrainNoise(int width, int height, TerrainDataSo data)
        {
            float[,] heights = new float[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float finalNoise = 0f;
                    foreach (NoiseLayer layer in data.noiseLayers)
                    {
                        if (layer.enabled)
                        {
                            finalNoise += layer.Evaluate(x, y);
                            finalNoise = Mathf.Clamp(finalNoise, 0f, 1f);//归一化
                        }
                    }
                    heights[x, y] = finalNoise;
                }
            }
        
            return heights;
        }
    }
}