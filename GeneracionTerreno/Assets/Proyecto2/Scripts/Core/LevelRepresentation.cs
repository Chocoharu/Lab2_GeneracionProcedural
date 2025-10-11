using System;
using System.Collections.Generic;

[Serializable]
public class LevelRepresentation
{
    public int width, height;
    public int[,] grid; // 0 suelo, 1 muro, 2 caja, 3 meta, 4 jugador

    public float fitness; // <-- Añade esta línea

    public LevelRepresentation(int w, int h)
    {
        width = w;
        height = h;
        grid = new int[w, h];
        fitness = 0f; // Inicializa fitness
    }

    public LevelRepresentation Clone()
    {
        LevelRepresentation clone = new LevelRepresentation(width, height);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                clone.grid[x, y] = grid[x, y];
        clone.fitness = fitness; // Copia fitness
        return clone;
    }

    // Conversión a genotipo (lista lineal)
    public int[] ToGenotype()
    {
        int[] g = new int[width * height];
        Buffer.BlockCopy(grid, 0, g, 0, g.Length * sizeof(int));
        return g;
    }

    // Desde genotipo a grid
    public static LevelRepresentation FromGenotype(int[] g, int width, int height)
    {
        var level = new LevelRepresentation(width, height);
        Buffer.BlockCopy(g, 0, level.grid, 0, g.Length * sizeof(int));
        return level;
    }
}
