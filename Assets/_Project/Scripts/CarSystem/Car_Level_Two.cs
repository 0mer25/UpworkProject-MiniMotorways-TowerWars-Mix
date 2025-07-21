public class Car_Level_Two : BaseCar
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
