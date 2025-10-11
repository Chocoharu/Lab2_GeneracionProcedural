using UnityEngine;

public class BackwardsGenerator
{
    public static LevelRepresentation Generate(int width, int height, int numBoxes)
    {
        LevelRepresentation level = new LevelRepresentation(width, height);

        // Paso 1: generar metas aleatorias
        for (int i = 0; i < numBoxes; i++)
        {
            int gx = Random.Range(1, width - 1);
            int gy = Random.Range(1, height - 1);
            level.grid[gx, gy] = 3;
        }

        // Paso 2: “retroceder” empujes simples (simplificado)
        // → aquí se puede implementar la lógica backward real

        // Paso 3: colocar jugador aleatoriamente
        level.grid[Random.Range(1, width - 1), Random.Range(1, height - 1)] = 4;

        return level;
    }
}
