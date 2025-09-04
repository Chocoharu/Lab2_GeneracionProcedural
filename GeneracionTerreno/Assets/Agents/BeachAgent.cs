using UnityEngine;

public class BeachAgent
{
    public Vector2Int position;   // Posición actual en el mapa
    public int tokens;            // Energía (pasos que puede dar)
    public int walkLength;        // Cuánto avanza tierra adentro
    private SmoothAgent smoothAgent; // Agente de suavizado
    private float minHeight;
    private float maxHeight;

    public BeachAgent(Vector2Int startPos, int tokenCount, int walk, SmoothAgent sAgent, float minHeight, float maxHeight)
    {
        position = startPos;
        tokens = tokenCount;
        walkLength = walk;
        smoothAgent = sAgent;
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
    }

    public void GenerateBeach(float[,] heightMap)
    {
        int sizeX = heightMap.GetLength(0);
        int sizeY = heightMap.GetLength(1);

        while (tokens > 0)
        {
            tokens--;

            // --- 1. Reubicar si estamos demasiado cerca del agua ---
            if (heightMap[position.x, position.y] < 0.35f)
            {
                position = new Vector2Int(
                    Mathf.Clamp(position.x + Random.Range(-2, 3), 0, sizeX - 1),
                    Mathf.Clamp(position.y + Random.Range(-2, 3), 0, sizeY - 1)
                );
            }

            // --- 2. Aplanar área costera ---
            FlattenArea(heightMap, position, 3, minHeight, maxHeight);

            // --- 3. Suavizar con SmoothAgent ---
            smoothAgent.RunFromPoint(heightMap, position, 5);

            // --- 4. Mover tierra adentro ---
            Vector2Int inland = position + new Vector2Int(Random.Range(-1, 2), Random.Range(1, 3));
            inland.x = Mathf.Clamp(inland.x, 0, sizeX - 1);
            inland.y = Mathf.Clamp(inland.y, 0, sizeY - 1);

            // --- 5. Paseo hacia adentro ---
            for (int i = 0; i < walkLength; i++)
            {
                FlattenArea(heightMap, inland, 2, 0.18f, 0.25f);
                smoothAgent.RunFromPoint(heightMap, inland, 3);

                // Paso aleatorio
                inland += new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
                inland.x = Mathf.Clamp(inland.x, 0, sizeX - 1);
                inland.y = Mathf.Clamp(inland.y, 0, sizeY - 1);
            }
        }
    }

    // --- Aplanar zona ---
    private void FlattenArea(float[,] map, Vector2Int center, int radius, float minHeight, float maxHeight)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = Mathf.Clamp(center.x + x, 0, map.GetLength(0) - 1);
                int ny = Mathf.Clamp(center.y + y, 0, map.GetLength(1) - 1);

                float dist = Mathf.Sqrt(x * x + y * y) / radius; // 0 en centro, 1 en borde
                float targetHeight = Mathf.Lerp(maxHeight, minHeight, dist);

                map[nx, ny] = Mathf.Lerp(map[nx, ny], targetHeight, 0.7f);
            }
        }
    }
}
