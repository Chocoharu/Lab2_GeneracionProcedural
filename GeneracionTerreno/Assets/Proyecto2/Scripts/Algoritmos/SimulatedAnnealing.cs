using UnityEngine;
using System.Collections.Generic;

public class SimulatedAnnealing
{
    public float initialTemp = UIManager.Instance.saInitialTemp;
    public float coolingRate = UIManager.Instance.saCoolingRate;
    public int iterations = UIManager.Instance.saIterations;
    public float desiredWallDensity = UIManager.Instance.saDesiredWallDensity;

    public LevelRepresentation Run(LevelRepresentation start)
    {
        LevelRepresentation current = start.Clone();
        float currentFitness = Evaluate(current);

        float T = initialTemp;
        for (int i = 0; i < iterations; i++)
        {
            LevelRepresentation neighbor = Mutate(current);
            float newFitness = Evaluate(neighbor);

            // Aceptar si mejora o aleatoriamente si empeora
            if (newFitness > currentFitness || Mathf.Exp((newFitness - currentFitness) / T) > Random.value)
            {
                current = neighbor;
                currentFitness = newFitness;
            }

            T *= coolingRate;
        }

        current.fitness = currentFitness;
        return current;
    }

    // ---- MUTACIÓN: más variada ----
    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone();
        int width = level.width;
        int height = level.height;

        int mode = Random.Range(0, 3); // 0=flip single, 1=add cluster, 2=remove cluster

        int cx = Random.Range(1, width - 2);
        int cy = Random.Range(1, height - 2);

        int clusterSize = Random.Range(1, 3); // tamaño del grupo (1 o 2)

        for (int dx = -clusterSize; dx <= clusterSize; dx++)
        {
            for (int dy = -clusterSize; dy <= clusterSize; dy++)
            {
                int x = cx + dx;
                int y = cy + dy;
                if (x <= 0 || y <= 0 || x >= width - 1 || y >= height - 1) continue;
                int cell = clone.grid[x, y];
                if (cell == 2 || cell == 3 || cell == 4) continue;

                switch (mode)
                {
                    case 0: // flip
                        clone.grid[x, y] = (cell == 0) ? 1 : 0;
                        break;
                    case 1: // add wall cluster
                        clone.grid[x, y] = 1;
                        break;
                    case 2: // remove wall cluster
                        clone.grid[x, y] = 0;
                        break;
                }
            }
        }

        return clone;
    }

    // ---- EVALUACIÓN ----
    private float Evaluate(LevelRepresentation level)
    {
        int interiorCells = Mathf.Max(1, (level.width - 2) * (level.height - 2));
        int desiredWallCount = Mathf.Clamp(Mathf.RoundToInt(desiredWallDensity * interiorCells), 1, interiorCells);

        // 1️⃣ Densidad de muros
        int wallCount = 0;
        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                if (level.grid[x, y] == 1)
                    wallCount++;
        float wallBalance = Mathf.Exp(-Mathf.Abs(wallCount - desiredWallCount) / (float)Mathf.Max(1, desiredWallCount));

        // 2️⃣ Accesibilidad
        int reachable = CountReachableCells(level);
        float areaCoverage = reachable / (float)(level.width * level.height);

        // 3️⃣ Complejidad de caminos
        float pathComplexity = ComputePathComplexity(level);

        // 4️⃣ Conectividad global (penaliza regiones separadas)
        int regions = CountDisconnectedAreas(level);
        float connectivity = 1f / Mathf.Max(1f, regions);

        // 5️⃣ Combinar
        float fitness = 0.4f * areaCoverage
                      + 0.25f * wallBalance
                      + 0.2f * pathComplexity
                      + 0.15f * connectivity;

        if (reachable < (level.width * level.height * 0.05f))
            fitness *= 0.5f;

        return Mathf.Clamp01(fitness);
    }

    private int CountReachableCells(LevelRepresentation level)
    {
        (int x, int y)? player = FindPlayer(level);
        if (player == null) return 0;
        bool[,] visited = new bool[level.width, level.height];
        Queue<(int, int)> q = new();
        q.Enqueue(player.Value);
        visited[player.Value.x, player.Value.y] = true;
        int count = 1;
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        while (q.Count > 0)
        {
            var (cx, cy) = q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];
                if (nx <= 0 || ny <= 0 || nx >= level.width - 1 || ny >= level.height - 1) continue;
                if (visited[nx, ny] || level.grid[nx, ny] == 1) continue;
                visited[nx, ny] = true;
                q.Enqueue((nx, ny));
                count++;
            }
        }
        return count;
    }

    private float ComputePathComplexity(LevelRepresentation level)
    {
        (int x, int y)? player = FindPlayer(level);
        if (player == null) return 0;
        bool[,] visited = new bool[level.width, level.height];
        Queue<(int, int, int)> q = new();
        q.Enqueue((player.Value.x, player.Value.y, 0));
        visited[player.Value.x, player.Value.y] = true;

        int longest = 0;
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var (cx, cy, dist) = q.Dequeue();
            longest = Mathf.Max(longest, dist);
            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];
                if (nx <= 0 || ny <= 0 || nx >= level.width - 1 || ny >= level.height - 1) continue;
                if (visited[nx, ny] || level.grid[nx, ny] == 1) continue;
                visited[nx, ny] = true;
                q.Enqueue((nx, ny, dist + 1));
            }
        }
        return Mathf.Clamp01(longest / (float)(level.width + level.height));
    }

    private int CountDisconnectedAreas(LevelRepresentation level)
    {
        bool[,] visited = new bool[level.width, level.height];
        int regions = 0;
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        for (int x = 1; x < level.width - 1; x++)
        {
            for (int y = 1; y < level.height - 1; y++)
            {
                if (level.grid[x, y] == 1 || visited[x, y]) continue;
                regions++;
                Queue<(int, int)> q = new();
                q.Enqueue((x, y));
                visited[x, y] = true;
                while (q.Count > 0)
                {
                    var (cx, cy) = q.Dequeue();
                    for (int i = 0; i < 4; i++)
                    {
                        int nx = cx + dx[i];
                        int ny = cy + dy[i];
                        if (nx <= 0 || ny <= 0 || nx >= level.width - 1 || ny >= level.height - 1) continue;
                        if (visited[nx, ny] || level.grid[nx, ny] == 1) continue;
                        visited[nx, ny] = true;
                        q.Enqueue((nx, ny));
                    }
                }
            }
        }
        return regions;
    }

    private (int, int)? FindPlayer(LevelRepresentation level)
    {
        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                if (level.grid[x, y] == 4)
                    return (x, y);
        return null;
    }
}
