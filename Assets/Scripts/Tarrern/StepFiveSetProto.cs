using GPT;
using UnityEngine;

namespace Tarrern
{
    public class StepFiveSetProto : StepPrompt
    {
        
    }

    public partial class TerrainGenerator : MonoBehaviour
    {
        private void SetProto(TerrainData terrainData)
        {
            DetailPrototype grassPrototype = new DetailPrototype
            {
                prototypeTexture = Resources.Load<Texture2D>("Textures/GrassBillboard"),
                minHeight = 1.0f,
                maxHeight = 2.0f,
                minWidth = 1.0f,
                maxWidth = 2.0f
            };

            terrainData.detailPrototypes = new DetailPrototype[] { grassPrototype };

            int resolution = terrainData.detailResolution;
            int[,] details = new int[resolution, resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    details[y, x] = Random.Range(0, terrainDataSo.grassDensity); // 根据草密度设置
                }
            }

            terrainData.SetDetailLayer(0, 0, 0, details);
        }
    }
}