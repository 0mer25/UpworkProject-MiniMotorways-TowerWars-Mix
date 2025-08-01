using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpawnerBuilding : BaseBuilding
{
    [SerializeField] private SpawnerBuildingData spawnerData;
    [SerializeField] private List<GameObject> upgradeFloors;

    public float SpawnTime { get => spawnerData.SpawnInterval / level; }
    private float spawnTimer = 0f;

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
        var spawnPrefab = spawnerData.GetCarPrefab(teamIndex);
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
            Debug.Log($"Found target for {team} team: {targetTile.GridObj.name}");
            return targetTile;
        }

        targetTile = TrySetPathToReachableTarget(startTile, RoadManager.Instance.BuildingTiles(Team.Neutral));
        if (targetTile != null)
        {
            Debug.Log($"Found neutral target for {team} team: {targetTile.GridObj.name}");
            return targetTile;
        }

        var ourTeamTiles = RoadManager.Instance.BuildingTiles(team)
            .FindAll(t => t.GridObj != this); // ✅ filter out self

        targetTile = TrySetPathToReachableTarget(startTile, ourTeamTiles);
        if (targetTile != null)
        {
            Debug.Log($"Found our team target for {team} team: {targetTile.GridObj.name}");
            return targetTile;
        }

        Debug.Log($"No valid target found for {team} team from tile at {startTile.Tile.GridPosition}");
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

            if(gridObj is SpawnerBuilding spawner)
            {
                if (spawner.team == team && Health < spawner.Health)
                    continue;
            }

            if (building == null || building.ConnectionTiles == null)
                continue;

            foreach (var connection in building.ConnectionTiles)
            {
                var path = RoadManager.Instance.ShortestRoadPath(startTile, connection);
                if (path != null && path.Count > 0)
                {
                    return connection; // or return target if you want to track the building
                }
            }
        }

        return null;
    }


    protected override void UpdateGfx(int newLevel)
    {
        if (newLevel == 1)
        {
            if (!upgradeFloors[0].activeSelf)
            {
                upgradeFloors[0].SetActive(true);
                upgradeFloors[0].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);

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
            }

            if (upgradeFloors[0].activeSelf)
            {
                upgradeFloors[0].SetActive(false);
                upgradeFloors[0].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            }
            else if(!upgradeFloors[2].activeSelf)
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

                if (upgradeFloors[1].activeSelf)
                {
                    upgradeFloors[1].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                        .OnComplete(() => upgradeFloors[1].SetActive(false));
                }
            }
        }
    }
}
