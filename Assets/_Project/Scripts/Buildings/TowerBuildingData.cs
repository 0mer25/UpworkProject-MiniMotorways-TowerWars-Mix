using UnityEngine;

[CreateAssetMenu(fileName = "TowerBuildingData", menuName = "ScriptableObjects/TowerBuildingData", order = 1)]
public class TowerBuildingData : BaseBuildingData
{
    public GameObject bulletPrefab;
    public float attackRange = 5.0f;
    public int attackDamage = 1;
}
