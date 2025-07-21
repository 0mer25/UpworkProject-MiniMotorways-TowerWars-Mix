using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject straightRoadPrefab;
    [SerializeField] private GameObject cornerRoadPrefab;
    [SerializeField] private LayerMask roadLayerMask;
    [SerializeField] private LayerMask gridLayerMask;

    private List<GridCell> pathCells = new List<GridCell>();
    private GridCell lastCell = null;
    private bool isDrawing = false;
    private bool isDeleting = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RoadCountManager.Instance.GetRoadCount() <= 0)
            {
                Debug.Log("No roads left to place.");
                return;
            }

            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayerMask))
            {
                GridCell cell = hit.collider.GetComponent<GridCell>();

                if (cell == null) return;

                if (cell.HasRoad)
                {
                    isDrawing = true;
                    lastCell = null;
                    pathCells.Clear();
                }
                else
                {
                    isDeleting = true; // Set to delete mode if the cell already has a road
                }
            }
            else
            {
                Debug.Log("No grid cell hit to start drawing.");
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            lastCell = null;
            isDeleting = false;
        }

        if (isDeleting)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridCell cellToDelete = hit.collider.GetComponent<GridCell>();
                Debug.Log("Hit cell: " + (cellToDelete != null ? cellToDelete.GridPosition.ToString() : "null"));

                if (cellToDelete != null)
                {
                    Debug.Log("Attempting to delete road from cell at " + cellToDelete.GridPosition);
                    if (cellToDelete.HasRoad && pathCells.Contains(cellToDelete))
                    {
                        Debug.Log("Deleting road from cell at " + cellToDelete.GridPosition);
                        cellToDelete.ClearRoad();
                        pathCells.Remove(cellToDelete);
                        lastCell = null;
                    }
                }
            }
            else
            {
                Debug.Log("No grid cell hit for deletion.");
            }
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
                            pathCells.Remove(lastCell);

                            lastCell.PlaceRoad(straightRoadPrefab, direction);
                            pathCells.Add(lastCell);
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
                            pathCells.Remove(prev);
                            Quaternion rot = GetCornerRotation(fromPrevPrev, toCurrent);

                            prev.PlaceRoad(cornerRoadPrefab, rot);
                            pathCells.Add(prev);
                        }
                        }

                        currentCell.PlaceRoad(straightRoadPrefab, direction);
                        pathCells.Add(currentCell);
                        lastCell = currentCell;
                    }
                }
            }
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
