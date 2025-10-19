using System;
using System.Collections.Generic;

[Serializable]
public class LevelRepresentation
{
    public int width, height;
    
    //Representacion Positiva del Entorno
    public int[,] grid; // 0 suelo, 1 muro, 2 caja, 3 meta, 4 jugador

    public float fitness; // <-- Añade esta línea
    
    public LevelRepresentation(int w, int h)
    {
        if (w < 1 || h < 1) throw new ArgumentException("Dimensiones Invalidas");
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
        int i = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                g[i++] = grid[x, y];
            }
        }
        return g;
    }

    public static LevelRepresentation FromGenotype(int[] g, int width, int height)
    {
        if (g == null || g.Length != width * height) throw new ArgumentException("Genotype size mismatch.");
        var level = new LevelRepresentation(width, height);
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                level.grid[x, y] = g[i++];
        return level;
    }

    // Utilities
    public (int x, int y) FindPlayer()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 4) return (x, y);
        return (-1, -1);
    }

    public List<(int x, int y)> FindBoxes()
    {
        var list = new List<(int, int)>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 2) list.Add((x, y));
        return list;
    }

    public List<(int x, int y)> FindGoals()
    {
        var list = new List<(int, int)>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 3) list.Add((x, y));
        return list;
    }

    public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;
}
