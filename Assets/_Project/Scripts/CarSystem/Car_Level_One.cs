public class Car_Level_One : BaseCar
{
    public override void BlowUp()
    {
        Destroy(gameObject);
    }

    public override void SpawnCar(CarStatSO carStat)
    {
        carStats = carStat;
    }


    void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (team == Team.Blue)
        {
            if (other.TryGetComponent(out BaseCar car))
            {
                if (car.team == Team.Red)
                {
                    car.BlowUp();
                    BlowUp();
                }
            }
        }
    }
}