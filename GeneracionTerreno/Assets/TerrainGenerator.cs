using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Configuración del mapa")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    private float[,] heightmap;
    [SerializeField] private float heightScale;
    [SerializeField] private float terrainScale;

    [Header("Parámetros de generación")]
    [SerializeField] private int cantMontains;
    [SerializeField] private float poderMontanasMin;
    [SerializeField] private float poderMontanasMax;
    [SerializeField] private int radioMontanasMin;
    [SerializeField] private int radioMontanasMax;

    [Header("Parámetros de colinas")]
    [SerializeField] private int cantHills;
    [SerializeField] private float poderColinasMin;
    [SerializeField] private float poderColinasMax;
    [SerializeField] private int radioColinasMin;
    [SerializeField] private int radioColinasMax;

    [Header("Parámetros de ríos")]
    [SerializeField] private int cantRios;
    [SerializeField] private int anchoRio;
    [SerializeField] private float profundidadRio;

    [Header("Parámetros de costa")]
    [SerializeField] private float escalaRuidoCosta;
    [SerializeField] private float intensidadRuidoCosta;

    [Header("Parámetros de playa")]
    [SerializeField] private int anchoPlaya;
    [SerializeField] private float alturaPlaya;

    [Header("Parámetros de suavizado")]
    [SerializeField] private int iteracionesSuavizado;

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
            float poder = Random.Range(poderMontanasMin, poderMontanasMax);
            int radio = Random.Range(radioMontanasMin, radioMontanasMax);

            AgenteMontaña(rx, ry, poder, radio);
        }

        // 3b. Agregar colinas
        for (int i = 0; i < cantHills; i++)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);
            float poder = Random.Range(poderColinasMin, poderColinasMax);
            int radio = Random.Range(radioColinasMin, radioColinasMax);

            AgenteColina(rx, ry, poder, radio);
        }

        // 3c. Agregar ríos
        for (int i = 0; i < cantRios; i++)
        {
            int x0 = Random.Range(width / 4, 3 * width / 4);
            int y0 = 0;
            int x1 = Random.Range(width / 4, 3 * width / 4);
            int y1 = height - 1;

            AgenteRio(x0, y0, x1, y1, anchoRio, profundidadRio);
        }

        // 4. Agregar playa
        AgentePlaya(anchoPlaya, alturaPlaya);

        // 5. Suavizar el heightmap
        SuavizarHeightmap(iteracionesSuavizado);

        CrearMesh();

        AplicarColores();
    }

    void GenerarCosta()
    {
        float maxDist = Vector2.Distance(Vector2.zero, new Vector2(width / 2, height / 2));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distX = x - width / 2f;
                float distY = y - height / 2f;
                float distCentro = Mathf.Sqrt(distX * distX + distY * distY);

                float alturaBase = 1f - (distCentro / maxDist);
                float ruido = Mathf.PerlinNoise(x * escalaRuidoCosta, y * escalaRuidoCosta);
                alturaBase += (ruido - 0.5f) * intensidadRuidoCosta;

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
                int distBorde = Mathf.Min(x, y, width - 1 - x, height - 1 - y);
                if (distBorde < anchoPlaya)
                {
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
        List<int> trianglesList = new List<int>();
        Vector2[] uvs = new Vector2[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float z = heightmap[x, y] * heightScale;
                int idx = x + y * width;
                vertices[idx] = new Vector3(x * terrainScale, z, y * terrainScale);
                uvs[idx] = new Vector2(x / (float)(width - 1), y / (float)(height - 1));

            }
        }

        float radioIsla = Mathf.Min(width, height) * 0.5f * 0.95f;
        float centroX = width / 2f;
        float centroY = height / 2f;

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                float cx = x + 0.5f - centroX;
                float cy = y + 0.5f - centroY;
                float dist = Mathf.Sqrt(cx * cx + cy * cy);

                if (dist < radioIsla)
                {
                    int i = x + y * width;
                    trianglesList.Add(i);
                    trianglesList.Add(i + width);
                    trianglesList.Add(i + width + 1);
                    trianglesList.Add(i);
                    trianglesList.Add(i + width + 1);
                    trianglesList.Add(i + 1);
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = trianglesList.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
    }
    void AplicarColores()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

        Material material = mr.material;
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            mr.material = material;
        }

        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float altura = heightmap[x, y];
                Color color;

                if (altura < 0.3f) // Agua
                    color = Color.blue;
                else if (altura < 0.4f) // Playa
                    color = Color.yellow;
                else if (altura < 0.6f) // Tierra
                    color = Color.green;
                else if (altura < 0.8f) // Montañas
                    color = Color.gray;
                else // Picos nevados
                    color = Color.white;

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        material.mainTexture = texture;
    }
}
