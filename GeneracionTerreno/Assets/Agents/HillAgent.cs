using UnityEngine;

public class HillAgent
{
    private readonly float[,] heightmap;
    private readonly int width, depth;
    private readonly SmoothAgent smoothAgent;

    public HillAgent(float[,] heightmap, SmoothAgent sAgent)
    {
        this.heightmap = heightmap;
        width = heightmap.GetLength(0);
        depth = heightmap.GetLength(1);
        smoothAgent = sAgent;
    }

    // Genera una colina orientada desde basePoint alejándose de mountainCenter
    public void GenerateHill(Vector2Int basePoint, Vector2 mountainCenter, int tokens, float heightIncrement, int smoothRadius, int rPerp, int rAlong)
    {
        Vector2Int loc = Clamp(basePoint);
        Vector2 dirF = ((Vector2)loc - mountainCenter).normalized;
        if (dirF.sqrMagnitude < 1e-4f) dirF = Random.insideUnitCircle.normalized;
        Vector2Int dir = StepFrom(dirF);

        for (int t = 0; t < tokens; t++)
        {
            // Suaviza el punto actual
            float avg = smoothAgent.AverageNeighborhoodPublic(heightmap, loc.x, loc.y, 1);
            heightmap[loc.x, loc.y] = Mathf.Lerp(heightmap[loc.x, loc.y], avg, 0.4f);

            // Eleva una cuña 2D con caída gaussiana
            RaiseWedge2D(loc, dir, heightIncrement, rPerp, rAlong);

            // Avanza y ocasionalmente rota la dirección
            loc += dir;
            loc = Clamp(loc);

            if (t % 18 == 0)
                dir = RotateStep(dir, Random.value < 0.5f ? 15f : -15f);
        }
    }

    // Eleva una región en forma de cuña con caída gaussiana
    private void RaiseWedge2D(Vector2Int center, Vector2Int dir, float inc, int rPerp, int rAlong)
    {
        if (dir == Vector2Int.zero) dir = Vector2Int.up;
        Vector2Int perp = new Vector2Int(-dir.y, dir.x);

        for (int a = -rAlong; a <= rAlong; a++)
        {
            for (int p = -rPerp; p <= rPerp; p++)
            {
                Vector2Int pos = center + dir * a + perp * p;
                if (!Inside(pos)) continue;

                float u = (float)p / rPerp;
                float v = (float)a / rAlong;
                float falloff = Mathf.Exp(-(u * u * 3f + v * v * 1.5f));

                heightmap[pos.x, pos.y] = Mathf.Clamp01(heightmap[pos.x, pos.y] + inc * falloff);
            }
        }
    }

    // Verifica si la posición está dentro de los límites
    private bool Inside(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < depth;

    // Limita la posición a los bordes del mapa
    private Vector2Int Clamp(Vector2Int p) => new Vector2Int(Mathf.Clamp(p.x, 0, width - 1), Mathf.Clamp(p.y, 0, depth - 1));

    // Convierte un vector en una dirección cardinal
    private Vector2Int StepFrom(Vector2 f)
    {
        return Mathf.Abs(f.x) > Mathf.Abs(f.y)
            ? new Vector2Int(f.x >= 0 ? 1 : -1, 0)
            : new Vector2Int(0, f.y >= 0 ? 1 : -1);
    }

    // Rota la dirección cardinal por un ángulo dado
    private Vector2Int RotateStep(Vector2Int dir, float degrees)
    {
        Vector2 v = ((Vector2)dir).normalized;
        float rad = degrees * Mathf.Deg2Rad;
        float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
        Vector2 r = new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs).normalized;

        return Mathf.Abs(r.x) > Mathf.Abs(r.y)
            ? new Vector2Int(r.x >= 0 ? 1 : -1, 0)
            : new Vector2Int(0, r.y >= 0 ? 1 : -1);
    }
}
