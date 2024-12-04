using System.Collections.Generic;
using GPT;
using UnityEngine;

namespace Tarrern
{
    public class StepFourSetPlants : StepPrompt
    {
        
    }

    public partial class TerrainGenerator : MonoBehaviour
    {
        private void SetPlants(TerrainData terrainData)
        {
            List<TreePrototype> treePrototypes = new List<TreePrototype>();
            foreach (var plant in terrainDataSo.plants)
            {
                TreePrototype prototype = new TreePrototype
                {
                    prefab = plant.prefab  // 使用植物的预设
                };
                treePrototypes.Add(prototype);
            }
            terrainData.treePrototypes = treePrototypes.ToArray();

            List<TreeInstance> treeInstances = new List<TreeInstance>();

            // 为每种植物生成实例
            for (int p = 0; p < terrainDataSo.plants.Length; p++)
            {
                Plant plant = terrainDataSo.plants[p];

                // 遍历每个圆形区域
                foreach (var area in plant.area)
                {
                    Vector2 center = new Vector2(area.x, area.y);  // 圆心坐标
                    float radius = area.z;                         // 半径

                    int areaCount = Mathf.FloorToInt(terrainData.size.x * terrainData.size.z * plant.density * 0.01f);

                    for (int i = 0; i < areaCount; i++)
                    {
                        // 随机生成点的 x 和 z 坐标
                        float normalizedX = Random.Range(0f, 1f);
                        float normalizedZ = Random.Range(0f, 1f);

                        Vector2 point = new Vector2(normalizedX, normalizedZ);

                        // 判断点是否在圆形区域内
                        if (IsPointInCircle(point, center, radius))
                        {
                            // 获取该点的海拔
                            float terrainHeight = terrainData.GetHeight(
                                Mathf.FloorToInt(normalizedX * terrainData.heightmapResolution),
                                Mathf.FloorToInt(normalizedZ * terrainData.heightmapResolution)
                            );

                            // 获取海拔比例
                            float normalizedY = terrainHeight / terrainData.size.y;

                            // 判断海拔是否在指定范围内
                            if (normalizedY >= plant.elevationRange.x && normalizedY <= plant.elevationRange.y)
                            {
                                // 获取该点的坡度
                                float slope = CalculateSlope(terrainData, Mathf.FloorToInt(normalizedX * terrainData.heightmapResolution), Mathf.FloorToInt(normalizedZ * terrainData.heightmapResolution));

                                // 检查该点的坡度是否在指定范围内
                                if (slope >= plant.minSlope && slope <= plant.maxSlope)
                                {
                                    // 如果符合坡度要求，生成植物
                                    float widthScale = Random.Range(plant.widthScaleRange.x, plant.widthScaleRange.y);
                                    float heightScale = Random.Range(plant.heightScaleRange.x, plant.heightScaleRange.y);

                                    TreeInstance treeInstance = new TreeInstance
                                    {
                                        position = new Vector3(normalizedX, normalizedY, normalizedZ),
                                        prototypeIndex = p,
                                        widthScale = widthScale,
                                        heightScale = heightScale,
                                        color = Color.white,
                                        lightmapColor = Color.white
                                    };

                                    treeInstances.Add(treeInstance);
                                }
                            }
                        }
                    }
                }
            }
            terrainData.treeInstances = treeInstances.ToArray();
        }
        
        private bool IsPointInCircle(Vector2 point, Vector2 center, float radius)
        {
            // 计算点与圆心的距离
            float distance = Vector2.Distance(point, center);
            return distance <= radius;
        }
    }
}