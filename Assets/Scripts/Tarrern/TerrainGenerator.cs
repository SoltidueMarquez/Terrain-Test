using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
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
    
    private void SetResolution(TerrainData terrainData)
    {
        // 从 ScriptableObject 读取分辨率和尺寸
        terrainData.heightmapResolution = terrainDataSo.heightmapResolution;
        terrainData.alphamapResolution = terrainDataSo.alphamapResolution;
        terrainData.SetDetailResolution(terrainDataSo.alphamapResolution, terrainDataSo.detailResolution);
        terrainData.size = terrainDataSo.dataSize;
    }
    
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
    
    private void CreateTerrainObject(TerrainData terrainData)
    {
        // 创建 Terrain 对象
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.transform.position = Vector3.zero;
        // 确保绘制树木
        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.drawTreesAndFoliage = true;
    }
    
    private void SetLight(Terrain terrainObject)
    {
        // 光照和其他参数
        terrainObject.materialTemplate = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
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
    
    private bool IsPointInCircle(Vector2 point, Vector2 center, float radius)
    {
        // 计算点与圆心的距离
        float distance = Vector2.Distance(point, center);
        return distance <= radius;
    }

}