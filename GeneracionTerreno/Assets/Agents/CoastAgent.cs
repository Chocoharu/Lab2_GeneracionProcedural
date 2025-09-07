using System.Collections.Generic;
using UnityEngine;

public class CoastAgent
{
    // Dimensiones y par�metros de generaci�n
    private int width, depth, tokenLimit;
    private float perlinScale = 0.15f;
    private float perlinWeight = 1.2f;
    private float centerBiasWeight = 1.0f;

    public CoastAgent(int width, int depth, int tokenLimit, float perlinScale, float perlinWeight, float centerBiasWeight)
    {
        this.width = width;
        this.depth = depth;
        this.tokenLimit = tokenLimit;
        this.perlinScale = perlinScale;
        this.perlinWeight = perlinWeight;
        this.centerBiasWeight = centerBiasWeight;
    }

    // Inicia la generaci�n de la l�nea costera
    public void GenerateCoastline(float[,] heightmap, int initialTokens)
    {
        Vector2Int start = RandomBorderPoint();
        Vector2Int dir = RandomDirection();
        CoastAgentInstance root = new CoastAgentInstance(start, initialTokens, dir);
        CoastlineGenerate(root, heightmap);
    }

    // Recursivamente divide el agente o pinta localmente
    private void CoastlineGenerate(CoastAgentInstance agent, float[,] heightmap)
    {
        if (agent.tokens <= tokenLimit)
        {
            RunLocalPaint(agent, heightmap);
            return;
        }

        CoastAgentInstance childA = CreateChild(agent);
        CoastAgentInstance childB = CreateChild(agent);

        CoastlineGenerate(childA, heightmap);
        CoastlineGenerate(childB, heightmap);
    }

    // Pinta la l�nea costera localmente seg�n la puntuaci�n de celdas adyacentes
    private void RunLocalPaint(CoastAgentInstance agent, float[,] heightmap)
    {
        for (int t = 0; t < agent.tokens; t++)
        {
            Vector2Int bestCell = agent.position;
            float bestScore = float.NegativeInfinity;

            foreach (Vector2Int neighbor in GetAdjacent(agent.position))
            {
                if (!IsInside(neighbor)) continue;

                float s = EvaluateScore(neighbor, agent, heightmap);
                if (s > bestScore)
                {
                    bestScore = s;
                    bestCell = neighbor;
                }
            }

            heightmap[bestCell.x, bestCell.y] = 0.1f;
            agent.position = bestCell;
        }
    }

    // Crea un agente hijo con la mitad de los tokens
    private CoastAgentInstance CreateChild(CoastAgentInstance parent)
    {
        Vector2Int seed = RandomBorderPoint();
        Vector2Int dir = RandomDirection();
        int tok = Mathf.Max(1, parent.tokens / 2);

        return new CoastAgentInstance(seed, tok, dir);
    }

    // Calcula la puntuaci�n de una celda para decidir el avance del agente
    private float EvaluateScore(Vector2Int cell, CoastAgentInstance agent, float[,] heightmap)
    {
        float score = 0f;

        if (heightmap[cell.x, cell.y] == 0f) score += 1f;

        Vector2 v = ((Vector2)(cell - agent.position)).normalized;
        Vector2 d = ((Vector2)agent.direction).normalized;
        score += Vector2.Dot(v, d) * 0.5f;

        // Par�metros configurables para ruido y sesgo al centro
        score += Mathf.PerlinNoise(cell.x * perlinScale, cell.y * perlinScale) * perlinWeight;

        float cx = width / 2f;
        float cy = depth / 2f;
        float distToCenter = Vector2.Distance(new Vector2(cell.x, cell.y), new Vector2(cx, cy));
        float maxDist = Mathf.Min(width, depth) * 0.5f;
        score += (1f - (distToCenter / maxDist)) * centerBiasWeight;

        return score;
    }

    // Verifica si la celda est� dentro de los l�mites
    private bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < depth;
    }

    // Devuelve un punto aleatorio en el borde del mapa
    private Vector2Int RandomBorderPoint()
    {
        int side = Random.Range(0, 4);
        if (side == 0) return new Vector2Int(Random.Range(0, width), 0);
        if (side == 1) return new Vector2Int(Random.Range(0, width), depth - 1);
        if (side == 2) return new Vector2Int(0, Random.Range(0, depth));
        return new Vector2Int(width - 1, Random.Range(0, depth));
    }

    // Devuelve una direcci�n cardinal aleatoria
    private Vector2Int RandomDirection()
    {
        Vector2Int[] dirs = {
           Vector2Int.up, Vector2Int.down,
           Vector2Int.left, Vector2Int.right
        };
        return dirs[Random.Range(0, dirs.Length)];
    }

    // Devuelve las celdas adyacentes (incluyendo diagonales)
    private List<Vector2Int> GetAdjacent(Vector2Int p)
    {
        return new List<Vector2Int>
        {
            p + new Vector2Int(1, 0),
            p + new Vector2Int(-1, 0),
            p + new Vector2Int(0, 1),
            p + new Vector2Int(0, -1),
            p + new Vector2Int(1, 1),
            p + new Vector2Int(-1, -1),
            p + new Vector2Int(1, -1),
            p + new Vector2Int(-1, 1)
        };
    }
}

// Instancia de agente para la recursividad
public class CoastAgentInstance
{
    public Vector2Int position;
    public int tokens;
    public Vector2Int direction;

    public CoastAgentInstance(Vector2Int startPos, int tokenCount, Vector2Int dir)
    {
        position = startPos;
        tokens = tokenCount;
        direction = dir;
    }
}
