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
    public bool IsBusy => State == GridObjType.Obstacle || State == GridObjType.Building;
    public GridObj GridObj;
    public BaseBuilding BaseBuildingObj;
    public bool IsConnectionPoint => BaseBuildingObj != null;
    public bool OutOfConnection => IsOutOfConnection();

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

        if (State == GridObjType.Road && BaseBuildingObj != null)
        {
            BaseBuildingObj.AnyConnectionConnected();
        }
    }
    public void ClearTileObj()
    {
        if (State == GridObjType.Road && BaseBuildingObj != null)
        {
            Debug.Log("Clearing tile object and disconnecting from base building.");
            BaseBuildingObj.AnyConnectionDisconnected();
        }

        GridObj = null;
        State = GridObjType.None;
    }


    private bool IsOutOfConnection()
    {
        if (BaseBuildingObj == null) return false;

        if (BaseBuildingObj is SpawnerBuilding)
        {
            var spawner = BaseBuildingObj as SpawnerBuilding;
            if (spawner == null) return false;

            return !spawner.CanConnectToRoad();
        }
        else
        {
            return false;
        }
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