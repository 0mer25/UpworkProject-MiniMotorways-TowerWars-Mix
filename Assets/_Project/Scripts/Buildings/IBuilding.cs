using UnityEngine;

public interface IBuilding
{
    public bool CanConnect { get; set; }

    public Team BuildingTeam { get; }
}
