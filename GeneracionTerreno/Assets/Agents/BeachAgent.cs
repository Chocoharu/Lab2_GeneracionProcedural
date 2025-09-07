using UnityEngine;

// Representa un agente que genera y suaviza zonas de playa en un mapa de alturas.
public class BeachAgent
{
    public Vector2Int position;   // Posición actual en el mapa
    public int tokens;            // Energía restante para avanzar
    public int walkLength;        // Pasos tierra adentro por iteración
    private SmoothAgent smoothAgent; // Referencia al agente de suavizado
    private float minHeight;      // Altura mínima de la playa
    private float maxHeight;      // Altura máxima de la playa

    public BeachAgent(Vector2Int startPos, int tokenCount, int walk, SmoothAgent sAgent, float minHeight, float maxHeight)
    {
        position = startPos;
        tokens = tokenCount;
        walkLength = walk;
        smoothAgent = sAgent;
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
    }

    // Modifica el mapa de alturas para crear una zona de playa y suavizarla.
    public void GenerateBeach(float[,] heightMap)
    {
        int sizeX = heightMap.GetLength(0);
        int sizeY = heightMap.GetLength(1);

        while (tokens > 0)
        {
            tokens--;

            // Si la posición actual está cerca del agua, reubica el agente.
            if (heightMap[position.x, position.y] < 0.35f)
            {
                position = new Vector2Int(
                    Mathf.Clamp(position.x + Random.Range(-2, 3), 0, sizeX - 1),
                    Mathf.Clamp(position.y + Random.Range(-2, 3), 0, sizeY - 1)
                );
            }

            // Aplana la zona costera en la posición actual.
            FlattenArea(heightMap, position, 3, minHeight, maxHeight);

            // Suaviza la zona costera usando SmoothAgent.
            smoothAgent.RunFromPoint(heightMap, position, 5);

            // Calcula una nueva posición tierra adentro.
            Vector2Int inland = position + new Vector2Int(Random.Range(-1, 2), Random.Range(1, 3));
            inland.x = Mathf.Clamp(inland.x, 0, sizeX - 1);
            inland.y = Mathf.Clamp(inland.y, 0, sizeY - 1);

            // Realiza el paseo tierra adentro, modificando y suavizando el terreno.
            for (int i = 0; i < walkLength; i++)
            {
                FlattenArea(heightMap, inland, 2, 0.18f, 0.25f);
                smoothAgent.RunFromPoint(heightMap, inland, 3);

                // Movimiento aleatorio en cada paso.
                inland += new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
                inland.x = Mathf.Clamp(inland.x, 0, sizeX - 1);
                inland.y = Mathf.Clamp(inland.y, 0, sizeY - 1);
            }
        }
    }

    // Aplana el área alrededor de un punto, interpolando entre minHeight y maxHeight según la distancia al centro.
    private void FlattenArea(float[,] map, Vector2Int center, int radius, float minHeight, float maxHeight)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = Mathf.Clamp(center.x + x, 0, map.GetLength(0) - 1);
                int ny = Mathf.Clamp(center.y + y, 0, map.GetLength(1) - 1);

                float dist = Mathf.Sqrt(x * x + y * y) / radius; // Proporción de distancia al centro
                float targetHeight = Mathf.Lerp(maxHeight, minHeight, dist);

                map[nx, ny] = Mathf.Lerp(map[nx, ny], targetHeight, 0.7f);
            }
        }
    }
}
