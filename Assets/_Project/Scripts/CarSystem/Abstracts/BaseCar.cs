using UnityEngine;

public abstract class BaseCar : MonoBehaviour
{
    [SerializeField] protected CarStatSO carStats;

    public Team team;
    public abstract void SpawnCar(CarStatSO carStat);
    public abstract void BlowUp();
}
