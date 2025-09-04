using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Mapa 3D")]
    public int width = 128;
    public int depth = 128;
    public int tokenLimit = 12;
    public int initialTokens = 700;

    [Header("Playas")]
    public int beachTokens = 250;
    public int beachSmoothRadius = 5;

    [Header("Montañas")]
    public int mountainTokens = 350;
    public float mountainHeightIncrement = 0.025f;
    public int mountainSmoothRadius = 2;
    public int mountainDirectionChangeInterval = 15;

    private float[,] heightmap;
    public Terrain terrain;

    void Start()
    {
        int resolution = Mathf.ClosestPowerOfTwo(Mathf.Max(width, depth) - 1) + 1;
        resolution += 1;

        // --- Inicializar mapa ---
        heightmap = new float[resolution, resolution];
        width = depth = resolution;

        // --- Generar línea de costa ---
        CoastAgent coastGen = new CoastAgent(width, depth, tokenLimit);
        coastGen.GenerateCoastline(heightmap, initialTokens);

        // --- Rellenar tierra ---
        RellenarInterior();

        // --- Forzar bordes a nivel del mar ---
        ForzarBordesMar(heightmap, 0.05f);

        // --- Recopilar todos los puntos de costa ---
        List<Vector2Int> coastPoints = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                if (heightmap[x, z] <= 0.12f)
                    coastPoints.Add(new Vector2Int(x, z));

        // --- Generar playas desde varios puntos de la costa ---
        SmoothAgent sAgent = new SmoothAgent(heightmap, coastPoints[0], beachTokens);
        int beachTokensPerAgent = Mathf.Max(1, beachTokens / coastPoints.Count);
        foreach (var p in coastPoints)
        {
            BeachAgent beach = new BeachAgent(p, beachTokensPerAgent, beachSmoothRadius, sAgent);
            beach.GenerateBeach(heightmap);
        }

        // --- Generar montañas ---
        MountainAgent mountain = new MountainAgent(heightmap, sAgent);
        Vector2Int mountainStart = new Vector2Int(width / 3, depth / 3);
        mountain.GenerateMountain(
            mountainStart,
            tokens: mountainTokens,
            heightIncrement: mountainHeightIncrement,
            smoothRadius: mountainSmoothRadius,
            directionChangeInterval: mountainDirectionChangeInterval
        );

        // --- Suavizado global más fuerte ---
        sAgent.SuavizadoGlobal(heightmap, 1);

        // --- Normalizar valores antes de aplicar ---
        //NormalizarTerreno(heightmap);

        // --- Aplicar al terreno 3D ---
        AplicarAlTerrain();
    }

    void RellenarInterior()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (heightmap[x, z] == 0f)
                    heightmap[x, z] = 0.2f; // altura media para tierra
            }
        }
    }
    void AplicarAlTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("⚠️ No se asignó un Terrain en el inspector.");
            return;
        }

        // Asegura que el heightmap y la resolución coincidan
        int resolution = Mathf.ClosestPowerOfTwo(Mathf.Max(width, depth)) + 1;
        terrain.terrainData.heightmapResolution = resolution;

        // Si el heightmap no es cuadrado, ajusta el array
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
    void ForzarBordesMar(float[,] map, float mar = 0.0f)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        // Bordes horizontales
        for (int x = 0; x < w; x++)
        {
            map[x, 0] = mar;
            map[x, h - 1] = mar;
        }
        // Bordes verticales
        for (int y = 0; y < h; y++)
        {
            map[0, y] = mar;
            map[w - 1, y] = mar;
        }
    }
}
