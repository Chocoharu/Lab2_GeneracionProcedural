using UnityEngine;

public class SimulatedAnnealing
{
    public float initialTemp = 1.0f;
    public float coolingRate = 0.995f;
    public int iterations = 2000;

    private SolverSokoban solver = new SolverSokoban();

    public LevelRepresentation Run(LevelRepresentation start)
    {
        var current = start.Clone();
        var best = current.Clone();
        float currentFitness = Evaluate(current);
        float bestFitness = currentFitness;

        float T = initialTemp;
        for (int i = 0; i < iterations; i++)
        {
            var neighbor = Mutate(current);
            neighbor = Repair(neighbor); // ensure invariants
            float newFitness = Evaluate(neighbor);

            if (newFitness > currentFitness || Mathf.Exp((newFitness - currentFitness) / T) > Random.value)
            {
                current = neighbor;
                currentFitness = newFitness;
                if (newFitness > bestFitness)
                {
                    best = neighbor.Clone();
                    bestFitness = newFitness;
                }
            }

            T *= coolingRate;
        }

        best.fitness = bestFitness;
        return best;
    }

    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone();
        // small mutation: toggle a wall, or move a box a bit
        float r = Random.value;
        if (r < 0.5f)
        {
            int x = Random.Range(1, clone.width - 1);
            int y = Random.Range(1, clone.height - 1);
            if (clone.grid[x, y] == 0) clone.grid[x, y] = 1;
            else if (clone.grid[x, y] == 1) clone.grid[x, y] = 0;
        }
        else
        {
            var boxes = clone.FindBoxes();
            if (boxes.Count > 0)
            {
                var b = boxes[Random.Range(0, boxes.Count)];
                var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                var d = dirs[Random.Range(0, dirs.Length)];
                int nx = b.x + d.dx, ny = b.y + d.dy;
                if (clone.IsInside(nx, ny) && (clone.grid[nx, ny] == 0 || clone.grid[nx, ny] == 3))
                {
                    clone.grid[b.x, b.y] = (clone.grid[b.x, b.y] == 3) ? 3 : 0;
                    clone.grid[nx, ny] = 2;
                }
            }
        }
        return clone;
    }

    // If something invalid happened (no player etc.), fix it
    private LevelRepresentation Repair(LevelRepresentation level)
    {
        // ensure 1 player; if missing, put on empty
        var p = level.FindPlayer();
        if (p.x < 0)
        {
            for (int x = 1; x < level.width; x++)
                for (int y = 1; y < level.height; y++)
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 4; x = level.width; break; }
        }
        // ensure boxes==goals count - adjust goals
        var boxes = level.FindBoxes();
        var goals = level.FindGoals();
        if (goals.Count < boxes.Count)
        {
            for (int x = 1; x < level.width && goals.Count < boxes.Count; x++)
                for (int y = 1; y < level.height && goals.Count < boxes.Count; y++)
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 3; goals.Add((x, y)); }
        }
        else if (goals.Count > boxes.Count)
        {
            int remove = goals.Count - boxes.Count;
            for (int x = 1; x < level.width && remove > 0; x++)
                for (int y = 1; y < level.height && remove > 0; y++)
                    if (level.grid[x, y] == 3) { level.grid[x, y] = 0; remove--; }
        }
        return level;
    }

    private float Evaluate(LevelRepresentation level)
    {
        // Use solver: solvable gets big positive, shorter solution better
        int solLen = solver.SolutionLength(level, maxSearchDepth: 2000);
        bool solvable = solLen >= 0;
        if (!solvable) return -1000f + Random.value * 0.01f;
        // Score: prefer shorter solution and moderate walls
        float score = 1000f - solLen;
        int wallCount = 0;
        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                if (level.grid[x, y] == 1) wallCount++;
        score += Mathf.Clamp(50 - Mathf.Abs(wallCount - (level.width * level.height / 10)), -20, 20);
        level.fitness = score;
        return score;
    }
}
