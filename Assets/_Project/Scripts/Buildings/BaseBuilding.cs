using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : GridObj
{
    protected RoadTile _centerGridRoad;
    protected List<RoadTile> _connectionTiles;
    public List<RoadTile> ConnectionTiles => _connectionTiles;
    public Team team;
    public Team BuildingTeam => team;

    public virtual void AnyConnectionConnected()
    {

    }

    public virtual void AnyConnectionDisconnected()
    {

    }

    public virtual bool CanConnectToRoad()
    {
        return true;
    }
}
