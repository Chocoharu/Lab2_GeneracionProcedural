using UnityEngine;

public class MountainAgent
{
    private float[,] heightmap;
    private int width, depth;
    private SmoothAgent smoothAgent;

    public MountainAgent(float[,] heightmap, SmoothAgent sAgent)
    {
        this.heightmap = heightmap;
        this.width = heightmap.GetLength(0);
        this.depth = heightmap.GetLength(1);
        this.smoothAgent = sAgent;
    }

    public void GenerateMountain(Vector2Int start, int tokens, float heightIncrement, int smoothRadius, int directionChangeInterval)
    {
        Vector2Int location = start;
        Vector2Int direction = RandomDirection();

        for (int i = 0; i < tokens; i++)
        {
            // Elevar un "wedge" (zona alargada)
            RaiseWedge(location, direction, heightIncrement);

            // Suavizar alrededor
            Smooth(location, smoothRadius);

            // Avanzar
            location += direction;

            // Cambiar dirección cada cierto número de tokens
            if (i % directionChangeInterval == 0)
            {
                direction = RotateDirection(direction, UnityEngine.Random.value > 0.5f ? 45 : -45);
            }
        }
    }

    private void RaiseWedge(Vector2Int location, Vector2Int direction, float increment)
    {
        int radius = 2; // grosor de la montaña
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = location.x + x;
                int ny = location.y + y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < depth)
                {
                    heightmap[nx, ny] = Mathf.Clamp01(heightmap[nx, ny] + increment);
                }
            }
        }
    }

    private void Smooth(Vector2Int location, int radius)
    {
        smoothAgent.SmoothArea(heightmap, location, radius);
    }

    private Vector2Int RandomDirection()
    {
        Vector2Int[] dirs = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };
        return dirs[UnityEngine.Random.Range(0, dirs.Length)];
    }

    private Vector2Int RotateDirection(Vector2Int dir, int degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        int newX = Mathf.RoundToInt(dir.x * cos - dir.y * sin);
        int newY = Mathf.RoundToInt(dir.x * sin + dir.y * cos);

        return new Vector2Int(newX, newY);
    }
}
