using UnityEngine;

public class CoastAgent
{
    public Vector2Int position; // Posici�n en el mapa
    public int tokens;          // Energ�a o pasos
    public Vector2Int direction; // Direcci�n de avance

    public CoastAgent(Vector2Int startPos, int tokenCount, Vector2Int dir)
    {
        position = startPos;
        tokens = tokenCount;
        direction = dir;
    }
}
