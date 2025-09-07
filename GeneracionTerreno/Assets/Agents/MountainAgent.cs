using UnityEngine;

public class MountainAgent
{
    private float[,] heightmap;
    private int width, depth;
    private SmoothAgent smoothAgent;

    // Inicializa el agente de montaña con el mapa de alturas y el agente de suavizado
    public MountainAgent(float[,] heightmap, SmoothAgent sAgent)
    {
        this.heightmap = heightmap;
        this.width = heightmap.GetLength(0);
        this.depth = heightmap.GetLength(1);
        this.smoothAgent = sAgent;
    }

    // Genera una montaña a partir de un punto inicial, modificando el mapa de alturas
    public void GenerateMountain(Vector2Int start, int tokens, float heightIncrement, int smoothRadius, int directionChangeInterval, int radius)
    {
        Vector2Int location = start;
        Vector2Int direction = RandomDirection();

        for (int i = 0; i < tokens; i++)
        {
            RaiseWedge(location, direction, heightIncrement, radius); // Eleva una zona en forma de "cuña"
            Smooth(location, smoothRadius); // Suaviza la zona elevada
            location += direction; // Avanza en la dirección actual

            // Cambia la dirección cada cierto número de pasos
            if (i % directionChangeInterval == 0)
            {
                float angle = Random.Range(-45f, 45f);
                direction = RotateDirection(direction, angle);
                if (direction == Vector2Int.zero) direction = RandomDirection();
            }
        }
    }

    // Eleva una zona circular alrededor de la ubicación dada
    private void RaiseWedge(Vector2Int location, Vector2Int direction, float increment, int radius)
    {
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

    // Aplica suavizado en el área alrededor de la ubicación dada
    private void Smooth(Vector2Int location, int radius)
    {
        smoothAgent.SmoothArea(heightmap, location, radius);
    }

    // Devuelve una dirección aleatoria cardinal
    private Vector2Int RandomDirection()
    {
        Vector2Int[] dirs = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };
        return dirs[UnityEngine.Random.Range(0, dirs.Length)];
    }

    // Rota la dirección actual por un ángulo dado en grados
    private Vector2Int RotateDirection(Vector2Int dir, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        float newX = dir.x * cos - dir.y * sin;
        float newY = dir.x * sin + dir.y * cos;

        Vector2 v = new Vector2(newX, newY).normalized;
        return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }
}
