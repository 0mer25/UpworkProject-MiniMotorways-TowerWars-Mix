using System.Collections.Generic;
using UnityEngine;

public class BaseLevel : MonoBehaviour
{
    public int levelIndex;
    public int totalRoadTiles;
    public int TotalBuildings => allBuildings.Count;
    public List<BaseBuilding> allBuildings;
    [SerializeField] private UIProgressParent teamHolderParent;

    void OnEnable()
    {
        EventManager.TriggerEvent(new EventManager.OnRoadCountChanged(totalRoadTiles));

        EventManager.RegisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);
        EventManager.RegisterEvent<EventManager.OnPlayerStartedToDrawRoad>(OnPlayerStartedToDrawRoad);
        EventManager.RegisterEvent<EventManager.OnTimerForSpawnCompleted>(SpawnRandomBuildings);

        List<BaseBuilding> activeBuildings = allBuildings.FindAll(b => b.gameObject.activeSelf);
        teamHolderParent.SetHoldersForStart(activeBuildings.ConvertAll(b => b.team));
    }

    void Start()
    {
        //InvokeRepeating(nameof(DrawRandomRoadPath), 60f, 15f);

        EventManager.TriggerEvent(new EventManager.OnAnyBuildingSpawned());
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DrawRandomRoadPath));
        EventManager.DeregisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);
        EventManager.DeregisterEvent<EventManager.OnPlayerStartedToDrawRoad>(OnPlayerStartedToDrawRoad);
        EventManager.DeregisterEvent<EventManager.OnTimerForSpawnCompleted>(SpawnRandomBuildings);
    }

    private void OnPlayerStartedToDrawRoad(EventManager.OnPlayerStartedToDrawRoad road)
    {
        /* CancelInvoke(nameof(DrawRandomRoadPath));
        
        InvokeRepeating(nameof(DrawRandomRoadPath), 5f, 15f); */
    }

    private void SpawnRandomBuildings(EventManager.OnTimerForSpawnCompleted completed)
    {
        var emptyAreas = RoadManager.Instance.FindAll3x3EmptyAreaCenters();
        if (emptyAreas != null && emptyAreas.Count > 0)
        {
            int randomIndex = Random.Range(0, emptyAreas.Count);
            var randomArea = emptyAreas[randomIndex];

            // Choose a random building to spawn

            List<BaseBuilding> buildingsToSpawn = allBuildings.FindAll(b => !b.gameObject.activeSelf);

            if (buildingsToSpawn.Count > 0)
            {
                int buildingIndex = Random.Range(0, buildingsToSpawn.Count);
                BaseBuilding buildingToSpawn = buildingsToSpawn[buildingIndex];
                if (buildingToSpawn != null && !buildingToSpawn.gameObject.activeSelf)
                {
                    buildingToSpawn.transform.position = new Vector3(randomArea.x * 2, 0, randomArea.y * 2);
                    buildingToSpawn.gameObject.SetActive(true);

                    completed.holder.OpenImage(buildingToSpawn.team);

                    EventManager.TriggerEvent(new EventManager.OnAnyBuildingSpawned());
                }
            }
        }
        else
        {
            Debug.LogError("No empty 3x3 area found.");
            EventManager.TriggerEvent(new EventManager.OnTimerForSpawnCompleted(completed.holder));
        }
    }

    private void DrawRandomRoadPath()
    {
        List<BaseBuilding> spawnerBuildings = allBuildings.FindAll(b => b is SpawnerBuilding building && building.gameObject.activeSelf && building.CanConnect &&
            building.team != Team.Blue && building.team != Team.Neutral && building.hasDrivenRoad == false);

        // If there are no spawner buildings, return
        if (spawnerBuildings.Count == 0)
        {
            return;
        }

        // Randomly select a spawner building
        int randomIndex = Random.Range(0, spawnerBuildings.Count);
        SpawnerBuilding selectedSpawner = spawnerBuildings[randomIndex] as SpawnerBuilding;

        // Draw the road path from the selected spawner building
        selectedSpawner.DrawRoadPath();
    }

    private void OnAnyBuildingCaptured(EventManager.OnAnyBuildingCaptured captured)
    {
        if (GetBlueTowerCount() >= TotalBuildings)
        {
            Debug.Log("All blue towers captured");
            EventManager.TriggerEvent(new EventManager.OnLevelCompleted());
        }
        else if (GetBlueTowerCount() <= 0)
        {
            EventManager.TriggerEvent(new EventManager.OnLevelFailed());
        }
    }

    private int GetBlueTowerCount()
    {
        int count = 0;
        foreach (var building in allBuildings)
        {
            if (building.team == Team.Blue)
            {
                count++;
            }
        }
        return count;
    }

    public void SpawnBuildings(Vector3 spawnPosition)
    {
        for (int i = 0; i < allBuildings.Count; i++)
        {
            GameObject building = allBuildings[i].gameObject;
            if (!building.activeSelf)
            {
                building.transform.position = spawnPosition;
                building.SetActive(true);
                return;
            }
        }
    }
}
