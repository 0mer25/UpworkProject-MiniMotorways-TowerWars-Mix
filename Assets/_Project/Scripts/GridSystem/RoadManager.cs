using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance
    {
        get
        {
            if (!_instance) _instance = FindFirstObjectByType<RoadManager>();
            return _instance;
        }
    }
    public static RoadManager _instance;
    [SerializeField] private Vector2Int xBounds;
    [SerializeField] private Vector2Int yBounds;

    private List<RoadTile> _mapTiles;

    void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        _mapTiles = new List<RoadTile>();
    }

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnLevelLoading>(OnLevelLoading);
    }
    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnLevelLoading>(OnLevelLoading);
    }

    private void OnLevelLoading(EventManager.OnLevelLoading loading)
    {
        _mapTiles.Clear();
    }

    public RoadTile AddTileToList(GridTile gridTile)
    {
        if (_mapTiles == null)
        {
            _mapTiles = new List<RoadTile>();
        }

        var road = _mapTiles.Find(x => x.Tile == gridTile);

        if (road != null)
        {
            Debug.Log("this tile already exist");
            return null;
        }
        else
        {
            road = new RoadTile(gridTile);
            _mapTiles.Add(road);
            return road;
        }
    }

    public List<RoadTile> ShortestRoadPath(RoadTile startTile, RoadTile endTile)
    {
        return ShortestPathByType(startTile, endTile, GridObjType.Road);
    }

    public List<RoadTile> ShortestPathByType(RoadTile startTile, RoadTile endTile, GridObjType type)
    {
        if (startTile == null || endTile == null) return null;

        var openSet = new List<RoadTile> { startTile };
        var cameFrom = new Dictionary<RoadTile, RoadTile>();

        var gScore = new Dictionary<RoadTile, int>();
        var fScore = new Dictionary<RoadTile, int>();

        foreach (var tile in RoadTiles())
        {
            gScore[tile] = int.MaxValue;
            fScore[tile] = int.MaxValue;
        }

        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, endTile);

        while (openSet.Count > 0)
        {
            var current = GetLowestFScore(openSet, fScore);

            if (current == endTile)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.State != type)
                    continue;

                int tentativeG = gScore[current] + 1; // Assuming all tiles have equal cost

                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, endTile);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    public RoadTile GetTileByGridPosition(Vector2Int pos)
    {
        return _mapTiles?.Find(t => t.Tile.GridPosition == pos);
    }


    public List<RoadTile> ShortestPath(GridTile start, GridTile end, GridObjType type)
    {
        var startRoad = _mapTiles.Find(t => t.Tile == start);
        var endRoad = _mapTiles.Find(t => t.Tile == end);
        return ShortestPathByType(startRoad, endRoad, type);
    }

    private int Heuristic(RoadTile a, RoadTile b)
    {
        return Mathf.Abs(a.Tile.GridPosition.x - b.Tile.GridPosition.x) +
               Mathf.Abs(a.Tile.GridPosition.y - b.Tile.GridPosition.y);
    }

    private RoadTile GetLowestFScore(List<RoadTile> openSet, Dictionary<RoadTile, int> fScore)
    {
        RoadTile best = openSet[0];
        int bestScore = fScore.ContainsKey(best) ? fScore[best] : int.MaxValue;

        foreach (var tile in openSet)
        {
            int score = fScore.ContainsKey(tile) ? fScore[tile] : int.MaxValue;
            if (score < bestScore)
            {
                best = tile;
                bestScore = score;
            }
        }

        return best;
    }

    private List<RoadTile> ReconstructPath(Dictionary<RoadTile, RoadTile> cameFrom, RoadTile current)
    {
        var totalPath = new List<RoadTile> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    private List<RoadTile> GetNeighbors(RoadTile tile)
    {
        List<RoadTile> neighbors = new List<RoadTile>();
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up, Vector2Int.down,
        Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = tile.Tile.GridPosition + dir;
            var neighbor = _mapTiles.Find(t => t.Tile.GridPosition == neighborPos);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public List<RoadTile> EmptyTiles()
    {
        return GetTilesByType(GridObjType.None);
    }

    public List<RoadTile> BuildingTiles()
    {
        return GetTilesByType(GridObjType.Building);
    }

    public List<RoadTile> BuildingTiles(Team buildingTeam)
    {
        var buildingTiles = BuildingTiles();
        return buildingTiles.FindAll(x => x.GridObj.GetComponent<IBuilding>().BuildingTeam == buildingTeam);
    }

    public List<RoadTile> RoadTiles()
    {
        return GetTilesByType(GridObjType.Road);
    }

    public List<RoadTile> GhostTiles()
    {
        return GetTilesByType(GridObjType.GhostRoad);
    }

    public List<RoadTile> ObstacleTiles()
    {
        return GetTilesByType(GridObjType.Obstacle);
    }

    public List<RoadTile> GetTilesByTypes(List<GridObjType> types)
    {
        return _mapTiles.FindAll(x => types.IndexOf(x.State) != -1);
    }

    public List<RoadTile> GetTilesByType(GridObjType gridObjType)
    {
        return _mapTiles.FindAll(x => x.State == gridObjType);
    }

    // Find Empty 3x3 matrix
    
    public Vector2Int? Find3x3EmptyArea()
    {
        Vector2Int minBounds = new Vector2Int(xBounds.x, yBounds.x);
        Vector2Int maxBounds = new Vector2Int(xBounds.y, yBounds.y);

        // Sınırları kontrol et - 3x3 alan için yeterli yer var mı?
        if (maxBounds.x - minBounds.x < 2 || maxBounds.y - minBounds.y < 2)
        {
            Debug.LogWarning("Verilen sınırlar 3x3 alan için çok küçük!");
            return null;
        }

        // Her pozisyonu kontrol et (3x3 alan için son 2 satır/sütunu kontrol etmeyiz)
        for (int x = minBounds.x; x <= maxBounds.x - 2; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y - 2; y++)
            {
                Vector2Int startPos = new Vector2Int(x, y);

                if (Is3x3AreaEmpty(startPos))
                {
                    // 3x3 alanın ortasındaki tile'ın pozisyonunu döndür
                    Vector2Int centerPos = new Vector2Int(x + 1, y + 1);
                    return centerPos;
                }
            }
        }

        return null; // Boş 3x3 alan bulunamadı
    }

    private bool Is3x3AreaEmpty(Vector2Int startPos)
    {
        // 3x3 alandaki tüm tile'ları kontrol et
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Vector2Int checkPos = startPos + new Vector2Int(x, y);
                RoadTile tile = GetTileByGridPosition(checkPos);

                // Tile yoksa veya boş değilse false döndür
                if (tile == null || tile.State != GridObjType.None)
                {
                    return false;
                }
            }
        }

        return true; // Tüm tile'lar boş
    }

    // Alternatif: Birden fazla 3x3 boş alanın merkezlerini bulmak istersen
    public List<Vector2Int> FindAll3x3EmptyAreaCenters()
    {
        Vector2Int minBounds = new Vector2Int(xBounds.x, yBounds.x);
        Vector2Int maxBounds = new Vector2Int(xBounds.y, yBounds.y);

        List<Vector2Int> centerPositions = new List<Vector2Int>();

        if (maxBounds.x - minBounds.x < 2 || maxBounds.y - minBounds.y < 2)
        {
            Debug.LogWarning("Verilen sınırlar 3x3 alan için çok küçük!");
            return centerPositions;
        }

        for (int x = minBounds.x; x <= maxBounds.x - 2; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y - 2; y++)
            {
                Vector2Int startPos = new Vector2Int(x, y);

                if (Is3x3AreaEmpty(startPos))
                {
                    // 3x3 alanın ortasındaki tile'ın pozisyonunu ekle
                    Vector2Int centerPos = new Vector2Int(x + 1, y + 1);
                    centerPositions.Add(centerPos);
                }
            }
        }

        return centerPositions;
    }
}