using System.Collections.Generic;
using UnityEngine;

public class GridTileRegistry : MonoBehaviour
{
    public static GridTileRegistry Instance { get; private set; }

    private Dictionary<Vector2Int, GridTile> tiles = new();

    private void Awake()
    {
        Instance = this;

        GridTile[] allTiles = FindObjectsByType<GridTile>(FindObjectsSortMode.None);
        foreach (var tile in allTiles)
        {
            tiles[tile.GridPosition] = tile;
        }
    }

    public bool TryGetTileAt(Vector2Int pos, out GridTile tile)
    {
        return tiles.TryGetValue(pos, out tile);
    }

    public IEnumerable<GridTile> AllTiles => tiles.Values;
}
