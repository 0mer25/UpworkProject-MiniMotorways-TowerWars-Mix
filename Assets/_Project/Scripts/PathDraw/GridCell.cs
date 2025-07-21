using UnityEngine;

public class GridCell : MonoBehaviour
{
    [Header("Grid Info")]
    public Vector2Int GridPosition;

    [Header("Spawn Point")]
    public Transform spawnPoint; // Prefab'ın çıkacağı nokta (grid merkezinde biraz yukarıda olmalı)

    public GameObject currentRoad;

    /// <summary>
    /// Hücreye yönüne göre düz veya viraj prefabı yerleştirir.
    /// </summary>
    public void PlaceRoad(GameObject prefab, Vector2Int direction)
    {
        if (HasRoad) return;

        Quaternion rotation = GetRotationFromDirection(direction);
        currentRoad = Instantiate(prefab, spawnPoint.position, rotation, transform);

        RoadCountManager.Instance.DecrementRoadCount(1);
    }

    public void PlaceRoad(GameObject prefab, Quaternion rotation)
    {
        if (HasRoad) return;

        currentRoad = Instantiate(prefab, spawnPoint.position, rotation, transform);

        RoadCountManager.Instance.DecrementRoadCount(1);
    }

    /// <summary>
    /// Hücredeki mevcut yolu kaldırır.
    /// </summary>
    public void ClearRoad()
    {
        if (currentRoad != null)
        {
            Destroy(currentRoad);
            currentRoad = null;

            RoadCountManager.Instance.IncrementRoadCount(1);
        }
    }

    /// <summary>
    /// Hücrede yol var mı?
    /// </summary>
    public bool HasRoad => currentRoad != null;

    /// <summary>
    /// Verilen yöne göre prefab'a uygun rotasyon döner.
    /// </summary>
    private Quaternion GetRotationFromDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return Quaternion.Euler(0, 0, 0);
        if (dir == Vector2Int.right) return Quaternion.Euler(0, 90, 0);
        if (dir == Vector2Int.down) return Quaternion.Euler(0, 180, 0);
        if (dir == Vector2Int.left) return Quaternion.Euler(0, 270, 0);
        return Quaternion.identity;
    }
}
