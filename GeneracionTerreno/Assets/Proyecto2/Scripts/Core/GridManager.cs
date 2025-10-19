using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public GameObject floorPrefab;

    private GameObject currentParent; // Contenedor del nivel

    public void GenerateLevel(LevelRepresentation level)
    {
        //  1. Limpiar nivel anterior
        if (currentParent != null)
            Destroy(currentParent);
        currentParent = new GameObject("GeneratedLevel");

        float tileSize = 1f;
        float wallHeight = 1f;
        float boxHeight = 0.5f;
        float goalHeight = 0.01f;
        float playerHeight = 0.5f;

        //  2. Crear nuevo nivel
        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                Vector3 basePos = new Vector3(x * tileSize, 0, y * tileSize);
                int tile = level.grid[x, y];

                // Siempre generar suelo
                GameObject floor = Instantiate(floorPrefab, basePos, Quaternion.identity, currentParent.transform);

                switch (tile)
                {
                    case 1: // Wall
                        Instantiate(wallPrefab, basePos + Vector3.up * wallHeight / 2, Quaternion.identity, currentParent.transform);
                        break;
                    case 2: // Box
                        Instantiate(boxPrefab, basePos + Vector3.up * boxHeight, Quaternion.identity, currentParent.transform);
                        break;
                    case 3: // Goal
                        Instantiate(goalPrefab, basePos + Vector3.up * goalHeight, Quaternion.identity, currentParent.transform);
                        break;
                    case 4: // Player
                        Instantiate(playerPrefab, basePos + Vector3.up * playerHeight, Quaternion.identity, currentParent.transform);
                        break;
                }
            }
        }
    }
}
