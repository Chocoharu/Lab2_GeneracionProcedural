using System;
using System.Collections.Generic;
using System.Linq;

public class SolverSokoban
{
    // Direcciones posibles (derecha, izquierda, arriba, abajo)
    private (int x, int y)[] dirs = new (int, int)[]
    {
        (1, 0), (-1, 0), (0, 1), (0, -1)
    };

    // Detecta un "deadlock" estático (una caja atrapada en una esquina que no es meta)
    private bool IsStaticDeadlock(int bx, int by, LevelRepresentation level)
    {
        if (level.grid[bx, by] == 3) return false; // 3 = meta, no hay problema

        bool leftWall = !level.IsInside(bx - 1, by) || level.grid[bx - 1, by] == 1;
        bool rightWall = !level.IsInside(bx + 1, by) || level.grid[bx + 1, by] == 1;
        bool upWall = !level.IsInside(bx, by + 1) || level.grid[bx, by + 1] == 1;
        bool downWall = !level.IsInside(bx, by - 1) || level.grid[bx, by - 1] == 1;

        // esquina: pared a izquierda+arriba, izquierda+abajo, derecha+arriba o derecha+abajo
        return (leftWall && upWall) || (leftWall && downWall) ||
               (rightWall && upWall) || (rightWall && downWall);
    }

    // Crea una clave única del estado (jugador + cajas)
    private string StateKey((int x, int y) player, List<(int x, int y)> boxes)
    {
        var sorted = boxes.Select(b => $"{b.x},{b.y}").OrderBy(s => s);
        return $"{player.x},{player.y}|" + string.Join(";", sorted);
    }

    // Verifica si las posiciones de cajas coinciden con las metas
    private bool BoxesMatchGoals(HashSet<(int x, int y)> boxes, List<(int x, int y)> goals)
    {
        if (boxes.Count != goals.Count) return false;
        foreach (var g in goals)
            if (!boxes.Contains(g)) return false;
        return true;
    }

    // Comprueba si el nivel es resolvible (BFS con detección de deadlocks)
    public bool IsSolvable(LevelRepresentation level, int maxStates = 20000, int maxDepth = 500)
    {
        var player = level.FindPlayer();
        if (player.x < 0) return false;

        var goals = level.FindGoals();
        var boxesList = level.FindBoxes();
        if (boxesList.Count != goals.Count) return false;

        foreach (var b in boxesList)
            if (IsStaticDeadlock(b.x, b.y, level)) return false;

        var q = new Queue<((int x, int y) player, List<(int x, int y)> boxes, int depth)>();
        var seen = new HashSet<string>();

        q.Enqueue((player, boxesList, 0));
        seen.Add(StateKey(player, boxesList));

        int processed = 0;
        while (q.Count > 0)
        {
            var (p, boxes, depth) = q.Dequeue();
            processed++;
            if (processed > maxStates) return false; // cortar y considerar no solvable por limite

            var boxSet = new HashSet<(int, int)>(boxes);
            if (BoxesMatchGoals(boxSet, goals)) return true;
            if (depth > maxDepth) continue;

            var reachable = PlayerReachable(p, boxes, level);

            for (int i = 0; i < boxes.Count; i++)
            {
                var b = boxes[i];
                foreach (var d in dirs)
                {
                    int pxNeeded = b.x - d.x;
                    int pyNeeded = b.y - d.y;
                    int bxNew = b.x + d.x;
                    int byNew = b.y + d.y;

                    if (!level.IsInside(pxNeeded, pyNeeded) || !level.IsInside(bxNew, byNew))
                        continue;

                    if (!reachable.Contains((pxNeeded, pyNeeded))) continue;
                    if (level.grid[bxNew, byNew] == 1) continue;
                    if (boxes.Any(bb => bb.x == bxNew && bb.y == byNew)) continue;

                    var newBoxes = new List<(int x, int y)>(boxes);
                    newBoxes[i] = (bxNew, byNew);

                    if (IsStaticDeadlock(bxNew, byNew, level)) continue;

                    var newPlayer = (b.x, b.y);
                    var key = StateKey(newPlayer, newBoxes);
                    if (seen.Contains(key)) continue;

                    seen.Add(key);
                    q.Enqueue((newPlayer, newBoxes, depth + 1));
                }
            }
        }

        return false;
    }

    // Calcula la distancia mínima de solución (en número de empujes)
    public int SolutionLength(LevelRepresentation level, int maxSearchDepth = 500, int maxStates = 20000)
    {
        var player = level.FindPlayer();
        if (player.x < 0) return -1;

        var goals = level.FindGoals();
        var boxesList = level.FindBoxes();
        if (boxesList.Count != goals.Count) return -1;

        var q = new Queue<((int x, int y) player, List<(int x, int y)> boxes, int depth)>();
        var seen = new HashSet<string>();

        q.Enqueue((player, boxesList, 0));
        seen.Add(StateKey(player, boxesList));

        int processed = 0;
        while (q.Count > 0)
        {
            var (p, boxes, depth) = q.Dequeue();
            processed++;
            if (processed > maxStates) return -1; // corte: demasiado grande

            var boxSet = new HashSet<(int, int)>(boxes);
            if (BoxesMatchGoals(boxSet, goals)) return depth;
            if (depth > maxSearchDepth) continue;

            var reachable = PlayerReachable(p, boxes, level);

            for (int i = 0; i < boxes.Count; i++)
            {
                var b = boxes[i];
                foreach (var d in dirs)
                {
                    int pxNeeded = b.x - d.x;
                    int pyNeeded = b.y - d.y;
                    int bxNew = b.x + d.x;
                    int byNew = b.y + d.y;

                    if (!level.IsInside(pxNeeded, pyNeeded) || !level.IsInside(bxNew, byNew))
                        continue;

                    if (!reachable.Contains((pxNeeded, pyNeeded))) continue;
                    if (level.grid[bxNew, byNew] == 1) continue;
                    if (boxes.Any(bb => bb.x == bxNew && bb.y == byNew)) continue;

                    var newBoxes = new List<(int x, int y)>(boxes);
                    newBoxes[i] = (bxNew, byNew);
                    if (IsStaticDeadlock(bxNew, byNew, level)) continue;

                    var newPlayer = (b.x, b.y);
                    var key = StateKey(newPlayer, newBoxes);
                    if (seen.Contains(key)) continue;

                    seen.Add(key);
                    q.Enqueue((newPlayer, newBoxes, depth + 1));
                }
            }
        }

        return -1;
    }

    // Celdas alcanzables para el jugador (sin mover cajas)
    private HashSet<(int x, int y)> PlayerReachable((int x, int y) player, List<(int x, int y)> boxes, LevelRepresentation level)
    {
        var blocked = new HashSet<(int, int)>(boxes);
        var seen = new HashSet<(int, int)>();
        var stack = new Stack<(int x, int y)>(); // Especificar nombres aquí
        stack.Push(player);
        seen.Add(player);

        while (stack.Count > 0)
        {
            var p = stack.Pop();
            foreach (var d in dirs)
            {
                int nx = p.x + d.x;  // Ahora p tiene campos x e y
                int ny = p.y + d.y;
                if (!level.IsInside(nx, ny)) continue;
                if (level.grid[nx, ny] == 1) continue;
                if (blocked.Contains((nx, ny))) continue;
                if (!seen.Contains((nx, ny)))
                {
                    seen.Add((nx, ny));
                    stack.Push((nx, ny));
                }
            }
        }
        return seen;
    }
}
