using System.Collections.Generic;
using UnityEngine;

public class EvolutionStrategy
{
    public int mu = 12;
    public int lambda = 36;
    public int generations = 40;
    public int width = 8, height = 8, boxes = 3;

    private List<LevelRepresentation> population = new List<LevelRepresentation>();
    private SolverSokoban solver = new SolverSokoban();

    public void InitializePopulation()
    {
        population.Clear();
        for (int i = 0; i < mu; i++)
        {
            var lvl = BackwardsGenerator.Generate(width, height, boxes, steps: 150);
            EnsureInvariant(lvl);
            lvl = Evaluate(lvl);
            population.Add(lvl);
        }
    }

    public LevelRepresentation Run()
    {
        for (int gen = 0; gen < generations; gen++)
        {
            var children = new List<LevelRepresentation>();
            for (int i = 0; i < lambda; i++)
            {
                var parent = population[Random.Range(0, population.Count)];
                var child = Mutate(parent);
                EnsureInvariant(child);
                child = Evaluate(child);
                children.Add(child);
            }

            // (mu + lambda) selection
            population.AddRange(children);
            population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            if (population.Count > mu) population = population.GetRange(0, mu);
        }

        return population[0];
    }

    // Mutations: move a box to a nearby empty cell, or toggle a wall
    private LevelRepresentation Mutate(LevelRepresentation level)
    {
        var clone = level.Clone();

        // pick mutation type
        float r = Random.value;
        if (r < 0.6f)
        {
            // move a random box to a random nearby empty cell (or random empty)
            var boxesList = clone.FindBoxes();
            if (boxesList.Count > 0)
            {
                var b = boxesList[Random.Range(0, boxesList.Count)];
                // try up to N attempts for local move
                var tries = 0;
                var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                while (tries < 10)
                {
                    tries++;
                    int nx = b.x + dirs[Random.Range(0, dirs.Length)].dx;
                    int ny = b.y + dirs[Random.Range(0, dirs.Length)].dy;
                    if (!clone.IsInside(nx, ny)) continue;
                    if (clone.grid[nx, ny] != 0 && clone.grid[nx, ny] != 3) continue; // must be free or goal
                    // move
                    clone.grid[b.x, b.y] = (clone.grid[b.x, b.y] == 3) ? 3 : 0;
                    clone.grid[nx, ny] = 2;
                    break;
                }
            }
        }
        else if (r < 0.85f)
        {
            // toggle a wall somewhere (but don't overwrite boxes/goals/player)
            int x = Random.Range(1, clone.width - 1);
            int y = Random.Range(1, clone.height - 1);
            if (clone.grid[x, y] == 0) clone.grid[x, y] = 1;
            else if (clone.grid[x, y] == 1) clone.grid[x, y] = 0;
        }
        else
        {
            // move player to random reachable empty cell
            var oldP = clone.FindPlayer();
            for (int tries = 0; tries < 10; tries++)
            {
                int x = Random.Range(1, clone.width - 1);
                int y = Random.Range(1, clone.height - 1);
                if (clone.grid[x, y] == 0)
                {
                    clone.grid[oldP.x, oldP.y] = 0;
                    clone.grid[x, y] = 4;
                    break;
                }
            }
        }

        return clone;
    }

    // Ensure invariants: one player, boxes == goals count (if mismatch, add/remove goals), no overlaps
    private void EnsureInvariant(LevelRepresentation level)
    {
        // ensure single player
        var p = level.FindPlayer();
        if (p.x < 0)
        {
            // place player on first empty
            for (int x = 1; x < level.width; x++)
            {
                for (int y = 1; y < level.height; y++)
                {
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 4; x = level.width; break; }
                }
            }
        }

        // fix overlaps and counts:
        var boxes = level.FindBoxes();
        var goals = level.FindGoals();
        // if different count, adjust goals to match boxes by adding or removing goals
        if (goals.Count < boxes.Count)
        {
            // add goals at empty positions
            for (int x = 1; x < level.width && goals.Count < boxes.Count; x++)
                for (int y = 1; y < level.height && goals.Count < boxes.Count; y++)
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 3; goals.Add((x, y)); }
        }
        else if (goals.Count > boxes.Count)
        {
            // remove extra goals
            int remove = goals.Count - boxes.Count;
            for (int x = 1; x < level.width && remove > 0; x++)
                for (int y = 1; y < level.height && remove > 0; y++)
                    if (level.grid[x, y] == 3) { level.grid[x, y] = 0; remove--; }
        }
    }

    // Evaluate using solver: solvable levels get high score; shorter solution = better fitness
    public LevelRepresentation Evaluate(LevelRepresentation level)
    {
        // Use solver with reasonable limits to avoid exponential work
        int solLen = solver.SolutionLength(level, maxSearchDepth: 300, maxStates: 10000);
        bool solvable = solLen >= 0;

        float fitness = 0f;
        if (!solvable)
        {
            fitness = -1000f + (Random.value * 0.01f); // strong penalty, but allow slight variation
        }
        else
        {
            fitness = 1000f - solLen; // shorter solution => larger fitness
            int wallCount = 0;
            for (int x = 0; x < level.width; x++)
                for (int y = 0; y < level.height; y++)
                    if (level.grid[x, y] == 1) wallCount++;
            fitness += Mathf.Clamp(50 - Mathf.Abs(wallCount - (level.width * level.height / 10)), -20, 20);
        }

        level.fitness = fitness;
        return level;
    }
}

public static class LevelExtensions
{
    public static float fitness;
}