using System;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [Header("Grid Info")]
    public Vector2Int GridPosition;
    [SerializeField] private GameObject currentRoad;
    [SerializeField] private GameObject straightRoadPrefab;
    [SerializeField] private GameObject cornerRoadPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    public bool HasRoad => currentRoad != null;
    public bool IsObstacle => obstaclePrefab != null;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnRoadPlaced>(UpdateGfxAfterAnyPlacement);
    }

    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnRoadPlaced>(UpdateGfxAfterAnyPlacement);
    }

    public void PlaceRoad(GameObject roadPrefab, Vector2Int direction)
    {
        if (HasRoad || IsObstacle) return;

        // Place the road prefab at the tile's position
        currentRoad = Instantiate(roadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), Quaternion.identity, transform);

        RoadCountManager.Instance.DecrementRoadCount(1);

        EventManager.TriggerEvent(new EventManager.OnRoadPlaced(this));
    }

    public void ClearRoad()
    {
        if (currentRoad != null)
        {
            Destroy(currentRoad);
            currentRoad = null;

            RoadCountManager.Instance.IncrementRoadCount(1);

            EventManager.TriggerEvent(new EventManager.OnRoadPlaced(this));
        }
    }
    
    public void SetForDeletion()
    {
        meshRenderer.material.color = Color.red; // Change color to indicate deletion
    }

    private void UpdateGfxAfterAnyPlacement(EventManager.OnRoadPlaced placed)
    {
        if (!HasRoad) return;

        var isNearby = IsNearby(placed.tile);

        if (isNearby || placed.tile == this)
        {
            List<Vector2Int> connectedDirections = new List<Vector2Int>();

            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = GridPosition + dir;
                if (GridTileRegistry.Instance.TryGetTileAt(neighborPos, out GridTile neighborTile))
                {
                    if (neighborTile.HasRoad)
                    {
                        connectedDirections.Add(dir);
                    }
                }
            }

            if (connectedDirections.Count == 2)
            {
                Debug.Log("Two connected directions found: " + connectedDirections[0] + ", " + connectedDirections[1]);
                Vector2Int dirA = connectedDirections[0];
                Vector2Int dirB = connectedDirections[1];

                // Eğer düz değilse, viraj demektir (örnek: up+right)
                if (dirA != -dirB)
                {
                    ReplaceWithCornerRoad(GetCornerRotation(dirA, dirB));
                }
                else
                {
                    ReplaceWithStraightRoad(GetStraightRotation(dirA));
                }
            }
            else if (connectedDirections.Count == 1)
            {
                ReplaceWithStraightRoad(GetStraightRotation(connectedDirections[0]));
            }
        }
    }

    private bool IsNearby(GridTile otherTile)
    {
        int dx = Mathf.Abs(GridPosition.x - otherTile.GridPosition.x);
        int dy = Mathf.Abs(GridPosition.y - otherTile.GridPosition.y);

        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    private Quaternion GetCornerRotation(Vector2Int from, Vector2Int to)
    {
        from = ClampDirection(from);
        to = ClampDirection(to);

        if ((from == Vector2Int.up && to == Vector2Int.right) || (from == Vector2Int.right && to == Vector2Int.up))
            return Quaternion.Euler(0, 0, 0);

        if ((from == Vector2Int.up && to == Vector2Int.left) || (from == Vector2Int.left && to == Vector2Int.up))
            return Quaternion.Euler(0, -90, 0);

        if ((from == Vector2Int.down && to == Vector2Int.left) || (from == Vector2Int.left && to == Vector2Int.down))
            return Quaternion.Euler(0, 180, 0);

        if ((from == Vector2Int.down && to == Vector2Int.right) || (from == Vector2Int.right && to == Vector2Int.down))
            return Quaternion.Euler(0, 90, 0);

        return Quaternion.identity;
    }

    private Vector2Int ClampDirection(Vector2Int dir)
    {
        return new Vector2Int(
            dir.x == 0 ? 0 : dir.x / Mathf.Abs(dir.x),
            dir.y == 0 ? 0 : dir.y / Mathf.Abs(dir.y)
        );
    }

    private Quaternion GetStraightRotation(Vector2Int dir)
    {
        // Up-Down → düz
        if (dir == Vector2Int.up || dir == Vector2Int.down)
            return Quaternion.Euler(0, 0, 0); // Zaten dikeyse rotasyon 0

        // Left-Right → yatay
        if (dir == Vector2Int.left || dir == Vector2Int.right)
            return Quaternion.Euler(0, 90, 0); // Yataysa 90 derece döndür

        return Quaternion.identity;
    }

    private void ReplaceWithCornerRoad(Quaternion rotation)
    {
        Destroy(currentRoad);
        currentRoad = Instantiate(cornerRoadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), rotation, transform);
    }
    private void ReplaceWithStraightRoad(Quaternion rotation)
    {
        Destroy(currentRoad);
        currentRoad = Instantiate(straightRoadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), rotation, transform);
    }
}
