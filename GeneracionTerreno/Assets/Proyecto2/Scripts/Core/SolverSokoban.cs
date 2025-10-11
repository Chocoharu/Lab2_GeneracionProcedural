using System;
using System.Collections.Generic;

public class SolverSokoban
{
    public bool IsSolvable(LevelRepresentation level)
    {
        // TODO: Implementar BFS o IDA*
        return true;
    }

    public int SolutionLength(LevelRepresentation level)
    {
        // Corregido: crear una instancia de Random
        Random random = new Random();
        return random.Next(10, 100);
    }
}
