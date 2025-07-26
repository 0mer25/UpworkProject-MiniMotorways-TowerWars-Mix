using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnerBuilding : BaseBuilding, IBuilding
{
    [SerializeField] private List<SpawnerBuildingData> spawnerData;
    [SerializeField] private List<int> levelLimits;
    [SerializeField] private int health = 1;
    [SerializeField] private int startLevel = -1;

    public bool CanConnect { get => currentConnectionCount <= MaxConnectionCount; set => CanConnect = value; }
    [SerializeField] private CarPrefabsHolder carPrefabsHolder;
    [SerializeField] private MaterialHolder materialHolder;
    [SerializeField] private List<MeshRenderer> meshRenderers;
    [SerializeField] private TextMeshPro healthText;

    public float spawnTime = 3.5f;
    public float spawnTimer = 0f;


    private int MaxConnectionCount => CurrentData.maxConnectionCount;
    private int level = -1;
    private int currentConnectionCount = 0;
    private SpawnerBuildingData CurrentData => spawnerData[level - 1];
    private Material defaultMaterial;


    void Awake()
    {
        if (startLevel == -1)
        {
            level = 1;
        }
        else
        {
            level = startLevel;
        }

        defaultMaterial = meshRenderers[0].materials[0];
    }

    void OnEnable()
    {
        SetForStart();
    }

    void Update()
    {
        if(team == Team.Neutral)
        {
            return; // No spawning for neutral team
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0f;
            TryToSpawnCars();
        }
    }

    private void SetForStart()
    {
        ChangeTeam(team);
        healthText.text = health.ToString();
        UpdateGfx(level);
        UpdateTileLogic();
        UpdateConnectionPoints();

        // Ready to spawn cars or any further initialization
    }

    private void UpdateConnectionPoints()
    {
        _connectionTiles = new List<RoadTile>();

        if (_centerGridRoad == null)
        {
            Debug.LogWarning("Center road tile not set.");
            return;
        }

        Vector2Int center = _centerGridRoad.Tile.GridPosition;

        int width = Mathf.RoundToInt(Dimensions.x);
        int height = Mathf.RoundToInt(Dimensions.y);

        int halfW = width / 2;
        int halfH = height / 2;

        // Four edge centers (relative to center)
        Vector2Int right = center + new Vector2Int(halfW + 1, 0);
        Vector2Int left = center + new Vector2Int(-halfW - 1, 0);
        Vector2Int top = center + new Vector2Int(0, halfH + 1);
        Vector2Int bottom = center + new Vector2Int(0, -halfH - 1);

        TryAddConnectionTile(right);
        TryAddConnectionTile(left);
        TryAddConnectionTile(top);
        TryAddConnectionTile(bottom);

        Vector2Int topRight = center + new Vector2Int(halfW + 1, halfH + 1);
        Vector2Int topLeft = center + new Vector2Int(-halfW - 1, halfH + 1);
        Vector2Int bottomRight = center + new Vector2Int(halfW + 1, -halfH - 1);
        Vector2Int bottomLeft = center + new Vector2Int(-halfW - 1, -halfH - 1);

        TryAddBuildingEdgeTile(topRight);
        TryAddBuildingEdgeTile(topLeft);
        TryAddBuildingEdgeTile(bottomRight);
        TryAddBuildingEdgeTile(bottomLeft);
    }

    private void TryAddConnectionTile(Vector2Int pos)
    {
        var tile = RoadManager.Instance.GetTileByGridPosition(pos);
        if (tile != null)
        {
            tile.BaseBuildingObj = this;
            _connectionTiles.Add(tile);
        }
        else
        {
            Debug.Log($"Connection point not found or not a road at {pos}");
        }
    }

    private void TryAddBuildingEdgeTile(Vector2Int pos)
    {
        var tile = RoadManager.Instance.GetTileByGridPosition(pos);

        if (tile != null)
        {
            SetTile(tile);
        }
        else
        {
            Debug.Log($"Connection point not found or not a road at {pos}");
        }
    }

    private void UpdateTileLogic()
    {
        var hits = Physics.OverlapSphere(transform.position, 0.2f);
        GridTile centerGridTile = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<GridTile>(out var tile))
            {
                float dist = Vector3.Distance(transform.position, tile.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    centerGridTile = tile;
                }
            }
        }

        if (centerGridTile == null)
        {
            Debug.LogWarning("No GridTile found near the building position.");
            return;
        }

        // Set central RoadTile
        _centerGridRoad = RoadManager.Instance.GetTileByGridPosition(centerGridTile.GridPosition);

        // === Get all covered tiles based on Dimensions ===
        List<RoadTile> occupiedTiles = new List<RoadTile>();
        Vector2Int centerPos = centerGridTile.GridPosition;

        int width = Mathf.RoundToInt(Dimensions.x);
        int height = Mathf.RoundToInt(Dimensions.y);

        int xOffset = width % 2 == 0 ? width / 2 - 1 : width / 2;
        int yOffset = height % 2 == 0 ? height / 2 - 1 : height / 2;

        for (int x = -xOffset; x <= xOffset + (width % 2 == 0 ? 1 : 0); x++)
        {
            for (int y = -yOffset; y <= yOffset + (height % 2 == 0 ? 1 : 0); y++)
            {
                Vector2Int pos = new Vector2Int(centerPos.x + x, centerPos.y + y);
                RoadTile tile = RoadManager.Instance.GetTileByGridPosition(pos);
                if (tile != null)
                {
                    occupiedTiles.Add(tile);
                }
                else
                {
                    Debug.LogWarning($"Missing RoadTile at position {pos}");
                }
            }
        }

        // === Assign tiles to this object ===
        SetTiles(occupiedTiles);
    }

    public override bool CanConnectToRoad()
    {
        return CanConnect;
    }
    public override void AnyConnectionConnected()
    {
        base.AnyConnectionConnected();
        currentConnectionCount++;
    }
    override public void AnyConnectionDisconnected()
    {
        base.AnyConnectionDisconnected();
        currentConnectionCount--;
    }
    private void TryToSpawnCars()
    {
        /* if (currentConnectionCount <= 0)
        {
            Debug.Log("there are no connection to spawn");
            return;
        } */

        foreach (var connectionTile in _connectionTiles)
        {
            if (connectionTile.State == GridObjType.Road)
            {
                for (int x = 7; x <= 15; x++)
                {
                    var tile = RoadManager.Instance.GetTileByGridPosition(new Vector2Int(x, 14));
                }

                var target = TryFindTarget(connectionTile);
                if (target != null)
                {
                    SpawnCar(connectionTile, target);
                }
            }

        }

    }


    private void SpawnCar(RoadTile spawnTile, RoadTile targetTile)
    {
        var teamIndex = team == Team.Blue ? 0 : team == Team.Red ? 1 : 2;
        var spawnPrefab = carPrefabsHolder.GetCarPrefab(teamIndex, level);
        var carGO = Instantiate(spawnPrefab, spawnTile.Tile.transform.position, Quaternion.identity);
        var car = carGO.GetComponent<BaseCar>();
        car.SpawnCar(this);
        car.Initialize(spawnTile, targetTile, team);
    }

    public RoadTile TryFindTarget(RoadTile startTile)
    {
        RoadTile targetTile;

        var enemyTeam = TeamExtensions.GetEnemyTeam(team);

        targetTile = TrySetPathToReachableTarget(startTile, RoadManager.Instance.BuildingTiles(enemyTeam));
        if (targetTile != null)
        {
            Debug.Log("Path found to enemy building.");
            return targetTile;
        }

        targetTile = TrySetPathToReachableTarget(startTile, RoadManager.Instance.BuildingTiles(Team.Neutral));
        if (targetTile != null)
        {
            Debug.Log("Path found to neutral building.");
            return targetTile;
        }

        var ourTeamTiles = RoadManager.Instance.BuildingTiles(team)
            .FindAll(t => t.GridObj != this); // ✅ filter out self

        targetTile = TrySetPathToReachableTarget(startTile, ourTeamTiles);
        if (targetTile != null)
        {
            Debug.Log("Path found to another teammate building.");
            return targetTile;
        }

        Debug.Log("No reachable buildings found for any team.");
        return null;
    }


    private RoadTile TrySetPathToReachableTarget(RoadTile startTile, List<RoadTile> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        foreach (var target in targets)
        {
            var gridObj = target.GridObj;
            if (gridObj == null)
                continue;

            // This cast assumes all targets are buildings with connection points
            var building = gridObj as SpawnerBuilding; // Replace with actual class name

            if (building == null || building.ConnectionTiles == null)
                continue;

            foreach (var connection in building.ConnectionTiles)
            {
                var path = RoadManager.Instance.ShortestRoadPath(startTile, connection);
                if (path != null && path.Count > 0)
                {
                    Debug.Log($"Valid path from {startTile.Tile.GridPosition} to {connection.Tile.GridPosition} (building at {target.Tile.GridPosition})");
                    return connection; // or return target if you want to track the building
                }
                else
                {
                    Debug.Log($"No path to building connection at {connection.Tile.GridPosition}");
                }
            }
        }

        return null;
    }


    private void ChangeTeam(Team newTeam)
    {
        team = newTeam;
        SetMaterialToTeam();
    }

    private void SetMaterialToTeam()
    {
        Material[] mats = new Material[2];
        mats[0] = defaultMaterial;
        mats[1] = team == Team.Blue ? materialHolder.blueMaterial : team == Team.Red ? materialHolder.redMaterial : materialHolder.neutralMaterial;
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.materials = mats;
        }
    }


    private void UpgradeBuilding()
    {
        level++;

        UpdateGfx(level);
    }

    private void UpdateGfx(int newLevel)
    {
        if (newLevel == 1)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            return;
        }

        transform.GetChild(newLevel - 2).gameObject.SetActive(false);
        transform.GetChild(newLevel - 1).gameObject.SetActive(true);
    }


    public void CollisionWithCar(Team team)
    {
        if (team == this.team)
        {
            health++;

            if (level >= levelLimits.Count)
            {
                // Check if max level reached
                return;
            }

            if (health >= levelLimits[level])
            {
                UpgradeBuilding();
            }
        }
        else
        {
            if (health == 0)
            {
                ChangeTeam(team);
                return;
            }

            health--;
        }

        healthText.text = health.ToString();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseCar>(out BaseCar car))
        {
            if (car.Spawner == this) return;

            CollisionWithCar(car.team);
            car.BlowUp();
        }
    }

}
