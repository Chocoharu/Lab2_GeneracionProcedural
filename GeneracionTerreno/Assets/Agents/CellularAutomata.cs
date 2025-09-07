using UnityEngine;

public class CellularAutomata
{
    public int width;
    public int height;
    public int steps;
    public float fillProbability;

    public void ApplyToHeightmapMasked(float[,] heightmap, bool[,] mask)
    {
        width = heightmap.GetLength(0);
        height = heightmap.GetLength(1);
        bool[,] grid = new bool[width, height];

        // Inicialización aleatoria solo en la zona de la máscara
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (mask[x, y])
                    grid[x, y] = Random.value < fillProbability;

        for (int step = 0; step < steps; step++)
        {
            bool[,] newGrid = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!mask[x, y])
                    {
                        newGrid[x, y] = grid[x, y];
                        continue;
                    }
                    int alive = CountAliveNeighbors(grid, x, y);
                    // Cambia la regla para mayor variabilidad y naturalidad
                    if (grid[x, y])
                        newGrid[x, y] = alive >= 3 && alive <= 6;
                    else
                        newGrid[x, y] = alive == 5;
                }
            }
            grid = newGrid;
        }

        // Aplica los resultados solo en la zona de la máscara
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (mask[x, y])
                    heightmap[x, y] = grid[x, y] ? heightmap[x, y] : 0.0f;
    }

    private int CountAliveNeighbors(bool[,] grid, int x, int y)
    {
        int count = 0;
        for (int nx = x - 1; nx <= x + 1; nx++)
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx == x && ny == y) continue;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (grid[nx, ny]) count++;
            }
        return count;
    }
}
