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
        float[,] heights = GeneratePerlinNoise(
            terrainDataSo.heightmapResolution,
            terrainDataSo.heightmapResolution,
            terrainDataSo.octaves,
            terrainDataSo.persistence,
            terrainDataSo.noiseScale
        );
        terrainData.SetHeights(0, 0, heights);
    }
    private float[,] GeneratePerlinNoise(int width, int height, int octaves, float persistence, float scale)
    {
        float[,] heights = new float[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = 0;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < octaves; i++)
                {
                    noise += Mathf.PerlinNoise(x * scale * frequency, y * scale * frequency) * amplitude;
                    amplitude *= persistence;
                    frequency *= 2;
                }
                heights[x, y] = noise;
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
        // 获取 TerrainDataSo 中的纹理层数量
        int layerCount = terrainDataSo.textureLayers.Length;

        // 创建 TerrainLayer 数组
        TerrainLayer[] layers = new TerrainLayer[layerCount];

        for (int i = 0; i < layerCount; i++)
        {
            // 在循环内定义 soLayer，绑定到当前的 textureLayers[i]
            TerrainTextureLayer soLayer = terrainDataSo.textureLayers[i];

            // 创建 TerrainLayer 并设置参数
            TerrainLayer layer = new TerrainLayer
            {
                diffuseTexture = soLayer.diffuseTexture,
                tileSize = soLayer.tileSize
            };

            layers[i] = layer;
        }

        // 应用纹理层到地形
        terrainData.terrainLayers = layers;

        // 设置 Alpha Maps（控制纹理混合）
        int resolution = terrainData.alphamapResolution;
        float[,,] alphaMaps = new float[resolution, resolution, layerCount];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float totalWeight = 0;

                for (int layer = 0; layer < layerCount; layer++)
                {
                    // 重新绑定当前的 soLayer
                    TerrainTextureLayer currentLayer = terrainDataSo.textureLayers[layer];

                    // 使用 currentLayer 的混合参数
                    float weight = Random.Range(currentLayer.minBlend, currentLayer.maxBlend);
                    alphaMaps[y, x, layer] = weight;
                    totalWeight += weight;
                }

                // 归一化混合权重
                for (int layer = 0; layer < layerCount; layer++)
                {
                    alphaMaps[y, x, layer] /= totalWeight;
                }
            }
        }

        // 应用混合贴图
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }


    private void SetPlants(TerrainData terrainData)
    {
        List<TreePrototype> treePrototypes = new List<TreePrototype>();
        foreach (var plant in terrainDataSo.plants)
        {
            TreePrototype prototype = new TreePrototype
            {
                prefab = plant.prefab
            };
            treePrototypes.Add(prototype);
        }
        terrainData.treePrototypes = treePrototypes.ToArray();

        List<TreeInstance> treeInstances = new List<TreeInstance>();
        for (int p = 0; p < terrainDataSo.plants.Length; p++)
        {
            Plant plant = terrainDataSo.plants[p];
            float areaWidth = (plant.area.y - plant.area.x) * terrainData.size.x;
            float areaHeight = (plant.area.y - plant.area.x) * terrainData.size.z;
            int areaCount = Mathf.FloorToInt(areaWidth * areaHeight * plant.density * 0.01f);

            for (int i = 0; i < areaCount; i++)
            {
                float normalizedX = Random.Range(plant.area.x, plant.area.y);
                float normalizedZ = Random.Range(plant.area.x, plant.area.y);

                float terrainHeight = terrainData.GetHeight(
                    Mathf.FloorToInt(normalizedX * terrainData.heightmapResolution),
                    Mathf.FloorToInt(normalizedZ * terrainData.heightmapResolution)
                );

                float normalizedY = terrainHeight / terrainData.size.y;

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
}