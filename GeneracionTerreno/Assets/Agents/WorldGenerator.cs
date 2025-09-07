using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Mapa 3D")]
    public int width;
    public int depth;
    private float[,] heightmap;
    public Terrain terrain;

    [Header("Costa")]
    public int tokenLimit;
    public int initialTokens;
    public float coastPerlinScale;
    public float coastPerlinWeight;
    public float coastCenterBiasWeight;

    [Header("Suavizado")]
    public int smoothTokens;
    public int globalSmoothIterations;

    [Header("Playas")]
    public int beachTokens;
    public int beachSmoothRadius;
    public float beachMaxHeight;
    public float beachMinHeight;

    [Header("Montañas")]
    public int mountainTokens;
    public float mountainHeightIncrement;
    public int mountainSmoothRadius;
    public int mountainDirectionChangeInterval;
    public int mountainRadius;

    [Header("Colinas")]
    public int hillsCount;
    public int hillTokens;
    public float hillHeightIncrement;
    public int hillSmoothRadius;
    public float hillMinDistance;
    public float hillMaxDistance;
    public int hillRPerp;
    public int hillRAlong;

    [Header("Río")]
    public float minDist;
    public float maxDist;
    public float riverDepthFactor;
    public int riverSmoothRadius;
    public GameObject waterPlanePrefab;

    void Start()
    {
        int resolution = Mathf.ClosestPowerOfTwo(Mathf.Max(width, depth) - 1) + 1;
        resolution += 1;

        // Inicializa el mapa de alturas
        heightmap = new float[resolution, resolution];
        width = depth = resolution;

        // Genera la línea de costa
        CoastAgent coastGen = new CoastAgent(width, depth, tokenLimit, coastPerlinScale, coastPerlinWeight, coastCenterBiasWeight);
        coastGen.GenerateCoastline(heightmap, initialTokens);

        // Rellena el interior del mapa con tierra
        RellenarInterior();

        // Obtiene los puntos de costa
        List<Vector2Int> coastPoints = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                if (heightmap[x, z] <= 0.12f)
                    coastPoints.Add(new Vector2Int(x, z));

        // Genera playas en los puntos de costa
        SmoothAgent sAgent = new SmoothAgent(heightmap, coastPoints[0], smoothTokens);
        int beachTokensPerAgent = Mathf.Max(1, beachTokens / coastPoints.Count);
        foreach (var p in coastPoints)
        {
            BeachAgent beach = new BeachAgent(p, beachTokensPerAgent, beachSmoothRadius, sAgent, beachMinHeight, beachMaxHeight);
            beach.GenerateBeach(heightmap);
        }

        // Genera una montaña en una posición aleatoria
        MountainAgent mountain = new MountainAgent(heightmap, sAgent);
        Vector2Int mountainStart = new Vector2Int(
            Random.Range(width / 6, 5 * width / 6),
            Random.Range(depth / 6, 5 * depth / 6)
        );
        mountain.GenerateMountain(
            mountainStart,
            tokens: mountainTokens,
            heightIncrement: mountainHeightIncrement,
            smoothRadius: mountainSmoothRadius,
            directionChangeInterval: mountainDirectionChangeInterval,
            radius: mountainRadius
        );

        // Genera colinas alrededor de la montaña
        var hill = new HillAgent(heightmap, sAgent);
        Vector2 mountainCenter = new Vector2(mountainStart.x, mountainStart.y);

        for (int i = 0; i < hillsCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(hillMinDistance, hillMaxDistance);
            int margin = width / 10;

            int baseX = Mathf.RoundToInt(mountainCenter.x + Mathf.Cos(angle) * distance);
            int baseY = Mathf.RoundToInt(mountainCenter.y + Mathf.Sin(angle) * distance);

            baseX = Mathf.Clamp(baseX, margin, width - margin - 1);
            baseY = Mathf.Clamp(baseY, margin, depth - margin - 1);

            var basePoint = new Vector2Int(baseX, baseY);

            hill.GenerateHill(
                basePoint,
                mountainCenter,
                tokens: hillTokens,
                heightIncrement: hillHeightIncrement,
                smoothRadius: hillSmoothRadius,
                rPerp: hillRPerp,
                rAlong: hillRAlong
            );
        }

        // Busca un punto de costa cercano a la montaña para el río
        List<Vector2Int> coastNearMountain = new List<Vector2Int>();
        foreach (var p in coastPoints)
        {
            float dist = Vector2Int.Distance(p, mountainStart);
            if (dist >= minDist && dist <= maxDist)
                coastNearMountain.Add(p);
        }
        Vector2Int riverCoastPoint;
        if (coastNearMountain.Count > 0)
            riverCoastPoint = coastNearMountain[Random.Range(0, coastNearMountain.Count)];
        else
            riverCoastPoint = coastPoints[Random.Range(0, coastPoints.Count)];

        // Genera el río desde la montaña hasta la costa
        RiverAgent river = new RiverAgent(heightmap, sAgent);
        river.GenerateRiver(riverCoastPoint, mountainStart, riverDepthFactor, riverSmoothRadius);

        // Suaviza el terreno globalmente
        sAgent.SuavizadoGlobal(heightmap, globalSmoothIterations);

        // Ajusta los bordes al nivel del mar
        //ForzarBordesMar(heightmap, 0.05f);

        // Aplica el mapa de alturas al Terrain de Unity
        AplicarAlTerrain();

        // Pinta las texturas del terreno según la altura
        PintarTerreno();
    }

    void RellenarInterior()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (heightmap[x, z] == 0f)
                    heightmap[x, z] = 0.2f;
            }
        }
    }

    void AplicarAlTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("No se asignó un Terrain en el inspector.");
            return;
        }

        int resolution = Mathf.ClosestPowerOfTwo(Mathf.Max(width, depth)) + 1;
        terrain.terrainData.heightmapResolution = resolution;

        float[,] heights = new float[resolution, resolution];
        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                int hx = Mathf.Clamp(x, 0, width - 1);
                int hz = Mathf.Clamp(z, 0, depth - 1);
                heights[z, x] = heightmap[hx, hz];
            }
        }

        terrain.terrainData.size = new Vector3(width, 100, depth);
        terrain.terrainData.SetHeights(0, 0, heights);
    }

    void PintarTerreno()
    {
        if (terrain == null)
        {
            Debug.LogError("No se asignó un Terrain en el inspector.");
            return;
        }

        TerrainData data = terrain.terrainData;
        int mapWidth = data.alphamapWidth;
        int mapHeight = data.alphamapHeight;
        int numTextures = data.terrainLayers.Length;

        if (numTextures < 3)
        {
            Debug.LogWarning("Se requieren al menos 3 Terrain Layers en el Terrain.");
            return;
        }

        float[,,] splatmapData = new float[mapWidth, mapHeight, numTextures];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float normX = x / (float)(mapWidth - 1);
                float normY = y / (float)(mapHeight - 1);

                float height = data.GetInterpolatedHeight(normX, normY) / data.size.y;

                float[] weights = new float[numTextures];

                // Asigna la textura según la altura
                if (height < 0.15f)
                    weights[4] = 1f;
                else if (height < 0.19f)
                    weights[0] = 1f;
                else if (height < 0.22f)
                    weights[1] = 1f;
                else if (height < 0.35f)
                    weights[2] = 1f;
                else
                    weights[3] = 1f;

                float total = 0;
                for (int i = 0; i < numTextures; i++) total += weights[i];
                for (int i = 0; i < numTextures; i++) weights[i] /= total;

                for (int i = 0; i < numTextures; i++)
                    splatmapData[y, x, i] = weights[i];
            }
        }

        data.SetAlphamaps(0, 0, splatmapData);
    }

    void ForzarBordesMar(float[,] map, float mar = 0.0f)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            map[x, 0] = mar;
            map[x, h - 1] = mar;
        }
        for (int y = 0; y < h; y++)
        {
            map[0, y] = mar;
            map[w - 1, y] = mar;
        }
    }
}
