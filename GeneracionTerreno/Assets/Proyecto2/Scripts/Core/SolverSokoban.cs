using System;
using System.Collections.Generic;

public class SolverSokoban
{
    // Retorna true si existe solución (usando BFS sobre empujes)
    public bool IsSolvable(LevelRepresentation level)
    {
        return SolutionLength(level) >= 0;
    }

    // Retorna la longitud mínima en número de empujes; -1 si no es resoluble
    public int SolutionLength(LevelRepresentation level)
    {
        int width = level.width;
        int height = level.height;

        // Encuentra posición jugador y posiciones de cajas
        int playerPos = -1;
        List<int> boxes = new List<int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int v = level.grid[x, y];
                if (v == 4) playerPos = PosToIndex(x, y, width);
                if (v == 2) boxes.Add(PosToIndex(x, y, width));
            }
        }

        if (playerPos < 0 || boxes.Count == 0) return -1;

        var solver = new PushBFS(level, playerPos, boxes);
        return solver.Run();
    }

    // Convierte coordenadas a índice plano
    private static int PosToIndex(int x, int y, int width) => x + y * width;

    // Clase interna que ejecuta BFS sobre estados definidos por posiciones de cajas y posición del jugador,
    // pero los nodos de BFS son estados tras empujes (coste = 1 por empuje). Se calcula el conjunto de casillas alcanzables
    // por el jugador sin empujar para generar posibles empujes.
    private class PushBFS
    {
        private LevelRepresentation level;
        private int width, height;
        private int startPlayer;
        private List<int> startBoxes;

        private readonly int[] dx = new int[] { 1, -1, 0, 0 };
        private readonly int[] dy = new int[] { 0, 0, 1, -1 };

        public PushBFS(LevelRepresentation level, int playerPos, List<int> boxes)
        {
            this.level = level;
            this.width = level.width;
            this.height = level.height;
            this.startPlayer = playerPos;
            this.startBoxes = new List<int>(boxes);
            this.startBoxes.Sort();
        }

        // Ejecuta BFS y devuelve número mínimo de empujes o -1
        public int Run()
        {
            var visited = new HashSet<string>();
            var q = new Queue<Node>();
            var startKey = KeyFromState(startBoxes, startPlayer);
            q.Enqueue(new Node { boxes = startBoxes, player = startPlayer, pushes = 0 });
            visited.Add(startKey);

            while (q.Count > 0)
            {
                var node = q.Dequeue();

                // Check goal: todas las cajas sobre casillas meta (valor 3)
                if (AllBoxesOnGoals(node.boxes))
                    return node.pushes;

                // calcula casillas alcanzables por el jugador sin mover cajas
                var reachable = ComputeReachable(node.player, node.boxes);

                // para cada caja, si el jugador puede situarse en su lado opuesto, y la casilla de empuje está libre, generar nuevo estado
                for (int i = 0; i < node.boxes.Count; i++)
                {
                    int boxIndex = node.boxes[i];
                    int bx = boxIndex % width;
                    int by = boxIndex / width;

                    for (int dir = 0; dir < 4; dir++)
                    {
                        int px = bx - dx[dir];
                        int py = by - dy[dir];
                        int tx = bx + dx[dir];
                        int ty = by + dy[dir];

                        if (!InBounds(px, py) || !InBounds(tx, ty)) continue;

                        int pIndex = PosToIndex(px, py, width);
                        int tIndex = PosToIndex(tx, ty, width);

                        // El jugador debe poder alcanzar px,py (lado desde donde empuja)
                        if (!reachable.Contains(pIndex)) continue;

                        // Celda destino tx,ty debe estar libre (no muro ni caja)
                        if (!IsFree(tx, ty, node.boxes)) continue;

                        // Genera nuevo arreglo de cajas con esta caja movida a destino
                        var newBoxes = new List<int>(node.boxes);
                        newBoxes[i] = tIndex;
                        newBoxes.Sort();
                        int newPlayerPos = boxIndex; // jugador queda donde estaba la caja tras el empuje

                        string key = KeyFromState(newBoxes, newPlayerPos);
                        if (visited.Add(key))
                        {
                            q.Enqueue(new Node { boxes = newBoxes, player = newPlayerPos, pushes = node.pushes + 1 });
                        }
                    }
                }
            }

            return -1; // no solucion
        }

        private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        // Chequea que la casilla (x,y) no es muro y no está ocupada por una caja
        private bool IsFree(int x, int y, List<int> boxes)
        {
            if (level.grid[x, y] == 1) return false; // muro
            int idx = PosToIndex(x, y, width);
            for (int i = 0; i < boxes.Count; i++) if (boxes[i] == idx) return false;
            return true;
        }

        // Devuelve conjunto de índices alcanzables por el jugador sin empujar cajas (flood fill)
        private HashSet<int> ComputeReachable(int playerIndex, List<int> boxes)
        {
            var reachable = new HashSet<int>();
            var stack = new Stack<int>();
            reachable.Add(playerIndex);
            stack.Push(playerIndex);

            var boxSet = new HashSet<int>(boxes);

            while (stack.Count > 0)
            {
                int cur = stack.Pop();
                int cx = cur % width;
                int cy = cur / width;

                for (int k = 0; k < 4; k++)
                {
                    int nx = cx + dx[k];
                    int ny = cy + dy[k];
                    if (!InBounds(nx, ny)) continue;
                    int nidx = PosToIndex(nx, ny, width);
                    if (reachable.Contains(nidx)) continue;
                    if (level.grid[nx, ny] == 1) continue; // muro
                    if (boxSet.Contains(nidx)) continue;   // caja bloquea el paso
                    reachable.Add(nidx);
                    stack.Push(nidx);
                }
            }

            return reachable;
        }

        private bool AllBoxesOnGoals(List<int> boxes)
        {
            foreach (var b in boxes)
            {
                int bx = b % width;
                int by = b / width;
                if (level.grid[bx, by] != 3) return false;
            }
            return true;
        }

        private static string KeyFromState(List<int> boxes, int player)
        {
            // cajas ordenadas + jugador
            return string.Join(",", boxes) + "|" + player;
        }

        private static int PosToIndex(int x, int y, int width) => x + y * width;

        private class Node
        {
            public List<int> boxes;
            public int player;
            public int pushes;
        }
    }
}
