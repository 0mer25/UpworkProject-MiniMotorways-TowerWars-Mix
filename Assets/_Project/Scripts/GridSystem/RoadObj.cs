using UnityEngine;

public class RoadObj : GridObj
{
    void OnEnable()
    {
        if (transform.parent.TryGetComponent<GridTile>(out var grid))
        {
            SetTile(grid.MapRoad);
        }
    }   
}
