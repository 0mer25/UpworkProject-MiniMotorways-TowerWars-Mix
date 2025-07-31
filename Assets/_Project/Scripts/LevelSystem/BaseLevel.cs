using System.Collections.Generic;
using UnityEngine;

public class BaseLevel : MonoBehaviour
{
    public int totalRoadTiles;
    public int totalBuildings;
    public List<BaseBuilding> willSpawnBuildings;

    void OnEnable()
    {
        EventManager.TriggerEvent(new EventManager.OnRoadCountChanged(totalRoadTiles));
    }

    public void SpawnBuildings(Vector3 spawnPosition)
    {
        for (int i = 0; i < willSpawnBuildings.Count; i++)
        {
            GameObject building = willSpawnBuildings[i].gameObject;
            if (!building.activeSelf)
            {
                building.transform.position = spawnPosition;
                building.SetActive(true);
            }
        }
    }
}
