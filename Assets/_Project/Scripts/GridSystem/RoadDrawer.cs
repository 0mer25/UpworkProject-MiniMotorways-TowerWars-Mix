using System;
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
    private bool waitForNextTile = false;

    private bool isDrawingRoad = true;

    private bool isPlayerStartedToDrawRoad = false;

    private List<GridTile> willDeleteTiles = new List<GridTile>();
    [SerializeField] private GameObject deletionOutlineObject;

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnLevelLoading>(OnLevelLoading);

        EventManager.RegisterEvent<EventManager.OnAnyRoadButtonPressed>(OnAnyRoadButtonPressed);
    }

    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnLevelLoading>(OnLevelLoading);
        EventManager.DeregisterEvent<EventManager.OnAnyRoadButtonPressed>(OnAnyRoadButtonPressed);
    }

    private void OnAnyRoadButtonPressed(EventManager.OnAnyRoadButtonPressed pressed)
    {
        if (pressed.willDeleteButtonPressed)
        {
            isDrawingRoad = false;
            deletionOutlineObject.SetActive(true);
        }
        else
        {
            isDrawingRoad = true;
            deletionOutlineObject.SetActive(false);
        }
    }

    private void OnLevelLoading(EventManager.OnLevelLoading loaded)
    {
        willDeleteTiles.Clear(); // Clear any tiles marked for deletion
        isDrawing = false;
        isDeleting = false;
        waitForNextTile = false;
        isPlayerStartedToDrawRoad = false;
    }

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

                if (!tile.HasRoad && !tile.IsConnectionTile)
                {
                    /* isDeleting = true; */
                    return;
                }

                /* isDrawing = true; */

                if (isDrawingRoad)
                {
                    isDrawing = true;
                    isDeleting = false;
                }
                else
                {
                    isDrawing = false;
                    isDeleting = true;
                }



                if (!isPlayerStartedToDrawRoad)
                    {
                        isPlayerStartedToDrawRoad = true;
                        EventManager.TriggerEvent(new EventManager.OnPlayerStartedToDrawRoad());
                    }
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

        if (!Input.GetMouseButton(0)) return;


        if (isDrawing && RoadCountManager.Instance.CanPlaceRoad() && isDrawingRoad)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayerMask))
            {
                GridTile currentTile = hit.collider.GetComponent<GridTile>();

                // Check for 4 nearby tiles if there is road there
                if (!currentTile.IsThereRoadNearby() && !currentTile.IsConnectionTile)
                {
                    isDrawing = false;
                    return;
                }

                if (currentTile && currentTile.IsObstacle)
                    {
                        isDrawing = false;
                        return;
                    }

                if (currentTile.MapRoad == null)
                {
                    return;
                }

                if (currentTile.MapRoad.IsBusy || currentTile.MapRoad.OutOfConnection)
                {
                    waitForNextTile = true;
                    return;
                }
                else
                {
                    waitForNextTile = false;
                }

                // Place the road prefab at the tile's position
                if (currentTile && !waitForNextTile) currentTile.PlaceRoad();
            }
        }

        if (isDeleting && !isDrawingRoad)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayerMask))
            {
                GridTile tileToDelete = hit.collider.GetComponent<GridTile>();

                if (tileToDelete != null && tileToDelete.HasRoad && !willDeleteTiles.Contains(tileToDelete) && tileToDelete.canBeDeleted)
                {
                    willDeleteTiles.Add(tileToDelete);
                    tileToDelete.SetForDeletion();
                }
            }
        }
    }
}
