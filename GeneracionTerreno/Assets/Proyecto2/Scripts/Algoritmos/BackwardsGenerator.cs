using UnityEngine;
using System.Collections.Generic;
public class BackwardsGenerator
{
    public static LevelRepresentation Generate(int width, int height, int numBoxes, int steps = 200)
    {
        var level = new LevelRepresentation(width, height);

        // 1) Borde con paredes
        for (int x = 0; x < width; x++)
        {
            level.grid[x, 0] = 1;
            level.grid[x, height - 1] = 1;
        }
        for (int y = 0; y < height; y++)
        {
            level.grid[0, y] = 1;
            level.grid[width - 1, y] = 1;
        }

        // 2) Obtener numBoxes posiciones de metas en el interior, sin solapamiento
        var emptyPositions = new List<(int x, int y)>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                emptyPositions.Add((x, y));
        // shuffle
        for (int i = 0; i < emptyPositions.Count; i++)
        {
            int j = Random.Range(i, emptyPositions.Count);
            var tmp = emptyPositions[i];
            emptyPositions[i] = emptyPositions[j];
            emptyPositions[j] = tmp;
        }

        for (int i = 0; i < numBoxes && i < emptyPositions.Count; i++)
        {
            var p = emptyPositions[i];
            level.grid[p.x, p.y] = 3; // meta
            level.grid[p.x, p.y] = 3;
        }

        // Place boxes on goals initially
        var goals = level.FindGoals();
        for (int i = 0; i < goals.Count; i++)
        {
            var g = goals[i];
            level.grid[g.x, g.y] = 2; // box in goal location (we will mark both; rendering uses priority)
        }

        // place player near a random goal
        if (goals.Count > 0)
        {
            var g = goals[Random.Range(0, goals.Count)];
            // place player to any adjacent empty cell if possible, otherwise on goal cell (will be adjusted)
            var candidates = new List<(int x, int y)>();
            var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var d in dirs)
            {
                int nx = g.x + d.dx;
                int ny = g.y + d.dy;
                if (level.IsInside(nx, ny) && level.grid[nx, ny] == 0) candidates.Add((nx, ny));
            }
            if (candidates.Count > 0) { var p = candidates[Random.Range(0, candidates.Count)]; level.grid[p.x, p.y] = 4; }
            else level.grid[g.x, g.y] = 4;
        }

        // Perform random backward-like moves: move a random box into a random adjacent empty cell (simulate reverse of a push)
        for (int step = 0; step < steps; step++)
        {
            var boxes = level.FindBoxes();
            if (boxes.Count == 0) break;
            var b = boxes[Random.Range(0, boxes.Count)];
            var directions = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            var shuffled = new List<(int dx, int dy)>(directions);
            // shuffle
            for (int i = 0; i < shuffled.Count; i++)
            {
                int j = Random.Range(i, shuffled.Count);
                var tmp = shuffled[i]; shuffled[i] = shuffled[j]; shuffled[j] = tmp;
            }

            foreach (var d in shuffled)
            {
                int tx = b.x + d.dx;
                int ty = b.y + d.dy;
                if (!level.IsInside(tx, ty)) continue;
                if (level.grid[tx, ty] != 0 && level.grid[tx, ty] != 3) continue; // only move to empty or goals
                // perform move: remove original box, place box in tx,ty; preserve meta if any
                level.grid[b.x, b.y] = (level.grid[b.x, b.y] == 3) ? 3 : 0; // if there was a goal under box, restore it; otherwise empty
                level.grid[tx, ty] = 2; // box moved
                // place player behind the moved box to make configuration plausible
                int px = b.x - d.dx;
                int py = b.y - d.dy;
                if (level.IsInside(px, py) && level.grid[px, py] == 0) // only if empty
                {
                    // clear previous player
                    var oldPlayer = level.FindPlayer();
                    if (oldPlayer.x >= 0) level.grid[oldPlayer.x, oldPlayer.y] = 0;
                    level.grid[px, py] = 4;
                }
                break; // do only one move this step
            }
        }

        // final: ensure there's exactly 1 player and boxes==goals count
        var finalPlayer = level.FindPlayer();
        if (finalPlayer.x < 0)
        {
            // place player in first available empty cell
            for (int x = 1; x < width - 1 && finalPlayer.x < 0; x++)
                for (int y = 1; y < height - 1 && finalPlayer.x < 0; y++)
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 4; finalPlayer = (x, y); }
        }

        return level;
    }
}
