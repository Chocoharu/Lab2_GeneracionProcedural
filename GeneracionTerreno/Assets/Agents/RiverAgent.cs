using System.Collections.Generic;
using UnityEngine;

public class RiverAgent
{
    private float[,] heightmap;
    private int width, depth;
    private SmoothAgent smoothAgent;
    public List<Vector2Int> RiverPath { get; private set; } = new List<Vector2Int>();

    // Constructor: inicializa el agente con el mapa de alturas y el agente de suavizado
    public RiverAgent(float[,] heightmap, SmoothAgent smoothAgent)
    {
        this.heightmap = heightmap;
        this.width = heightmap.GetLength(0);
        this.depth = heightmap.GetLength(1);
        this.smoothAgent = smoothAgent;
    }

    // Genera el río desde la costa hasta la montaña, excavando y suavizando el terreno
    public void GenerateRiver(Vector2Int coastPoint, Vector2Int mountainPoint, float depthFactor, int smoothRadius)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = coastPoint;
        RiverPath.Clear();

        int safety = 0;

        // Calcula el camino del río evitando colinas
        while (current != mountainPoint && safety < width * depth)
        {
            path.Add(current);
            RiverPath.Add(current);
            current = NextStepTowards(current, mountainPoint);
            safety++;
        }

        if (!path.Contains(mountainPoint))
            path.Add(mountainPoint);

        // Excava y suaviza el terreno a lo largo del camino
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int p = path[i];
            Vector2 dir = (i < path.Count - 1) ? (path[i + 1] - path[i]) : Vector2.zero;
            if (dir == Vector2.zero) dir = Vector2.right;

            AttenuateWedge(p, dir, depthFactor, 4);
            smoothAgent.SmoothArea(heightmap, p, smoothRadius);
        }
    }

    // Calcula el siguiente paso hacia el objetivo, evitando zonas elevadas
    private Vector2Int NextStepTowards(Vector2Int from, Vector2Int target)
    {
        Vector2Int bestStep = from;
        float bestScore = float.MaxValue;
        float hillThreshold = 0.22f;

        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                int nx = from.x + dx;
                int nz = from.y + dz;
                if (nx < 0 || nx >= width || nz < 0 || nz >= depth) continue;

                float h = heightmap[nx, nz];
                if (h > hillThreshold) continue;

                Vector2Int candidate = new Vector2Int(nx, nz);
                float dist = Vector2Int.Distance(candidate, target);
                float perlin = Mathf.PerlinNoise(nx * 0.07f, nz * 0.07f);

                float score = dist - h * 4f - perlin * 4f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestStep = candidate;
                }
            }

        return bestStep;
    }

    // Excava una zona en forma de cuña en la dirección indicada
    private void AttenuateWedge(Vector2Int center, Vector2 dir, float depthFactor, int radius)
    {
        Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
        for (int x = -radius; x <= radius; x++)
            for (int z = -radius; z <= radius; z++)
            {
                int px = center.x + x;
                int pz = center.y + z;

                if (px < 0 || px >= width || pz < 0 || pz >= depth) continue;

                float dist = Mathf.Abs(Vector2.Dot(new Vector2(x, z), perp));
                if (dist <= radius)
                {
                    float falloff = 1f - (dist / radius);
                    heightmap[px, pz] = Mathf.Clamp01(heightmap[px, pz] - depthFactor * falloff);
                }
            }
    }
}
