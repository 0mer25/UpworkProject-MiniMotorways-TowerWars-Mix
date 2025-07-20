using UnityEngine;

public class GridCell : MonoBehaviour
{
    [Header("Grid Info")]
    public Vector2Int GridPosition;

    [Header("Spawn Point")]
    public Transform spawnPoint; // Prefab'ın çıkacağı nokta (grid merkezinde biraz yukarıda olmalı)

    private GameObject currentRoad;
    private bool hasRoad = false;

    /// <summary>
    /// Hücreye yönüne göre düz veya viraj prefabı yerleştirir.
    /// </summary>
    public void PlaceRoad(GameObject prefab, Vector2Int direction)
    {
        if (hasRoad) return;

        Quaternion rotation = GetRotationFromDirection(direction);
        currentRoad = Instantiate(prefab, spawnPoint.position, rotation, transform);
        hasRoad = true;
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
        }
        hasRoad = false;
    }

    /// <summary>
    /// Hücrede yol var mı?
    /// </summary>
    public bool HasRoad => hasRoad;

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
