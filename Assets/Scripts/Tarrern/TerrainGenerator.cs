using UnityEngine;

namespace Tarrern
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        [Tooltip("地形配置数据")]
        public TerrainDataSo terrainDataSo;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Start Generate");
                GenerateTerrain();
            }
        }

        private void CreateTerrainObject(TerrainData terrainData)
        {
            // 创建 Terrain 对象
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.transform.position = Vector3.zero;
            // 确保绘制树木
            Terrain terrain = terrainObject.GetComponent<Terrain>();
            terrain.drawTreesAndFoliage = true;
        }

        private void GenerateTerrain()
        {
            if (terrainDataSo == null)
            {
                Debug.LogError("TerrainDataSo is not assigned!");
                return;
            }
        
            // 创建 TerrainData
            TerrainData terrainData = new TerrainData();

            SetResolution(terrainData);
        
            SetHeight(terrainData);

            SetLayers(terrainData);
        
            SetPlants(terrainData);

            SetProto(terrainData);

            CreateTerrainObject(terrainData);

            //SetLight(terrainObject.GetComponent<Terrain>());
        }

    }
}