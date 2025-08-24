using UnityEngine;

public class MountainAgent
{
    public Vector2 position;
    public Vector2 direction;
    public int tokens;
    public float wedgeWidth;
    public float elevation;
    public int smoothRadius;
    public int changeDirEvery; // cada cuántos pasos puede cambiar dirección
    public float maxAngleChange; // en grados

    public MountainAgent(Vector2 startPos, Vector2 dir, int tokenCount, float wedgeWidth, float elevation, int smoothRadius, int changeDirEvery, float maxAngleChange)
    {
        position = startPos;
        direction = dir.normalized;
        tokens = tokenCount;
        this.wedgeWidth = wedgeWidth;
        this.elevation = elevation;
        this.smoothRadius = smoothRadius;
        this.changeDirEvery = changeDirEvery;
        this.maxAngleChange = maxAngleChange;
    }
}
