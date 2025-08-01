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
    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);
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
            }
        }


        if (GetBlueTowerCount() >= TotalBuildings)
        {
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
