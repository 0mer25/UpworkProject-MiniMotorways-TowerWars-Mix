using System;
using System.Collections.Generic;
using UnityEngine;

public class UIProgressParent : MonoBehaviour
{
    [SerializeField] private float timerDuration = 20f;
    [SerializeField] private List<UIProgressTeamHolder> teamHolders;

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnAnyBuildingSpawned>(OnAnyBuildingSpawned);
    }

    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnAnyBuildingSpawned>(OnAnyBuildingSpawned);
    }

    private void OnAnyBuildingSpawned(EventManager.OnAnyBuildingSpawned spawned)
    {
        foreach (var holder in teamHolders)
        {
            if (!holder.isOpened)
            {
                holder.StartTimer(timerDuration);
                break;
            }
        }
    }
}
