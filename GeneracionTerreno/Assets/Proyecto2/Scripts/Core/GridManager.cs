using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject wallPrefab, floorPrefab, boxPrefab, goalPrefab, playerPrefab;

    private GameObject[,] gridObjects;

    public void GenerateLevel(int[,] levelData)
    {
        ClearGrid();
        gridObjects = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);
                Instantiate(floorPrefab, pos, Quaternion.identity, transform);

                switch (levelData[x, y])
                {
                    case 1: gridObjects[x, y] = Instantiate(wallPrefab, pos, Quaternion.identity, transform); break;
                    case 2: gridObjects[x, y] = Instantiate(boxPrefab, pos, Quaternion.identity, transform); break;
                    case 3: gridObjects[x, y] = Instantiate(goalPrefab, pos, Quaternion.identity, transform); break;
                    case 4: gridObjects[x, y] = Instantiate(playerPrefab, pos, Quaternion.identity, transform); break;
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
