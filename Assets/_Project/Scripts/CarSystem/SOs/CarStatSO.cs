using UnityEngine;

[CreateAssetMenu(fileName = "CarStat", menuName = "ScriptableObjects/CarStatSO", order = 1)]
public class CarStatSO : ScriptableObject
{
    public GameObject carPrefab;
    public int level;
    public float speed;
    public int health;
}
