using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject straightRoadPrefab;
    [SerializeField] private GameObject cornerRoadPrefab;

    private List<GridCell> pathCells = new List<GridCell>();
    private GridCell lastCell = null;
    private bool isDrawing = false;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            lastCell = null;
            pathCells.Clear();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            lastCell = null;
        }

        if (isDrawing)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridCell currentCell = hit.collider.GetComponent<GridCell>();

                if (currentCell != null && !pathCells.Contains(currentCell))
                {
                    Vector2Int direction = Vector2Int.up;

                    if (lastCell == null)
                    {
                        // İlk hücreye anında düz yol yerleştir
                        currentCell.PlaceRoad(straightRoadPrefab, direction);
                        pathCells.Add(currentCell);
                        lastCell = currentCell;
                        return;
                    }

                    direction = ClampDirection(currentCell.GridPosition - lastCell.GridPosition);

                    // İkinci hücre geldiyse → ilk hücredeki düz yolun yönünü güncelle
                    if (pathCells.Count == 1)
                    {
                        lastCell.ClearRoad();
                        lastCell.PlaceRoad(straightRoadPrefab, direction);
                    }

                    // Viraj kontrolü (her yeni hücrede bir önceki yönle karşılaştır)
                    if (pathCells.Count >= 2)
                    {
                        GridCell prev = pathCells[pathCells.Count - 1];
                        GridCell prevPrev = pathCells[pathCells.Count - 2];

                        Vector2Int fromPrevPrev = ClampDirection(prev.GridPosition - prevPrev.GridPosition);
                        Vector2Int toCurrent = ClampDirection(currentCell.GridPosition - prev.GridPosition);

                        if (fromPrevPrev != toCurrent)
                        {
                            prev.ClearRoad();
                            Quaternion rot = GetCornerRotation(fromPrevPrev, toCurrent);
                            Instantiate(cornerRoadPrefab, prev.spawnPoint.position, rot, prev.transform);
                        }
                    }

                    currentCell.PlaceRoad(straightRoadPrefab, direction);
                    pathCells.Add(currentCell);
                    lastCell = currentCell;
                }
            }
        }
#endif
    }

    private Quaternion GetCornerRotation(Vector2Int from, Vector2Int to)
    {
        from = ClampDirection(from);
        to = ClampDirection(to);

        if ((from == Vector2Int.down && to == Vector2Int.left) || (from == Vector2Int.right && to == Vector2Int.up))
            return Quaternion.Euler(0, 270, 0);

        if ((from == Vector2Int.left && to == Vector2Int.up) || (from == Vector2Int.down && to == Vector2Int.right))
            return Quaternion.Euler(0, 0, 0);

        if ((from == Vector2Int.up && to == Vector2Int.right) || (from == Vector2Int.left && to == Vector2Int.down))
            return Quaternion.Euler(0, 90, 0);

        if ((from == Vector2Int.up && to == Vector2Int.left) || (from == Vector2Int.right && to == Vector2Int.down))
            return Quaternion.Euler(0, 180, 0);

        return Quaternion.identity;
    }

    private Vector2Int ClampDirection(Vector2Int dir)
    {
        return new Vector2Int(
            dir.x == 0 ? 0 : dir.x / Mathf.Abs(dir.x),
            dir.y == 0 ? 0 : dir.y / Mathf.Abs(dir.y)
        );
    }
}
