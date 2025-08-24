using UnityEngine;

public class SmoothAgent
{
    public Vector2Int position;
    public int tokens;

    public SmoothAgent(Vector2Int startPos, int tokenCount)
    {
        position = startPos;
        tokens = tokenCount;
    }
}
