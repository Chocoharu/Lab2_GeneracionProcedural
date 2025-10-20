using System.Collections.Generic;
using UnityEngine;

public static class HeuristicEvaluator
{
    // Evalúa un LevelRepresentation y devuelve un valor de "fitness" entre 0 y 1.
    // Valores más altos indican, según esta heurística, niveles "mejores" (más distancia media de cajas a metas,
    // pero penalizando deadlocks estáticos como cajas en esquinas sin meta).
    public static float Evaluate(LevelRepresentation level)
    {
        // 1) Obtener dimensiones del nivel
        int width = level.width;
        int height = level.height;

        // 2) Recolectar posiciones de cajas y metas
        var boxes = new List<Vector2Int>();
        var goals = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Asunción de codificación: 0 = suelo, 1 = muro, 2 = caja, 3 = meta, 4 = jugador
                if (level.grid[x, y] == 2)
                    boxes.Add(new Vector2Int(x, y)); // almacenar posición de caja
                else if (level.grid[x, y] == 3)
                    goals.Add(new Vector2Int(x, y)); // almacenar posición de meta
            }
        }

        // 3) Si no hay cajas o no hay metas, no tiene sentido evaluar → fitness 0
        if (boxes.Count == 0 || goals.Count == 0) return 0f;

        // 4) Para cada caja, encontrar la meta más cercana (distancia Manhattan) y sumar
        float totalDist = 0f;
        foreach (var box in boxes)
        {
            float best = float.MaxValue;
            foreach (var goal in goals)
            {
                // Distancia Manhattan: |dx| + |dy|
                float d = Mathf.Abs(box.x - goal.x) + Mathf.Abs(box.y - goal.y);
                if (d < best) best = d;
            }
            totalDist += best;
        }

        // 5) Calcular distancia media por caja
        float avgDist = totalDist / boxes.Count;

        // 6) Detectar penalizaciones por cajas en esquinas sin ser meta (deadlocks estáticos)
        // Recorremos solo celdas interiores para evitar índices fuera de rango.
        int cornerPenalty = 0;
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                // Si hay una caja en esta celda y no es una meta
                if (level.grid[x, y] == 2 && level.grid[x, y] != 3)
                {
                    // Consideramos 4 combinaciones de paredes que forman una esquina alrededor de la caja:
                    // derecha+arriba, izquierda+arriba, derecha+abajo, izquierda+abajo
                    bool corner = (level.grid[x + 1, y] == 1 && level.grid[x, y + 1] == 1) ||
                                  (level.grid[x - 1, y] == 1 && level.grid[x, y + 1] == 1) ||
                                  (level.grid[x + 1, y] == 1 && level.grid[x, y - 1] == 1) ||
                                  (level.grid[x - 1, y] == 1 && level.grid[x, y - 1] == 1);
                    if (corner) cornerPenalty++;
                }
            }
        }


        // 7) Convertir la distancia media a un valor de fitness entre 0 y 1.
        // La fórmula avgDist / (avgDist + 5) hace que el valor se normalice y tenga una saturación.
        // Incrementar avgDist da mayor fitness (según diseño del proyecto).
        float fitness = avgDist / (avgDist + 5f);

        // 8) Aplicar penalización exponencial por cada esquina detectada:
        // Cada penalización reduce el fitness multiplicándolo por exp(-0.3 * cornerPenalty).
        // El coeficiente 0.3 controla la severidad de la penalización.
        fitness *= Mathf.Exp(-0.3f * cornerPenalty);
        int wallAdjacencyPenalty = 0;
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (level.grid[x, y] == 2) // caja
                {
                    // Contar muros adyacentes
                    int adjacentWalls = 0;
                    if (level.grid[x + 1, y] == 1) adjacentWalls++;
                    if (level.grid[x - 1, y] == 1) adjacentWalls++;
                    if (level.grid[x, y + 1] == 1) adjacentWalls++;
                    if (level.grid[x, y - 1] == 1) adjacentWalls++;
                    wallAdjacencyPenalty += adjacentWalls;
                }
            }
        }

        // Penalización exponencial por cajas pegadas a muros
        fitness *= Mathf.Exp(-0.15f * wallAdjacencyPenalty);

        // 9) Asegurar que el resultado esté en [0,1]
        return Mathf.Clamp01(fitness);
    }
}
