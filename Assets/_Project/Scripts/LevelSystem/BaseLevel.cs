using System.Collections.Generic;
using UnityEngine;

public class BaseLevel : MonoBehaviour
{
    public int levelIndex;
    public int totalRoadTiles;
    public int TotalBuildings => allBuildings.Count;
    public List<BaseBuilding> allBuildings;

    [SerializeField] private List<UIProgressTeamHolder> teamHolders;
    [SerializeField] private Transform teamHolderParent;

    private bool isPlayerStartedToDrawRoad = false;

    void OnEnable()
    {
        EventManager.TriggerEvent(new EventManager.OnRoadCountChanged(totalRoadTiles));

        EventManager.RegisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);

        // Initialize team holders
        teamHolders = new List<UIProgressTeamHolder>();

        int teamHolderCount = 0;

        foreach (Transform child in teamHolderParent)
        {
            UIProgressTeamHolder holder = child.GetComponent<UIProgressTeamHolder>();
            if (holder != null)
            {
                holder.team = allBuildings[teamHolderCount].team;
                holder.SetImage(GetTeamColor(holder.team));
                teamHolders.Add(holder);
                teamHolderCount++;
            }
        }
    }

    void Start()
    {
        InvokeRepeating(nameof(SpawnRandomBuildings), 5f, 5);
        InvokeRepeating(nameof(DrawRandomRoadPath), 60f, 15f);

        EventManager.RegisterEvent<EventManager.OnPlayerStartedToDrawRoad>(OnPlayerStartedToDrawRoad);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(SpawnRandomBuildings));
        CancelInvoke(nameof(DrawRandomRoadPath));
        EventManager.DeregisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);
        EventManager.DeregisterEvent<EventManager.OnPlayerStartedToDrawRoad>(OnPlayerStartedToDrawRoad);
    }

    private void OnPlayerStartedToDrawRoad(EventManager.OnPlayerStartedToDrawRoad road)
    {
        isPlayerStartedToDrawRoad = true;

        CancelInvoke(nameof(DrawRandomRoadPath));
        
        InvokeRepeating(nameof(DrawRandomRoadPath), 5f, 15f);
    }

    private void SpawnRandomBuildings()
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
                }
            }
        }
        else
        {
            Debug.LogError("No empty 3x3 area found.");
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
        // Update the UI images
        foreach (var holder in teamHolders)
        {
            if (holder.team == captured.previousTeam)
            {
                holder.team = captured.newTeam;
                holder.SetImage(GetTeamColor(captured.newTeam));
                break;
            }
        }


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

    private Color GetTeamColor(Team team)
    {
        return team switch
        {
            Team.Blue => Color.blue,
            Team.Red => Color.red,
            _ => Color.gray,
        };
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
        Debug.Log($"Blue tower count: {count}");
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
