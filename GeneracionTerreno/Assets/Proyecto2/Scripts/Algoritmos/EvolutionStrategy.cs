using System.Collections.Generic;
using UnityEngine;

public class EvolutionStrategy
{
    public int mu = 10;
    public int lambda = 30;
    public int generations = 50;

    private List<LevelRepresentation> population = new();

    public void InitializePopulation(int width, int height, int boxes)
    {
        population.Clear();
        for (int i = 0; i < mu; i++)
        {
            population.Add(BackwardsGenerator.Generate(width, height, boxes));
        }
    }

    public LevelRepresentation Run()
    {
        for (int gen = 0; gen < generations; gen++)
        {
            List<LevelRepresentation> children = new();

            for (int i = 0; i < lambda; i++)
            {
                var parent = population[Random.Range(0, population.Count)];
                var child = Mutate(parent);
                child = Evaluate(child);
                children.Add(child);
            }

            // Selección (μ+λ)
            population.AddRange(children);
            population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            population = population.GetRange(0, mu);
        }

        return population[0];
    }

    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone();
        // Ejemplo simple: mover una caja al azar
        int x = Random.Range(0, level.width);
        int y = Random.Range(0, level.height);
        if (clone.grid[x, y] == 2)
        {
            clone.grid[x, y] = 0;
            clone.grid[(x + 1) % level.width, y] = 2;
        }
        return clone;
    }

    private LevelRepresentation Evaluate(LevelRepresentation level)
    {
        // TODO: usar SolverSokoban para medir solvabilidad y dificultad
        level.fitness = Random.Range(0f, 1f); // temporal
        return level;
    }
}

public static class LevelExtensions
{
    public static float fitness;
}