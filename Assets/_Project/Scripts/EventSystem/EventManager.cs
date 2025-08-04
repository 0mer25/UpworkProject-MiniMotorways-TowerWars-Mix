using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    public interface IEventType
    {
    }


    public struct OnRoadCountChanged : IEventType
    {
        public int newRoadCount;

        public OnRoadCountChanged(int roadCount)
        {
            newRoadCount = roadCount;
        }
    }


    public struct OnRoadPlaced : IEventType
    {
        public GridTile tile;

        public OnRoadPlaced(GridTile tile)
        {
            this.tile = tile;
        }
    }


    public struct OnLevelLoaded : IEventType
    {
        public BaseLevel level;

        public OnLevelLoaded(BaseLevel level)
        {
            this.level = level;
        }
    }

    public struct OnLevelLoading : IEventType
    {
        public BaseLevel level;

        public OnLevelLoading(BaseLevel level)
        {
            this.level = level;
        }
    }


    public struct OnAnyBuildingCaptured : IEventType
    {
        public Team previousTeam;
        public Team newTeam;
        public OnAnyBuildingCaptured(Team previousTeam, Team newTeam)
        {
            this.previousTeam = previousTeam;
            this.newTeam = newTeam;
        }
    }

    public struct OnLevelCompleted : IEventType
    {

    }

    public struct OnLevelFailed : IEventType
    {
        
    }

    public struct OnPlayerStartedToDrawRoad : IEventType
    {
        
    }

    public struct OnResetButtonPressed : IEventType
    {
        
    }






    private static Dictionary<Type, Delegate> eventList = new();

    public static void RegisterEvent<T>(Action<T> eventToAdd) where T : IEventType
    {
        Type eventType = typeof(T);
        if (eventList.TryGetValue(eventType, out var existingDelegate))
        {
            eventList[eventType] = Delegate.Combine(existingDelegate, eventToAdd);
        }
        else
        {
            eventList[eventType] = eventToAdd;
        }
    }

    public static void DeregisterEvent<T>(Action<T> eventToRemove) where T : IEventType
    {
        Type eventType = typeof(T);
        if (eventList.TryGetValue(eventType, out var existingDelegate))
        {
            eventList[eventType] = Delegate.Remove(existingDelegate, eventToRemove);

            if (eventList[eventType] == null)
            {
                eventList.Remove(eventType);
            }
        }
    }

    public static void TriggerEvent<T>(T eventToTrigger) where T : IEventType
    {
        if (eventList.TryGetValue(typeof(T), out var registeredDelegate) && registeredDelegate is Action<T> action)
        {
            action(eventToTrigger);
        }
    }
}
