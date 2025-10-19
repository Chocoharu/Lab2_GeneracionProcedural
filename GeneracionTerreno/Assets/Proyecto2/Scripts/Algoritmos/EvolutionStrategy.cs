using System.Collections.Generic;
using UnityEngine;

public class EvolutionStrategy
{
    public int mu = UIManager.Instance.esMu;
    public int lambda = UIManager.Instance.esLambda;
    public int generations = UIManager.Instance.esGenerations;

    // Población actual de niveles
    private List<LevelRepresentation> population = new();

    // Inicializa la población con 'mu' individuos generados aleatoriamente
    public void InitializePopulation(int width, int height, int boxes)
    {
        population.Clear();
        for (int i = 0; i < mu; i++)
        {
            // Genera un nivel y lo añade a la población
            population.Add(BackwardsGenerator.Generate(width, height, boxes));
        }
    }

    // Ejecuta el algoritmo evolution strategy y devuelve el mejor nivel encontrado
    public LevelRepresentation Run()
    {
        for (int gen = 0; gen < generations; gen++)
        {
            List<LevelRepresentation> children = new();

            // Generar 'lambda' hijos por selección aleatoria y mutación
            for (int i = 0; i < lambda; i++)
            {
                var parent = population[Random.Range(0, population.Count)];
                var child = Mutate(parent);
                child = Evaluate(child); // asigna fitness al hijo
                children.Add(child);
            }

            // Selección (μ+λ): combinar y quedarnos con los mejores 'mu'
            population.AddRange(children);
            // Ordenar por fitness descendente (los mejores primero)
            population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            // Recortar la lista a los 'mu' mejores individuos
            if (population.Count > mu)
                population = population.GetRange(0, mu);
        }

        // Devolver el mejor individuo (suponiendo que siempre hay al menos uno)
        return population[0];
    }

    // Realiza una mutación simple: mover una caja aleatoria a una casilla vacía adyacente
    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone(); // trabajar sobre una copia
        int attempts = 0;
        int width = level.width;
        int height = level.height;

        // Recopilar posiciones de cajas (valor 2)
        var boxPositions = new List<(int x, int y)>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (clone.grid[x, y] == 2) boxPositions.Add((x, y));

        // Si no hay cajas, no hay mutación posible
        if (boxPositions.Count == 0) return clone;

        // Intentar mover una caja hasta 10 veces
        while (attempts < 10)
        {
            attempts++;
            var b = boxPositions[Random.Range(0, boxPositions.Count)];
            int dir = Random.Range(0, 4);
            // Calcular nueva posición según dirección
            int nx = b.x + (dir == 0 ? 1 : dir == 1 ? -1 : 0);
            int ny = b.y + (dir == 2 ? 1 : dir == 3 ? -1 : 0);

            // Evitar mover fuera de los límites o a bordes (paredes)
            if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1) continue;

            // Si la casilla destino está vacía (0), mover la caja
            if (clone.grid[nx, ny] == 0)
            {
                clone.grid[b.x, b.y] = 0;
                clone.grid[nx, ny] = 2;
                break; // mutación realizada
            }
        }

        return clone;
    }

    // Evalúa un nivel y asigna su fitness usando el evaluador heurístico
    private LevelRepresentation Evaluate(LevelRepresentation level)
    {
        float fitness = HeuristicEvaluator.Evaluate(level);
        level.fitness = fitness;
        return level;
    }
}

public static class LevelExtensions
{
    public static float fitness;
}