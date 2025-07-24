using UnityEngine;

[CreateAssetMenu(fileName = "SpawnerBuildingData", menuName = "ScriptableObjects/SpawnerBuildingData", order = 1)]
public class SpawnerBuildingData : ScriptableObject
{
    public int level = 0;
    public GameObject spawnPrefab;
    public int maxConnectionCount = 0;
}
