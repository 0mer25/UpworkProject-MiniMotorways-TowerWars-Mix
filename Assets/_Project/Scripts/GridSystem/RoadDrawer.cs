using System.Collections.Generic;
using UnityEngine;

public class RoadDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject straightRoadPrefab;
    [SerializeField] private LayerMask gridLayerMask;

    private bool isDrawing = false;
    private bool isDeleting = false;

    private List<GridTile> willDeleteTiles = new List<GridTile>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayerMask))
            {
                GridTile tile = hit.collider.GetComponent<GridTile>();

                if (tile == null) return;

                if (!tile.HasRoad)
                {
                    isDeleting = true;
                    return;
                }

                isDrawing = true;
            }
            else
            {
                return;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            isDeleting = false;

            if(willDeleteTiles.Count > 0)
            {
                foreach (var tile in willDeleteTiles)
                {
                    tile.ClearRoad();
                }
                willDeleteTiles.Clear();
            }
        }



        if (isDrawing)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridTile currentTile = hit.collider.GetComponent<GridTile>();

                Vector2Int direction = Vector2Int.up; // Default direction

                if (currentTile.IsObstacle)
                {
                    isDrawing = false;
                    return;
                }

                // Place the road prefab at the tile's position
                currentTile.PlaceRoad(straightRoadPrefab, direction);
            }
        }

        if (isDeleting)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridTile tileToDelete = hit.collider.GetComponent<GridTile>();

                if (tileToDelete != null && tileToDelete.HasRoad)
                {
                    willDeleteTiles.Add(tileToDelete);
                    tileToDelete.SetForDeletion();
                }
            }
        }
    }
}
