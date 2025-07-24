using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarPrefabsHolder", menuName = "ScriptableObjects/CarPrefabsHolder", order = 1)]
public class CarPrefabsHolder : ScriptableObject
{
    public List<GameObject> blueCarPrefabs;
    public List<GameObject> redCarPrefabs;
}
