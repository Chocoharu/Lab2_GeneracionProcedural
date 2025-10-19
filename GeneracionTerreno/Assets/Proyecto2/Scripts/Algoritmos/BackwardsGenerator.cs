using UnityEngine;
using System.Collections.Generic;

public class BackwardsGenerator
{
    public static LevelRepresentation Generate(int width, int height, int numBoxes)
    {
        var level = new LevelRepresentation(width, height);

        // --- Paso 0: Bordes exteriores ---
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                level.grid[x, y] = (x == 0 || y == 0 || x == width - 1 || y == height - 1) ? 1 : 0;

        var rnd = new System.Random();

        // --- Paso 1: Colocar metas aleatorias y guardarlas en una lista ---
        // Usamos una lista para poder restaurar las metas visibles más tarde.
        var goalPositions = new List<(int x, int y)>();
        int attempts = 0;
        // Intentar hasta colocar 'numBoxes' metas o agotar intentos
        while (goalPositions.Count < numBoxes && attempts < 500)
        {
            attempts++;
            int gx = rnd.Next(1, width - 1);  // dentro de los bordes
            int gy = rnd.Next(1, height - 1);
            // Solo colocar meta en celdas vacías (0)
            if (level.grid[gx, gy] == 0)
            {
                goalPositions.Add((gx, gy));
                level.grid[gx, gy] = 3; // marcar meta visible temporalmente con código 3
            }
        }

        // --- Paso 2: Estado resuelto: colocar cajas encima de las metas (2) ---
        // Esto crea la configuración final desde la que realizaremos movimientos inversos.
        foreach (var g in goalPositions)
            level.grid[g.x, g.y] = 2; // caja sobre meta

        // --- Paso 3: Colocar jugador inicial en una celda vacía (4) ---
        bool playerPlaced = false;
        attempts = 0;
        // Intentar hasta colocar el jugador o agotar intentos
        while (!playerPlaced && attempts < 500)
        {
            attempts++;
            int px = rnd.Next(1, width - 1);
            int py = rnd.Next(1, height - 1);
            // Colocar jugador solo en celdas vacías
            if (level.grid[px, py] == 0)
            {
                level.grid[px, py] = 4;
                playerPlaced = true;
            }
        }

        // --- Paso 4: Ejecutar movimientos inversos válidos ---
        // Determinar cuántos pasos inversos intentar. Se escala con numBoxes y se clampa.
        int reverseSteps = Mathf.Clamp(numBoxes * 6, 10, 100);
        for (int step = 0; step < reverseSteps; step++)
        {
            // Recolectar posiciones actuales de las cajas (código 2)
            var boxes = new List<(int x, int y)>();
            for (int x = 1; x < width - 1; x++)
                for (int y = 1; y < height - 1; y++)
                    if (level.grid[x, y] == 2)
                        boxes.Add((x, y));

            // Si no hay cajas, no hay movimientos que ejecutar
            if (boxes.Count == 0) break;

            // Elegir una caja al azar
            var chosen = boxes[rnd.Next(boxes.Count)];

            // Elegir una dirección al azar (0: +x, 1: -x, 2: +y, 3: -y)
            int dir = rnd.Next(4);
            int dx = (dir == 0 ? 1 : dir == 1 ? -1 : 0);
            int dy = (dir == 2 ? 1 : dir == 3 ? -1 : 0);

            // nx,ny = destino donde "se movería" la caja en el movimiento inverso
            // px,py = posición detrás de la caja (donde debería estar el jugador tras revertir)
            int nx = chosen.x + dx;
            int ny = chosen.y + dy;
            int px = chosen.x - dx;
            int py = chosen.y - dy;

            // Verificar límites para ambas posiciones
            if (!InBounds(nx, ny, width, height) || !InBounds(px, py, width, height))
                continue;

            // El destino debe estar libre (suelo 0) o una meta visible (3)
            if (level.grid[nx, ny] != 0 && level.grid[nx, ny] != 3) continue;

            // La posición detrás de la caja (donde se colocará el jugador) también debe estar libre o ser meta
            if (level.grid[px, py] != 0 && level.grid[px, py] != 3) continue;

            // --- Realizar el movimiento inverso ---
            // Borrar cualquier jugador existente (solo debe haber uno). Se borra en todo el interior del mapa.
            for (int x = 1; x < width - 1; x++)
                for (int y = 1; y < height - 1; y++)
                    if (level.grid[x, y] == 4)
                        level.grid[x, y] = 0;

            // Mover la caja desde su posición actual al destino calculado
            level.grid[chosen.x, chosen.y] = 0; // quitar caja de la casilla original
            level.grid[nx, ny] = 2;             // colocar caja en la casilla destino

            // Colocar jugador en la posición detrás de la caja (punto desde donde "empujó")
            level.grid[px, py] = 4;
        }

        // --- Paso 5: Restaurar metas visibles (sin borrar cajas encima) ---
        // Si una meta quedó ocupada por una caja o jugador no la sobrescribimos.
        foreach (var g in goalPositions)
        {
            if (level.grid[g.x, g.y] == 0)
                level.grid[g.x, g.y] = 3;
        }

        // --- Paso 6: Garantizar que exista al menos un jugador ---
        bool hasPlayer = false;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (level.grid[x, y] == 4)
                    hasPlayer = true;

        // Si no se colocó jugador por alguna razón, buscar una celda vacía interior y colocar uno.
        if (!hasPlayer)
        {
            for (int x = 1; x < width - 1 && !hasPlayer; x++)
                for (int y = 1; y < height - 1 && !hasPlayer; y++)
                    if (level.grid[x, y] == 0)
                    {
                        level.grid[x, y] = 4;
                        hasPlayer = true;
                    }
        }

        return level;
    }

    /// <summary>
    /// Comprueba si una coordenada (x,y) está dentro de los límites del grid.
    /// </summary>
    private static bool InBounds(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
}
