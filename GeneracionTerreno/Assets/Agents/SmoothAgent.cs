using UnityEngine;

public class SmoothAgent
{
    private float[,] heightMap;
    private int mapSize;
    private int tokens;
    private Vector2Int start;

    public SmoothAgent(float[,] heightMap, Vector2Int start, int tokens)
    {
        this.heightMap = heightMap;
        this.mapSize = heightMap.GetLength(0);
        this.start = start;
        this.tokens = tokens;
    }

    public void RunFromPoint(float[,] heightMap, Vector2Int start, int tokens)
    {
        Vector2Int location = start;

        for (int t = 0; t < tokens; t++)
        {
            float smoothed = GetWeightedAverage(location, heightMap);
            heightMap[location.x, location.y] = smoothed;

            location = GetRandomNeighbor(location, heightMap.GetLength(0));
        }
    }

    private float GetWeightedAverage(Vector2Int loc, float[,] map)
    {
        float sum = 0f;
        float weightSum = 0f;
        int size = map.GetLength(0);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = loc.x + dx;
                int ny = loc.y + dy;

                if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                {
                    float weight = (dx == 0 && dy == 0) ? 4f : 1f;
                    sum += map[nx, ny] * weight;
                    weightSum += weight;
                }
            }
        }
        return sum / weightSum;
    }

    private Vector2Int GetRandomNeighbor(Vector2Int loc, int size)
    {
        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        int choice = Random.Range(0, 4);

        int nx = Mathf.Clamp(loc.x + dirs[choice, 0], 0, size - 1);
        int ny = Mathf.Clamp(loc.y + dirs[choice, 1], 0, size - 1);

        return new Vector2Int(nx, ny);
    }

    public void SmoothArea(float[,] heightmap, Vector2Int location, int radius)
    {
        int width = heightmap.GetLength(0);
        int depth = heightmap.GetLength(1);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = location.x + x;
                int ny = location.y + y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < depth)
                {
                    float avg = AverageNeighborhood(heightmap, nx, ny, 1);
                    heightmap[nx, ny] = (heightmap[nx, ny] + avg) / 2f;
                }
            }
        }
    }

    private float AverageNeighborhood(float[,] heightmap, int cx, int cy, int radius)
    {
        float sum = 0f;
        int count = 0;
        int width = heightmap.GetLength(0);
        int depth = heightmap.GetLength(1);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = cx + x;
                int ny = cy + y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < depth)
                {
                    sum += heightmap[nx, ny];
                    count++;
                }
            }
        }
        return sum / count;
    }
    public float AverageNeighborhoodPublic(float[,] map, int cx, int cy, int radius)
    {
        float sum = 0f; int count = 0;
        int w = map.GetLength(0), h = map.GetLength(1);
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
            {
                int nx = cx + x, ny = cy + y;
                if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                { sum += map[nx, ny]; count++; }
            }
        return sum / Mathf.Max(1, count);
    }
    public void SuavizadoGlobal(float[,] map, int repeticiones)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int r = 0; r < repeticiones; r++)
        {
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    float avg =
                        (map[x, y] +
                         map[x - 1, y] + map[x + 1, y] +
                         map[x, y - 1] + map[x, y + 1]) / 5f;

                    map[x, y] = Mathf.Lerp(map[x, y], avg, 0.5f);
                }
            }
        }
    }
}
