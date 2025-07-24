using System.Collections.Generic;
using UnityEngine;

public class GridObj : MonoBehaviour
{
    public GridObjType ObjType = GridObjType.Building;
    public Vector2 Dimensions = new Vector2(1, 1);
    private List<RoadTile> _currentTiles;

    public void SetTiles(List<RoadTile> roadTiles)
    {
        _currentTiles = roadTiles;

        foreach (var tile in _currentTiles)
        {
            tile.AssignTileObj(this);
        }
    }

    public void SetTile(RoadTile roadTile)
    {
        _currentTiles = new List<RoadTile>() {roadTile};

        foreach (var tile in _currentTiles)
        {
            tile.AssignTileObj(this);
        }
    }

}


[System.Serializable]
public class RoadTile
{
    public GridTile Tile;
    public GridObjType State = GridObjType.None;
    public bool IsBusy => State != GridObjType.None;
    public GridObj GridObj;

    public RoadTile(GridTile gridTile)
    {
        Tile = gridTile;
    }

    public RoadTile(GridTile gridTile, GridObj startObj)
    {
        Tile = gridTile;
        AssignTileObj(startObj);
    }

    public RoadTile(GridTile gridTile, GridObjType roadState)
    {
        Tile = gridTile;
        State = roadState;
    }

    public void AssignTileObj(GridObj obj)
    {
        GridObj = obj;
        State = obj.ObjType;
    }
}

public enum GridObjType
{
    None,
    Building,
    Obstacle,
    Road,
    GhostRoad,
}