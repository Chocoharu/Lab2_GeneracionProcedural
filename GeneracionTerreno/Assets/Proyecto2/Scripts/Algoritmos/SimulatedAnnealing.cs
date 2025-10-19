using UnityEngine;
using System.Collections.Generic;

public class SimulatedAnnealing
{
    // Parámetros de SA tomados de la interfaz (UIManager). Se asume que UIManager.Instance está inicializado antes de usar esta clase.
    public float initialTemp = UIManager.Instance.saInitialTemp;   // Temperatura inicial
    public float coolingRate = UIManager.Instance.saCoolingRate;   // Factor multiplicativo por iteración (<1 para enfriar)
    public int iterations = UIManager.Instance.saIterations;       // Número de iteraciones del algoritmo
    public float desiredWallDensity = UIManager.Instance.saDesiredWallDensity; // Densidad de muros objetivo (0..1)

    // Ejecuta el algoritmo de Recocido Simulado sobre un nivel de partida.
    public LevelRepresentation Run(LevelRepresentation start)
    {
        // Trabajar sobre una copia para no mutar el original.
        LevelRepresentation current = start.Clone();
        float currentFitness = Evaluate(current); // Evaluar la solución inicial

        float T = initialTemp;
        for (int i = 0; i < iterations; i++)
        {
            // Generar vecino mediante una mutación local
            LevelRepresentation neighbor = Mutate(current);
            float newFitness = Evaluate(neighbor);

            // Regla de aceptación de SA:
            // - Aceptar si es mejor
            // - Si es peor, aceptar con probabilidad exp((new - current)/T)
            if (newFitness > currentFitness || Mathf.Exp((newFitness - currentFitness) / T) > Random.value)
            {
                current = neighbor;
                currentFitness = newFitness;
            }

            // Enfriamiento: reducir la temperatura
            T *= coolingRate;
        }

        // Guardar fitness final en la representación y devolverla
        current.fitness = currentFitness;
        return current;
    }

    // Realiza una mutación intentando hasta 20 veces encontrar una celda válida para alternar suelo <-> muro.
    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone();
        int tries = 0;

        // Evitar modificar las celdas del borde (bordes fijos) y evitar modificar cajas(2), metas(3) o jugador(4).
        while (tries < 20)
        {
            tries++;
            int x = Random.Range(1, level.width - 1);   // interior: 1 .. width-2
            int y = Random.Range(1, level.height - 1);  // interior: 1 .. height-2

            int cell = clone.grid[x, y];
            if (cell == 2 || cell == 3 || cell == 4) continue; // No modificar cajas, metas ni jugador

            // Alternar: si es suelo(0) -> muro(1); si es muro -> suelo
            clone.grid[x, y] = (cell == 0) ? 1 : 0;
            break;
        }

        return clone;
    }

    // Evalúa la calidad (fitness) de un nivel combinando varias métricas.
    private float Evaluate(LevelRepresentation level)
    {
        // Calcular celdas interiores (sin bordes) para estimar el número ideal de muros
        int interiorCells = Mathf.Max(1, (level.width - 2) * (level.height - 2));
        // desiredWallCount: número entero de muros deseados, limitado entre 1 y todas las celdas interiores
        int desiredWallCount = Mathf.Clamp(Mathf.RoundToInt(desiredWallDensity * interiorCells), 1, interiorCells);

        // --- 1) Cantidad de muros actuales ---
        int wallCount = 0;
        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                if (level.grid[x, y] == 1)
                    wallCount++;

        // wallBalance: penaliza desviaciones respecto al número de muros deseado.
        // Se usa una función exponencial suave para evitar penalizaciones abruptas.
        float wallBalance = Mathf.Exp(-Mathf.Abs(wallCount - desiredWallCount) / (float)Mathf.Max(1, desiredWallCount));

        // --- 2) Celdas accesibles desde la posición del jugador ---
        int reachable = CountReachableCells(level);
        // Proporción del área total alcanzable (incluye bordes, normalización sencilla)
        float areaCoverage = reachable / (float)(level.width * level.height);

        // --- 3) Complejidad del camino ---
        // Mide la distancia máxima alcanzable desde el jugador (proxy para longitud de caminos)
        float pathComplexity = ComputePathComplexity(level);

        // --- 4) Combinar métricas con pesos ---
        // Ajustar los pesos según se prefiera más exploración, balance de muros o complejidad del camino.
        float fitness = 0.5f * areaCoverage + 0.3f * wallBalance + 0.2f * pathComplexity;

        // Penalización adicional si hay muy pocas celdas alcanzables (posible encierro)
        if (reachable < (level.width * level.height * 0.05f))
            fitness *= 0.5f;

        // Guardar y devolver el fitness entre 0 y 1
        level.fitness = Mathf.Clamp01(fitness);
        return level.fitness;
    }

    // Cuenta las celdas alcanzables desde la posición del jugador (valor 4 en grid) usando BFS.
    private int CountReachableCells(LevelRepresentation level)
    {
        int startX = -1, startY = -1;
        // Buscar la posición del jugador
        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                if (level.grid[x, y] == 4)
                {
                    startX = x; startY = y;
                    break;
                }

        if (startX == -1) return 0; // Si no hay jugador, no hay celdas alcanzables

        bool[,] visited = new bool[level.width, level.height];
        Queue<(int, int)> q = new();
        q.Enqueue((startX, startY));
        visited[startX, startY] = true;

        int count = 1; // contar la celda inicial (jugador)
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var (cx, cy) = q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];
                // Evitar salir del interior (se considera que bordes externos no son explorables aquí)
                if (nx <= 0 || ny <= 0 || nx >= level.width - 1 || ny >= level.height - 1) continue;
                if (visited[nx, ny]) continue;
                if (level.grid[nx, ny] == 1) continue; // muro bloqueante
                visited[nx, ny] = true;
                q.Enqueue((nx, ny));
                count++;
            }
        }

        return count;
    }

    // Calcula una medida de complejidad basada en la longitud máxima de camino alcanzable desde el jugador.
    private float ComputePathComplexity(LevelRepresentation level)
    {
        int startX = -1, startY = -1;
        // Buscar posición del jugador
        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                if (level.grid[x, y] == 4)
                {
                    startX = x;
                    startY = y;
                    break;
                }

        if (startX == -1) return 0; // Sin jugador no tiene sentido calcular complejidad

        // BFS para obtener la distancia máxima (longest) alcanzable desde el jugador
        bool[,] visited = new bool[level.width, level.height];
        Queue<(int, int, int)> q = new();
        q.Enqueue((startX, startY, 0));
        visited[startX, startY] = true;

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
                if (visited[nx, ny]) continue;
                if (level.grid[nx, ny] == 1) continue;
                visited[nx, ny] = true;
                q.Enqueue((nx, ny, dist + 1));
            }
        }

        // Normalizar la longitud máxima por una aproximación del tamaño del nivel (width + height).
        // Esto produce un valor en 0..1 que representa "qué tan largo" es el camino en relación con el tamaño.
        return Mathf.Clamp01(longest / (float)(level.width + level.height));
    }
}
