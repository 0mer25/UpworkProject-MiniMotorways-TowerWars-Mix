using System;
using System.Collections.Generic;
using UnityEngine;

public class UIProgressParent : MonoBehaviour
{
    [SerializeField] private GameObject progressHolderPrefab;
    [SerializeField] private float timerDuration = 20f;
    [SerializeField] private List<UIProgressTeamHolder> teamHolders;

    void Awake()
    {
        BaseLevel parentObject = GetComponentInParent<BaseLevel>();

        for (int i = 0; i < parentObject.TotalBuildings; i++)
        {
            UIProgressTeamHolder holder = Instantiate(progressHolderPrefab, transform).GetComponent<UIProgressTeamHolder>();
            teamHolders.Add(holder);
        }
    }

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnAnyBuildingSpawned>(OnAnyBuildingSpawned);
        EventManager.RegisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);
    }

    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnAnyBuildingSpawned>(OnAnyBuildingSpawned);
        EventManager.DeregisterEvent<EventManager.OnAnyBuildingCaptured>(OnAnyBuildingCaptured);
    }

    private void OnAnyBuildingCaptured(EventManager.OnAnyBuildingCaptured captured)
    {
        foreach (var holder in teamHolders)
        {
            if (holder.isOpened && holder.team == captured.previousTeam)
            {
                holder.SetImage(captured.newTeam);
                break;
            }
        }
    }

    private void OnAnyBuildingSpawned(EventManager.OnAnyBuildingSpawned spawned)
    {
        for (int i = 0; i < teamHolders.Count; i++)
        {
            if (!teamHolders[i].isOpened)
            {
                teamHolders[i].StartTimer(timerDuration);
                break;
            }
        }
    }

    public void SetHoldersForStart(List<Team> teams)
    {
        for (int i = 0; i < teams.Count; i++)
        {
            teamHolders[i].OpenImage(teams[i]);
        }
    }
    
}
