using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    void Start()
    {

    }
    
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
}
