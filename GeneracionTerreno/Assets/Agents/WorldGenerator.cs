using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Mapa 3D")]
    public int width;
    public int depth;
    public int tokenLimit;
    public int initialTokens;

    [Header("Playas")]
    public int beachTokens;
    public int beachSmoothRadius;

    [Header("Montañas")]
    public int mountainTokens;
    public float mountainHeightIncrement;
    public int mountainSmoothRadius;
    public int mountainDirectionChangeInterval;

    private float[,] heightmap;
    public Terrain terrain;

    void Start()
    {
        // --- Inicializar mapa ---
        heightmap = new float[width, depth];

        // --- Generar línea de costa ---
        CoastAgent coastGen = new CoastAgent(width, depth, tokenLimit);
        coastGen.GenerateCoastline(heightmap, initialTokens);

        // --- Rellenar tierra ---
        RellenarInterior();

        // --- Generar playas ---
        Vector2Int beachStart = new Vector2Int(width / 2, 0); // borde inferior
        SmoothAgent sAgent = new SmoothAgent(heightmap, beachStart, beachTokens);
        BeachAgent beach = new BeachAgent(beachStart, beachTokens, beachSmoothRadius, sAgent);
        beach.GenerateBeach(heightmap);

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

        // Unity espera [height, width]
        float[,] heights = new float[depth, width];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                heights[z, x] = heightmap[x, z];
            }
        }

        // Configurar el terreno
        terrain.terrainData.heightmapResolution = Mathf.Max(width, depth);
        terrain.terrainData.size = new Vector3(width, 20, depth); // 20 = altura máxima
        terrain.terrainData.SetHeights(0, 0, heights);
    }
}
