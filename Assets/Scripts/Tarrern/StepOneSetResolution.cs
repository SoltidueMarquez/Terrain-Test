using GPT;
using UnityEngine;

namespace Tarrern
{
    public class StepOneSetResolution : StepPrompt
    {
        
    }
    public partial class TerrainGenerator : MonoBehaviour
    {
        private void SetResolution(TerrainData terrainData)
        {
            // 从 ScriptableObject 读取分辨率和尺寸
            terrainData.heightmapResolution = terrainDataSo.heightmapResolution;
            terrainData.alphamapResolution = terrainDataSo.alphamapResolution;
            terrainData.SetDetailResolution(terrainDataSo.alphamapResolution, terrainDataSo.detailResolution);
            terrainData.size = terrainDataSo.dataSize;
        }
    }
}