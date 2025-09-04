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

    // Sugerencia: pasa el centro de la montaña para orientar la colina hacia afuera
    public void GenerateHill(Vector2Int basePoint, Vector2 mountainCenter, int tokens, float heightIncrement, int smoothRadius, int rPerp, int rAlong)
    {
        Vector2Int loc = Clamp(basePoint);
        // Dirección “lejos” del centro de la montaña
        Vector2 dirF = ((Vector2)loc - mountainCenter).normalized;
        if (dirF.sqrMagnitude < 1e-4f) dirF = Random.insideUnitCircle.normalized;
        Vector2Int dir = StepFrom(dirF);


        for (int t = 0; t < tokens; t++)
        {
            // Suaviza un poco el punto actual para integrarlo (paso barato)
            float avg = smoothAgent.AverageNeighborhoodPublic(heightmap, loc.x, loc.y, 1);
            heightmap[loc.x, loc.y] = Mathf.Lerp(heightmap[loc.x, loc.y], avg, 0.4f);

            // Eleva una cuña 2D (perp + a lo largo) con caída suave
            RaiseWedge2D(loc, dir, heightIncrement, rPerp, rAlong);

            // Avanza; añade curvatura suave ocasional
            loc += dir;
            loc = Clamp(loc);

            if (t % 18 == 0) // curva leve para evitar líneas rectas
                dir = RotateStep(dir, Random.value < 0.5f ? 15f : -15f);
        }
    }

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

                // Caída gaussiana: más alto en el centro, muere hacia bordes
                float u = (float)p / rPerp;     // transversal
                float v = (float)a / rAlong;    // longitudinal
                float falloff = Mathf.Exp(-(u * u * 3f + v * v * 1.5f));

                heightmap[pos.x, pos.y] = Mathf.Clamp01(heightmap[pos.x, pos.y] + inc * falloff);
            }
        }
    }

    // Helpers
    private bool Inside(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < depth;
    private Vector2Int Clamp(Vector2Int p) => new Vector2Int(Mathf.Clamp(p.x, 0, width - 1), Mathf.Clamp(p.y, 0, depth - 1));

    private Vector2Int StepFrom(Vector2 f)
    {
        // Convierte un vector float a un paso cardinal (N/S/E/O)
        return Mathf.Abs(f.x) > Mathf.Abs(f.y)
            ? new Vector2Int(f.x >= 0 ? 1 : -1, 0)
            : new Vector2Int(0, f.y >= 0 ? 1 : -1);
    }

    private Vector2Int RotateStep(Vector2Int dir, float degrees)
    {
        // Rota en float y vuelve a una celda cardinal
        Vector2 v = ((Vector2)dir).normalized;
        float rad = degrees * Mathf.Deg2Rad;
        float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
        Vector2 r = new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs).normalized;

        // Escoge el eje dominante tras la rotación
        return Mathf.Abs(r.x) > Mathf.Abs(r.y)
            ? new Vector2Int(r.x >= 0 ? 1 : -1, 0)
            : new Vector2Int(0, r.y >= 0 ? 1 : -1);
    }
}
