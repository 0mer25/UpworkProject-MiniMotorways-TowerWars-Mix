using UnityEngine;

[CreateAssetMenu(fileName = "SpawnerBuildingData", menuName = "ScriptableObjects/SpawnerBuildingData", order = 1)]
public class SpawnerBuildingData : BaseBuildingData
{
    [SerializeField] private CarPrefabsHolder carPrefabsHolder;
    [SerializeField] private float spawnInterval = 1.0f;
    public float SpawnInterval => spawnInterval;

    public GameObject GetCarPrefab(int teamIndex)
    {
        return carPrefabsHolder.GetCarPrefab(teamIndex);
    }
}
