using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarPrefabsHolder", menuName = "ScriptableObjects/CarPrefabsHolder", order = 1)]
public class CarPrefabsHolder : ScriptableObject
{
    public GameObject blueCarPrefab;
    public GameObject redCarPrefab;

    public GameObject GetCarPrefab(int teamIndex)
    {
        GameObject carPrefab = teamIndex == 0 ? blueCarPrefab : redCarPrefab;

        return carPrefab;
    }
}
