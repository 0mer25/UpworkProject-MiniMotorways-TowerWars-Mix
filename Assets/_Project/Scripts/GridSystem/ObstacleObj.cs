
public class ObstacleObj : GridObj
{
    void OnEnable()
    {
        GetComponentInParent<GridTile>().MapRoad.State = GridObjType.Obstacle;
    }
}
