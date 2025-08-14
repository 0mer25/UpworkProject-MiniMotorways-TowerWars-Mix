using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class SpawnerBuilding : BaseBuilding
{
    [SerializeField] private SpawnerBuildingData spawnerData;
    [SerializeField] private List<GameObject> upgradeFloors;

    public float SpawnTime { get => spawnerData.SpawnInterval / level; }
    private float spawnTimer = 0f;
    public bool hasDrivenRoad = false;

    private List<BaseBuilding> roadDrivenBuildings = new List<BaseBuilding>();

    protected override void Awake()
    {
        base.Awake();
        roadDrivenBuildings ??= new List<BaseBuilding>();
    }

    void Update()
    {
        if (team == Team.Neutral)
        {
            return; // No spawning for neutral team
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= SpawnTime)
        {
            spawnTimer = 0f;
            TryToSpawnCars();
        }
    }

    private void TryToSpawnCars()
    {
        foreach (var connectionTile in _connectionTiles)
        {
            if (connectionTile.State == GridObjType.Road)
            {
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
        var spawnPrefab = spawnerData.GetCarPrefab(teamIndex);
        //var carGO = Instantiate(spawnPrefab, spawnTile.Tile.transform.position, Quaternion.identity);

        var carGO = PoolManager.Instance.Spawn(spawnPrefab, spawnTile.Tile.transform.position, Quaternion.identity);
        
        var car = carGO.GetComponent<BaseCar>();
        car.SpawnCar(this);
        car.Initialize(spawnTile, targetTile, team, 0, transform);
    }
    public RoadTile TryFindTarget(RoadTile startTile)
    {
        RoadTile targetTile;

        var enemyTeam = TeamExtensions.GetEnemyTeam(team);

        targetTile = TrySetPathToReachableTarget(startTile, RoadManager.Instance.BuildingTiles(enemyTeam));
        if (targetTile != null)
        {
            return targetTile;
        }

        targetTile = TrySetPathToReachableTarget(startTile, RoadManager.Instance.BuildingTiles(Team.Neutral));
        if (targetTile != null)
        {
            return targetTile;
        }

        var ourTeamTiles = RoadManager.Instance.BuildingTiles(team)
            .FindAll(t => t.GridObj != this); // ✅ filter out self

        targetTile = TrySetPathToReachableTarget(startTile, ourTeamTiles);
        if (targetTile != null)
        {
            return targetTile;
        }

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
            var building = gridObj as BaseBuilding; // Replace with actual class name

            if (gridObj is SpawnerBuilding spawner)
            {
                if (spawner.team == team && Health < spawner.Health)
                    continue;
            }

            if (building == null || building.ConnectionTiles == null)
                continue;

            List<List<RoadTile>> paths = new List<List<RoadTile>>();

            foreach (var connection in building.ConnectionTiles)
            {
                var path = RoadManager.Instance.ShortestRoadPath(startTile, connection);
                if (path != null && path.Count > 0)
                {
                    paths.Add(path);
                    /* return connection; // or return target if you want to track the building */
                }
            }

            // Return the shortest path if found
            if (paths.Count > 0)
            {
                paths.Sort((a, b) => a.Count.CompareTo(b.Count));
                return paths[0][^1]; // Return the last tile of the shortest path
            }
        }

        return null;
    }

    protected override void UpdateGfx(int newLevel)
    {
        base.UpdateGfx(newLevel);
        
        CloseAllConnectionPointVisuals();
        connectedPointVisuals.Clear();

        if (newLevel == 1)
        {
            if (!upgradeFloors[0].activeSelf)
            {
                upgradeFloors[0].SetActive(true);
                upgradeFloors[0].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);

                for (int i = 0; i < connectionPointsParents[0].transform.childCount; i++)
                {
                    var child = connectionPointsParents[0].transform.GetChild(i);
                    connectedPointVisuals.Add(child.gameObject);
                }

                for (int i = 0; i < currentConnectionCount; i++)
                {
                    FindFirstConnectionPointVisual(true).SetActive(true);
                }

                if (upgradeFloors[1].activeSelf)
                {
                    upgradeFloors[1].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                        .OnComplete(() => upgradeFloors[1].SetActive(false));
                }

                return;
            }
        }
        else if (newLevel == 2)
        {
            if (!upgradeFloors[1].activeSelf)
            {
                upgradeFloors[1].SetActive(true);
                upgradeFloors[1].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);

                for (int i = 0; i < connectionPointsParents[1].transform.childCount; i++)
                {
                    var child = connectionPointsParents[1].transform.GetChild(i);
                    connectedPointVisuals.Add(child.gameObject);
                }

                for (int i = 0; i < currentConnectionCount; i++)
                {
                    FindFirstConnectionPointVisual(true).SetActive(true);
                }
            }

            if (upgradeFloors[0].activeSelf)
            {
                upgradeFloors[0].SetActive(false);
                upgradeFloors[0].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            }
            else if (!upgradeFloors[2].activeSelf)
            {
                upgradeFloors[2].SetActive(false);
                upgradeFloors[2].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            }
        }
        else if (newLevel == 3)
        {
            if (!upgradeFloors[2].activeSelf)
            {
                upgradeFloors[2].SetActive(true);
                upgradeFloors[2].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);

                for (int i = 0; i < connectionPointsParents[2].transform.childCount; i++)
                {
                    var child = connectionPointsParents[2].transform.GetChild(i);
                    connectedPointVisuals.Add(child.gameObject);
                }

                for (int i = 0; i < currentConnectionCount; i++)
                {
                    FindFirstConnectionPointVisual(true).SetActive(true);
                }

                if (upgradeFloors[1].activeSelf)
                {
                    upgradeFloors[1].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                        .OnComplete(() => upgradeFloors[1].SetActive(false));
                }
            }
        }
    }


    private BaseBuilding FindABuildingCenterTile()
    {
        List<BaseBuilding> buildingCenters = FindObjectsByType<BaseBuilding>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();

        // Shuffle the list to randomize the selection
        System.Random rng = new System.Random();
        int n = buildingCenters.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = buildingCenters[k];
            buildingCenters[k] = buildingCenters[n];
            buildingCenters[n] = value;
        }

        for (int i = 0; i < buildingCenters.Count; i++)
        {
            if (roadDrivenBuildings.Contains(buildingCenters[i]) || !buildingCenters[i].CanConnect || buildingCenters[i] == this)
            {
                continue; // Skip if already in the list or cannot connect
            }

            roadDrivenBuildings.Add(buildingCenters[i]);
            return buildingCenters[i]; // Return the first building's center tile
        }

        return null; // Return null if no suitable tile is found
    }

    public void DrawRoadPath(GridTile target = null)
    {
        GridTile centerTile = null;
        foreach (var connectionTile in _connectionTiles)
        {
            if (connectionTile.State != GridObjType.Road)
            {
                centerTile = connectionTile.Tile;
                break;
            }
        }
        if (centerTile == null)
        {
            return;
        }


        if (target != null)
        {
            var pathhh = RoadManager.Instance.ShortestPath(centerTile, target, GridObjType.None);
            Debug.Log(pathhh == null);
            if (pathhh != null && pathhh.Count > 0)
            {
                foreach (var tile in pathhh)
                {
                    tile.Tile.PlaceRoad(false);
                }
            }

            hasDrivenRoad = true;
            return;
        }


        BaseBuilding targetBuilding = FindABuildingCenterTile();

        if (targetBuilding == null)
        {
            return;
        }

        GridTile targetTile = null;
        foreach (var connectionTile in targetBuilding.ConnectionTiles)
        {
            if (connectionTile.State != GridObjType.Road)
            {
                targetTile = connectionTile.Tile;
                break;
            }
        }
        if (targetTile == null)
        {
            return;
        }


        var path = RoadManager.Instance.ShortestPath(centerTile, targetTile, GridObjType.None);
        if (path != null && path.Count > 0)
        {
            foreach (var tile in path)
            {
                tile.Tile.PlaceRoad(false);
            }
        }
        
        hasDrivenRoad = true;
    }

}
