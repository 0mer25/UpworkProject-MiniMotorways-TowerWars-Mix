using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    [SerializeField] private Vector3 startPointForMatrix;

    [ContextMenu("Create Grids")]
    public void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);

                GridTile cell = cellObj.GetComponent<GridTile>();
                cell.GridPosition = new Vector2Int(x, y);
            }
        }
    }

    [ContextMenu("Set Grids")]
    public void SetGrids()
    {
        List<Transform> gridTiles = new List<Transform>();

        int index = 0;

        foreach (Transform child in transform)
        {
            gridTiles.Add(child);
        }

        Debug.Log(gridTiles.Count);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize) + startPointForMatrix;
                gridTiles[index].position = position;
                index++;
            }
        }
    }
}
