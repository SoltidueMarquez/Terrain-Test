using GPT;
using UnityEngine;

namespace Tarrern
{
    public class StepSixSetLight : StepPrompt
    {
        
    }

    public partial class TerrainGenerator : MonoBehaviour
    {
        private void SetLight(Terrain terrainObject)
        {
            // 光照和其他参数
            terrainObject.materialTemplate = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
        }
    }
}