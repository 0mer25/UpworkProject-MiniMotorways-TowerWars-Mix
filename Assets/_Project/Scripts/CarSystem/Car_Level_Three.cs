public class Car_Level_Three : BaseCar
{
    public override void BlowUp()
    {
        Destroy(gameObject);
    }
    public override void SpawnCar(CarStatSO carStat)
    {
        carStats = carStat;
    }
}
