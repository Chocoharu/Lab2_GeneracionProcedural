using UnityEngine;

public class CoastAgent
{
    public Vector2Int position; // Posición en el mapa
    public int tokens;          // Energía o pasos
    public Vector2Int direction; // Dirección de avance

    public CoastAgent(Vector2Int startPos, int tokenCount, Vector2Int dir)
    {
        position = startPos;
        tokens = tokenCount;
        direction = dir;
    }
}
