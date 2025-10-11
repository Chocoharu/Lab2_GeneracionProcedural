using UnityEngine;

public class SimulatedAnnealing
{
    public float initialTemp = 1.0f;
    public float coolingRate = 0.99f;
    public int iterations = 1000;

    public LevelRepresentation Run(LevelRepresentation start)
    {
        LevelRepresentation current = start.Clone();
        float currentFitness = Evaluate(current);

        float T = initialTemp;
        for (int i = 0; i < iterations; i++)
        {
            LevelRepresentation neighbor = Mutate(current);
            float newFitness = Evaluate(neighbor);

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

    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone();
        int x = Random.Range(0, level.width);
        int y = Random.Range(0, level.height);
        if (clone.grid[x, y] == 0)
            clone.grid[x, y] = 1; // ejemplo: agregar muro
        else if (clone.grid[x, y] == 1)
            clone.grid[x, y] = 0; // quitar muro
        return clone;
    }

    private float Evaluate(LevelRepresentation level)
    {
        // Reutilizar SolverSokoban aquí
        return Random.Range(0f, 1f);
    }
}
