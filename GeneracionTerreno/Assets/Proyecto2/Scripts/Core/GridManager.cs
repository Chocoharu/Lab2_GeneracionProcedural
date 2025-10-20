using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject wallPrefab, floorPrefab, boxPrefab, goalPrefab, playerPrefab;

    private GameObject[,] gridObjects;

    public void GenerateLevel(int[,] levelData)
    {
        width = UIManager.Instance.levelWidth;
        height = UIManager.Instance.levelHeight;
        ClearGrid();
        gridObjects = new GameObject[width, height];

        // Añadir validación y asegurar que no heredamos una escala que deforme posiciones
        if (levelData.GetLength(0) != width || levelData.GetLength(1) != height)
            Debug.LogWarning($"levelData size ({levelData.GetLength(0)},{levelData.GetLength(1)}) != width/height ({width},{height})");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Posición del suelo
                Vector3 posFloor = new Vector3(x, 0f, y);

                // Instanciar el suelo y hacerlo hijo preservando la posición mundial
                GameObject floor = Instantiate(floorPrefab, posFloor, Quaternion.identity);
                floor.transform.SetParent(transform, true); // true preserva la posición mundial

                // Determinar altura del objeto según tipo
                float objY;
                switch (levelData[x, y])
                {
                    case 1: // wall
                        objY = 1.1f;
                        gridObjects[x, y] = Instantiate(wallPrefab, new Vector3(x, objY, y), Quaternion.identity, transform);
                        break;
                    case 2: // box
                        objY = 0.7f;
                        gridObjects[x, y] = Instantiate(boxPrefab, new Vector3(x, objY, y), Quaternion.identity, transform);
                        break;
                    case 3: // goal
                        objY = 0f;
                        gridObjects[x, y] = Instantiate(goalPrefab, new Vector3(x, objY, y), Quaternion.identity, transform);
                        break;
                    case 4: // player
                        objY = 1.3f;
                        gridObjects[x, y] = Instantiate(playerPrefab, new Vector3(x, objY, y), Quaternion.identity, transform);
                        break;
                    default:
                        // Ningún objeto adicional en esta celda
                        break;
                }
            }
        }
    }

    private void ClearGrid()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }
}
