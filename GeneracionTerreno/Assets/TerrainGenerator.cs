using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Configuración del mapa")]
    public int width;
    public int height;
    public float[,] heightmap;
    [SerializeField] private float heightScale;
    [SerializeField] private float terrainScale;
    [SerializeField] private int cantMontains;
    [SerializeField] private int cantHills;
    [SerializeField] private int cantRios;

    void Start()
    {
        // 1. Inicializar heightmap
        heightmap = new float[width, height];

        // 2. Generar costa radial
        GenerarCosta();

        // 3. Agregar montañas
        for (int i = 0; i < cantMontains; i++)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);
            float poder = Random.Range(0.2f, 0.6f); // qué tan alta
            int radio = Random.Range(5, 15);        // tamaño de la montaña

            AgenteMontaña(rx, ry, poder, radio);
        }
        // 3b. Agregar colinas
        for (int i = 0; i < cantHills; i++)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);
            float poder = Random.Range(0.05f, 0.15f); // colinas más bajas
            int radio = Random.Range(8, 20);          // colinas más anchas
            AgenteColina(rx, ry, poder, radio);
        }
        // 3c. Agregar ríos
        for (int i = 0; i < cantRios; i++)
        {
            // Río de arriba a abajo, posición aleatoria en X
            int x0 = Random.Range(width / 4, 3 * width / 4);
            int y0 = 0;
            int x1 = Random.Range(width / 4, 3 * width / 4);
            int y1 = height - 1;
            int ancho = 3; // ancho del río
            float profundidad = 0.2f; // profundidad del río
            AgenteRio(x0, y0, x1, y1, ancho, profundidad);
        }

        // 4. Agregar playa
        AgentePlaya(5, 0.1f);

        // 5. Suavizar el heightmap
        SuavizarHeightmap(5);

        CrearMesh();
    }

    void GenerarCosta()
    {
        float maxDist = Vector2.Distance(Vector2.zero, new Vector2(width / 2, height / 2));
        float escalaRuido = 0.1f; // cuanto más chico, más grandes las formas
        float intensidadRuido = 0.4f; // cuánto afecta el ruido

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distX = x - width / 2f;
                float distY = y - height / 2f;
                float distCentro = Mathf.Sqrt(distX * distX + distY * distY);

                // 1 en el centro, 0 en los bordes
                float alturaBase = 1f - (distCentro / maxDist);

                // Agregar ruido Perlin
                float ruido = Mathf.PerlinNoise(x * escalaRuido, y * escalaRuido);
                alturaBase += (ruido - 0.5f) * intensidadRuido;


                // Clamping para evitar valores negativos
                alturaBase = Mathf.Clamp01(alturaBase);

                heightmap[x, y] = alturaBase;
            }
        }
    }
    void SuavizarHeightmap(int iteraciones)
    {
        for (int it = 0; it < iteraciones; it++)
        {
            float[,] temp = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float suma = 0f;
                    int cuenta = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                suma += heightmap[nx, ny];
                                cuenta++;
                            }
                        }
                    }
                    temp[x, y] = suma / cuenta;
                }
            }
            heightmap = temp;
        }
    }
    void AgenteMontaña(int cx, int cy, float poder, int radio)
    {
        for (int dx = -radio; dx <= radio; dx++)
        {
            for (int dy = -radio; dy <= radio; dy++)
            {
                int x = cx + dx;
                int y = cy + dy;

                // Asegurar que no se salga del mapa
                if (x < 0 || x >= width || y < 0 || y >= height) continue;

                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < radio)
                {
                    float alturaExtra = poder * (1 - dist / radio);
                    heightmap[x, y] += alturaExtra;
                }
            }
        }
    }
    void AgentePlaya(int anchoPlaya, float alturaPlaya)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Distancia mínima al borde
                int distBorde = Mathf.Min(x, y, width - 1 - x, height - 1 - y);
                if (distBorde < anchoPlaya)
                {
                    // Interpolación suave entre la altura original y la altura de playa
                    float t = 1f - (distBorde / (float)anchoPlaya);
                    heightmap[x, y] = Mathf.Lerp(heightmap[x, y], alturaPlaya, t);
                }
            }
        }
    }
    void AgenteColina(int cx, int cy, float poder, int radio)
    {
        for (int dx = -radio; dx <= radio; dx++)
        {
            for (int dy = -radio; dy <= radio; dy++)
            {
                int x = cx + dx;
                int y = cy + dy;
                if (x < 0 || x >= width || y < 0 || y >= height) continue;

                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < radio)
                {
                    // Colinas más suaves (curva cuadrática)
                    float alturaExtra = poder * Mathf.Pow(1 - dist / radio, 2);
                    heightmap[x, y] += alturaExtra;
                }
            }
        }
    }
    void AgenteRio(int x0, int y0, int x1, int y1, int ancho, float profundidad)
    {
        int pasos = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
        for (int i = 0; i <= pasos; i++)
        {
            float t = i / (float)pasos;
            // Interpolación lineal + ruido para hacer el río sinuoso
            float nx = Mathf.Lerp(x0, x1, t) + Mathf.PerlinNoise(i * 0.1f, 0) * 6f - 3f;
            float ny = Mathf.Lerp(y0, y1, t) + Mathf.PerlinNoise(0, i * 0.1f) * 6f - 3f;
            int cx = Mathf.RoundToInt(nx);
            int cy = Mathf.RoundToInt(ny);

            for (int dx = -ancho; dx <= ancho; dx++)
            {
                for (int dy = -ancho; dy <= ancho; dy++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x < 0 || x >= width || y < 0 || y >= height) continue;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < ancho)
                    {
                        // Rebaja la altura para simular el cauce
                        float rebaja = profundidad * (1 - dist / ancho);
                        heightmap[x, y] -= rebaja;
                    }
                }
            }
        }
    }
    void CrearMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];

        // Crear vértices
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float z = heightmap[x, y] * heightScale;
                vertices[x + y * width] = new Vector3(x * terrainScale, z, y * terrainScale);
            }
        }

        // Crear triángulos
        int t = 0;
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                int i = x + y * width;
                // Primer triángulo
                triangles[t++] = i;
                triangles[t++] = i + width;
                triangles[t++] = i + width + 1;
                // Segundo triángulo
                triangles[t++] = i;
                triangles[t++] = i + width + 1;
                triangles[t++] = i + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Asignar el mesh a un MeshFilter y MeshRenderer
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
    }
}
