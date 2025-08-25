using System.Collections.Generic;
using UnityEngine;

public class CostlineGenerator : MonoBehaviour
{
    [Header("Mapa 3D")]
    public int width;
    public int depth;
    public int tokenLimit;
    public int initialTokens;


    private float[,] heightmap;
    public Terrain terrain;

    void Start()
    {
        heightmap = new float[width, depth];

        Vector2Int start = RandomBorderPoint();
        Vector2Int dir = RandomDirection();

        CoastAgent root = new CoastAgent(start, initialTokens, dir);
        CoastlineGenerate(root);
        //RellenarInterior();
        AplicarAlTerrain();
    }

    void CoastlineGenerate(CoastAgent agent)
    {
        if (agent.tokens <= tokenLimit)
        {
            RunLocalPaint(agent);
            return;
        }

        CoastAgent childA = CreateChild(agent);
        CoastAgent childB = CreateChild(agent);

        CoastlineGenerate(childA);
        CoastlineGenerate(childB);
    }

    void RunLocalPaint(CoastAgent agent)
    {
        for (int t = 0; t < agent.tokens; t++)
        {
            Vector2Int bestCell = agent.position;
            float bestScore = float.NegativeInfinity;

            foreach (Vector2Int neighbor in GetAdjacent(agent.position))
            {
                if (!IsInside(neighbor)) continue;

                float s = EvaluateScore(neighbor, agent);
                if (s > bestScore)
                {
                    bestScore = s;
                    bestCell = neighbor;
                }
            }

            // Asigna altura baja para la costa (ejemplo: 0.1)
            heightmap[bestCell.x, bestCell.y] = 0.1f;
            agent.position = bestCell;
        }
    }

    CoastAgent CreateChild(CoastAgent parent)
    {
        Vector2Int seed = RandomBorderPoint();
        Vector2Int dir = RandomDirection();
        int tok = Mathf.Max(1, parent.tokens / 2);

        return new CoastAgent(seed, tok, dir);
    }

    float EvaluateScore(Vector2Int cell, CoastAgent agent)
    {
        float score = 0f;

        if (heightmap[cell.x, cell.y] == 0f) score += 1f; // preferir agua  

        Vector2 v = ((Vector2)(cell - agent.position)).normalized;
        Vector2 d = ((Vector2)agent.direction).normalized; // FIX: Cast Vector2Int to Vector2 before accessing normalized  
        score += Vector2.Dot(v, d) * 0.5f; // alineación  
        score += Random.value * 0.2f; // ruido  

        return score;
    }
    void RellenarInterior()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (heightmap[x, z] == 0f)
                    heightmap[x, z] = 0.5f; // altura de tierra
            }
        }
    }

    // ---------------- Helpers ----------------  
    bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < depth;
    }

    Vector2Int RandomBorderPoint()
    {
        int side = Random.Range(0, 4);
        if (side == 0) return new Vector2Int(Random.Range(0, width), 0);
        if (side == 1) return new Vector2Int(Random.Range(0, width), depth - 1);
        if (side == 2) return new Vector2Int(0, Random.Range(0, depth));
        return new Vector2Int(width - 1, Random.Range(0, depth));
    }

    Vector2Int RandomDirection()
    {
        Vector2Int[] dirs = {
           Vector2Int.up, Vector2Int.down,
           Vector2Int.left, Vector2Int.right
       };
        return dirs[Random.Range(0, dirs.Length)];
    }

    List<Vector2Int> GetAdjacent(Vector2Int p)
    {
        return new List<Vector2Int>
       {
           p + Vector2Int.up,
           p + Vector2Int.down,
           p + Vector2Int.left,
           p + Vector2Int.right
       };
    }

    // ---------------- Generar Mundo 3D ----------------  
    void AplicarAlTerrain()
    {
        // El heightmap de Unity debe ser [height, width]
        float[,] heights = new float[depth, width];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                heights[z, x] = heightmap[x, z];
            }
        }
        terrain.terrainData.heightmapResolution = Mathf.Max(width, depth);
        terrain.terrainData.size = new Vector3(width, 20, depth); // 20 = altura máxima del terreno
        terrain.terrainData.SetHeights(0, 0, heights);
    }
}