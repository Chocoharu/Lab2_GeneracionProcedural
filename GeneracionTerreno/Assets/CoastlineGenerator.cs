using System.Collections.Generic;
using UnityEngine;

public class CoastlineGenerator : MonoBehaviour
{
    public int width = 128;
    public int height = 128;
    private float[,] heightmap;

    private List<Vector2Int> coastPoints = new List<Vector2Int>();

    void Start()
    {
        heightmap = new float[width, height];

        CoastAgent root = new CoastAgent(
            new Vector2Int(width / 2, height / 2),
            100, // tokens iniciales
            Vector2Int.right
        );
        SmoothAgent smoothAgent = new SmoothAgent(
            new Vector2Int(width / 2, height / 2),
            100 // tokens iniciales para suavizado
        );
        MountainAgent mountain = new MountainAgent(
            new Vector2(width / 2, height / 2), // punto de inicio
            Random.insideUnitCircle.normalized, // dirección aleatoria
            60,                                 // tokens (longitud de la montaña)
            8,                                  // ancho de la cresta
            0.5f,                               // elevación por paso
            2,                                  // radio de suavizado
            10,                                 // cada cuántos pasos puede cambiar dirección
            45f                                 // máximo ángulo de cambio (grados)
        );

        CoastlineGenerate(root);
        MountainAgentRun(mountain);
        SmoothAgentRun(smoothAgent);

        CrearMeshTerreno(); // Crear el mesh de la costa
    }

    void CoastlineGenerate(CoastAgent agent)
    {
        if (agent.tokens > 10)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2Int randomDir = GetRandomDirection();
                Vector2Int borderPoint = GetRandomBorder(agent.position);

                CoastAgent child = new CoastAgent(
                    borderPoint,
                    agent.tokens / 2,
                    randomDir
                );

                CoastlineGenerate(child);
            }
        }
        else
        {
            for (int t = 0; t < agent.tokens; t++)
            {
                Vector2Int p = GetRandomBorder(agent.position);

                Vector2Int best = p;
                float bestScore = -999f;

                foreach (Vector2Int adj in GetAdjacents(p))
                {
                    float score = ScorePoint(adj);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = adj;
                    }
                }

                if (IsInsideMap(best))
                {
                    heightmap[best.x, best.y] = 1f;
                    coastPoints.Add(best); // almacenar para Gizmos
                }
            }
        }
    }

    void SmoothAgentRun(SmoothAgent agent)
    {
        Vector2Int location = agent.position;
        for (int t = 0; t < agent.tokens; t++)
        {
            float suma = 0f;
            int cuenta = 0;
            foreach (Vector2Int adj in GetAdjacentsInclSelf(location))
            {
                if (IsInsideMap(adj))
                {
                    suma += heightmap[adj.x, adj.y];
                    cuenta++;
                }
            }
            if (cuenta > 0)
                heightmap[location.x, location.y] = suma / cuenta;

            List<Vector2Int> vecinos = GetAdjacents(location);
            location = vecinos[Random.Range(0, vecinos.Count)];
            if (!IsInsideMap(location))
                location = agent.position;
        }
    }

    void MountainAgentRun(MountainAgent agent)
    {
        Vector2 originalDir = agent.direction;
        for (int t = 0; t < agent.tokens; t++)
        {
            Vector2 perp = new Vector2(-agent.direction.y, agent.direction.x);
            int centerX = Mathf.RoundToInt(agent.position.x);
            int centerY = Mathf.RoundToInt(agent.position.y);

            for (float w = -agent.wedgeWidth / 2f; w <= agent.wedgeWidth / 2f; w += 1f)
            {
                int wx = Mathf.RoundToInt(centerX + perp.x * w);
                int wy = Mathf.RoundToInt(centerY + perp.y * w);
                if (wx >= 0 && wx < width && wy >= 0 && wy < height)
                {
                    heightmap[wx, wy] += agent.elevation;
                }
            }

            for (int sx = -agent.smoothRadius; sx <= agent.smoothRadius; sx++)
            {
                for (int sy = -agent.smoothRadius; sy <= agent.smoothRadius; sy++)
                {
                    int nx = centerX + sx;
                    int ny = centerY + sy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        float suma = 0f;
                        int cuenta = 0;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int px = nx + dx;
                                int py = ny + dy;
                                if (px >= 0 && px < width && py >= 0 && py < height)
                                {
                                    suma += heightmap[px, py];
                                    cuenta++;
                                }
                            }
                        }
                        if (cuenta > 0)
                            heightmap[nx, ny] = suma / cuenta;
                    }
                }
            }

            agent.position += agent.direction;

            if (agent.changeDirEvery > 0 && t % agent.changeDirEvery == 0 && t > 0)
            {
                float angle = Random.Range(-agent.maxAngleChange, agent.maxAngleChange);
                agent.direction = Quaternion.Euler(0, 0, angle) * originalDir;
                agent.direction.Normalize();
            }
        }
    }

    List<Vector2Int> GetAdjacentsInclSelf(Vector2Int pos)
    {
        var list = GetAdjacents(pos);
        list.Add(pos);
        return list;
    }

    Vector2Int GetRandomDirection()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        return dirs[Random.Range(0, dirs.Length)];
    }

    Vector2Int GetRandomBorder(Vector2Int pos)
    {
        List<Vector2Int> borders = GetAdjacents(pos);
        return borders[Random.Range(0, borders.Count)];
    }

    List<Vector2Int> GetAdjacents(Vector2Int pos)
    {
        return new List<Vector2Int> {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        };
    }

    float ScorePoint(Vector2Int p)
    {
        float cx = width / 2f;
        float cy = height / 2f;
        float dist = Vector2.Distance(new Vector2(p.x, p.y), new Vector2(cx, cy));
        return -dist;
    }

    bool IsInsideMap(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    }

    void CrearMeshTerreno()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[width * height];
        Vector2[] uvs = new Vector2[width * height];
        List<int> triangles = new List<int>();

        // Crear vértices y UVs
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int idx = x + y * width;
                vertices[idx] = new Vector3(x, heightmap[x, y], y);
                uvs[idx] = new Vector2(x / (float)(width - 1), y / (float)(height - 1));
            }
        }

        // Crear triángulos
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                int i = x + y * width;
                triangles.Add(i);
                triangles.Add(i + width);
                triangles.Add(i + width + 1);

                triangles.Add(i);
                triangles.Add(i + width + 1);
                triangles.Add(i + 1);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard")) { color = Color.green };
    }
}
