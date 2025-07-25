using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [Header("Grid Info")]
    public Vector2Int GridPosition;
    [SerializeField] private GameObject currentRoad;
    [SerializeField] private GameObject straightRoadPrefab;
    [SerializeField] private GameObject cornerRoadPrefab;
    [SerializeField] private GameObject threeWayRoadPrefab;
    [SerializeField] private GameObject fourWayRoadPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    public bool HasRoad => currentRoad != null;
    public bool IsObstacle => obstaclePrefab != null;
    private List<MeshRenderer> meshRenderers;

    void Awake()
    {
        meshRenderers = new List<MeshRenderer>();
    }

    private RoadTile _roadTile;
    public RoadTile MapRoad => _roadTile;

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnRoadPlaced>(UpdateGfxAfterAnyPlacement);
        _roadTile = RoadManager.Instance.AddTileToList(this);
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

        meshRenderers = currentRoad.GetComponentsInChildren<MeshRenderer>().ToList();

        EventManager.TriggerEvent(new EventManager.OnRoadPlaced(this));
    }

    public void ClearRoad()
    {
        if (currentRoad != null)
        {
            _roadTile.ClearTileObj();

            Destroy(currentRoad);
            currentRoad = null;

            RoadCountManager.Instance.IncrementRoadCount(1);

            EventManager.TriggerEvent(new EventManager.OnRoadPlaced(this));
        }
    }

    public void SetForDeletion()
    {
        if(meshRenderers.Count > 0)
        {
            foreach (var renderer in meshRenderers)
            {
                renderer.material.color = Color.red; // Set the color to red to indicate deletion
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.5f); // Make it semi-transparent
            }
        }
    }

    private void UpdateGfxAfterAnyPlacement(EventManager.OnRoadPlaced placed)
    {
        if (!HasRoad) return;

        var isNearby = IsNearby(placed.tile);

        if (isNearby || placed.tile == this)
        {

            meshRenderers.Clear();
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
            else if (connectedDirections.Count == 3)
            {
                Vector2Int[] allDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (var dir in allDirections)
                {
                    if (!connectedDirections.Contains(dir))
                    {
                        ReplaceWithThreeWayRoad(GetThreeWayRotation(dir));
                        break;
                    }
                }
            }
            else if (connectedDirections.Count == 4)
            {
                ReplaceWithFourWayRoad(Quaternion.identity);
            }

            //meshRenderers = GetComponentsInChildren<MeshRenderer>().Where(mr => !mr.gameObject.name.Equals("GFX")).ToList();
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
    private Quaternion GetThreeWayRotation(Vector2Int dir)
    {
        if (dir == Vector2Int.down)   // Açık olmayan yön aşağı → Ters T
            return Quaternion.Euler(0, 180, 0);
        if (dir == Vector2Int.left)   // Açık olmayan yön sol → sağdan açık T
            return Quaternion.Euler(0, -90, 0);
        if (dir == Vector2Int.up)     // Açık olmayan yön yukarı → düz T
            return Quaternion.Euler(0, 0, 0);
        if (dir == Vector2Int.right)  // Açık olmayan yön sağ → soldan açık T
            return Quaternion.Euler(0, 90, 0);

        return Quaternion.identity;
    }

    private void ReplaceWithCornerRoad(Quaternion rotation)
    {
        Destroy(currentRoad);
        currentRoad = Instantiate(cornerRoadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), rotation, transform);

        // Update the mesh renderers after replacing the road
        meshRenderers = currentRoad.GetComponentsInChildren<MeshRenderer>().ToList();
    }
    private void ReplaceWithStraightRoad(Quaternion rotation)
    {
        Destroy(currentRoad);
        currentRoad = Instantiate(straightRoadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), rotation, transform);

        // Update the mesh renderers after replacing the road
        meshRenderers = currentRoad.GetComponentsInChildren<MeshRenderer>().ToList();
    }

    private void ReplaceWithThreeWayRoad(Quaternion rotation)
    {
        Destroy(currentRoad);
        currentRoad = Instantiate(threeWayRoadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), rotation, transform);

        // Update the mesh renderers after replacing the road
        meshRenderers = currentRoad.GetComponentsInChildren<MeshRenderer>().ToList();
    }
    private void ReplaceWithFourWayRoad(Quaternion rotation)
    {
        Destroy(currentRoad);
        currentRoad = Instantiate(fourWayRoadPrefab, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), rotation, transform);

        // Update the mesh renderers after replacing the road
        meshRenderers = currentRoad.GetComponentsInChildren<MeshRenderer>().ToList();
    }
}
