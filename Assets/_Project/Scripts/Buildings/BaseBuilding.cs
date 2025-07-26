using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : GridObj
{
    protected RoadTile _centerGridRoad;
    protected List<RoadTile> _connectionTiles;
    public List<RoadTile> ConnectionTiles => _connectionTiles;
    public Team team;
    public Team BuildingTeam => team;

    [SerializeField] private List<GameObject> connectedPointVisuals;

    public virtual void AnyConnectionConnected()
    {
        Debug.Log("qwvyudbıqwdlqwd");
        FindFirstConnectionPointVisual(true).SetActive(true);
    }

    public virtual void AnyConnectionDisconnected()
    {
        FindFirstConnectionPointVisual(false).SetActive(false);
    }

    public virtual bool CanConnectToRoad()
    {
        return true;
    }

    private GameObject FindFirstConnectionPointVisual(bool isConnected)
    {
        for (int i = 0; i < connectedPointVisuals.Count; i++)
        {
            for (int j = 0; j < connectedPointVisuals[i].transform.childCount; j++)
            {
                if (connectedPointVisuals[i].transform.GetChild(j).name.Contains("Connected") &&
                    connectedPointVisuals[i].transform.GetChild(j).gameObject.activeSelf != isConnected)
                {
                    return connectedPointVisuals[i].transform.GetChild(j).gameObject;
                }
            }
        }

        return null;
    }
}
