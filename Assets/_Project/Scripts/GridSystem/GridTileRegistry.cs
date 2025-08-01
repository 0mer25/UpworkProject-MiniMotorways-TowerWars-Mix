using System;
using System.Collections.Generic;
using UnityEngine;

public class GridTileRegistry : MonoBehaviour
{
    public static GridTileRegistry Instance { get; private set; }

    private Dictionary<Vector2Int, GridTile> tiles = new();

    public List<Denemeeee> denemeeList = new List<Denemeeee>();
    public int x = 0;

    private void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnLevelLoaded>(OnLevelLoaded);
    }
    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnLevelLoaded>(OnLevelLoaded);
    }

    private void OnLevelLoaded(EventManager.OnLevelLoaded loaded)
    {
        x = 0;
        tiles.Clear();
        denemeeList.Clear();
        Debug.Log("Level loaded, clearing tile registry.");
        GridTile[] allTiles = FindObjectsByType<GridTile>(FindObjectsSortMode.None);
        foreach (var tile in allTiles)
        {
            x++;
            tiles[tile.GridPosition] = tile;
            denemeeList.Add(new Denemeeee(tile.GridPosition, tile.RoadObject));
        }
    }

    public bool TryGetTileAt(Vector2Int pos, out GridTile tile)
    {
        Debug.Log(tiles.TryGetValue(pos, out tile)
            ? $"Tile found at {pos}. - {tile.HasRoad}"
            : $"No tile found at {pos}.");

        return tiles.TryGetValue(pos, out tile);
    }

    public IEnumerable<GridTile> AllTiles => tiles.Values;
}

[Serializable]
public class Denemeeee
{
    public Vector2Int Position;
    public GameObject currentRoad;

    public Denemeeee(Vector2Int position, GameObject road)
    {
        Position = position;
        currentRoad = road;
    }
}
