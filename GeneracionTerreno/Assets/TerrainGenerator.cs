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

        // 4. Suavizar el heightmap
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
