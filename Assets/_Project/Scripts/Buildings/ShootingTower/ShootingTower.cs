using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

public class ShootingTower : BaseBuilding, IBuilding
{
    [Header("Shooting Tower Settings")]
    [SerializeField] private float fireRate = 1.0f; // Time in seconds between shots
    [SerializeField] private float range = 10.0f; // Range of the shooting tower
    [SerializeField] private GameObject projectilePrefab; // Prefab for the projectile
    [SerializeField] private Transform projectileSpawnPoint; // Point where the projectile spawns
    [SerializeField] private Transform rangeGfx;
    [SerializeField] private int health = 1;
    [SerializeField] private TextMeshPro healthText;
    [SerializeField] private LayerMask carLayerMask; // Layer mask to filter car objects

    [Header("Team Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material neutralMaterial;

    private float nextFireTime = 0.0f; // Time when the tower can fire again


    private int currentConnectionCount = 0; // Current number of connections
    [SerializeField] private int MaxConnectionCount;
    public bool CanConnect { get => currentConnectionCount < MaxConnectionCount; set => CanConnect = value; }

    private void Start()
    {
        SetForStart();
    }

    void OnValidate()
    {
        rangeGfx.localScale = new Vector3(range * 2, range * 2, 1);
    }

    void Update()
    {
        
        // Find the closest target within range
        Collider[] targets = Physics.OverlapSphere(transform.position, range, carLayerMask);

        if (targets.Length == 0) return;

        foreach (Collider target in targets)
        {
            Debug.Log($"Target found: {target.name}");
        }

        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (Collider target in targets)
        {
            if (target.TryGetComponent(out BaseCar car))
            {
                if(car.team == team) continue; // Skip if the target is on the same team

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target.transform;
                }
            }
        }


        if (Time.time >= nextFireTime && closestTarget != null)
        {
            Shoot(closestTarget);
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot(Transform target)
    {
        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        // Set the projectile's direction towards the target
        projectile.transform.LookAt(target);
        projectile.transform.DOMove(target.position, .15f).OnComplete(() =>
        {
            target.GetComponent<BaseCar>()?.BlowUp();
            DestroyBullet(projectile);
        });
    }

    private void DestroyBullet(GameObject bullet)
    {
        Destroy(bullet);
    }

    private void ChangeTeam(Team newTeam)
    {
        team = newTeam;

        meshRenderer.materials[1].color = newTeam switch
        {
            Team.Blue => blueMaterial.color,
            Team.Red => redMaterial.color,
            _ => neutralMaterial.color
        };
    }

    public void CollisionWithCar(Team team)
    {
        if (team == this.team)
        {
            health++;
        }
        else
        {
            if (health == 0)
            {
                ChangeTeam(team);
                return;
            }

            health--;
        }

        healthText.text = health.ToString();
    }


    private void SetForStart()
    {
        ChangeTeam(team);
        healthText.text = health.ToString();
        UpdateTileLogic();
        UpdateConnectionPoints();
    }

    private void UpdateTileLogic()
    {
        var hits = Physics.OverlapSphere(transform.position, 0.2f);
        GridTile centerGridTile = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<GridTile>(out var tile))
            {
                float dist = Vector3.Distance(transform.position, tile.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    centerGridTile = tile;
                }
            }
        }

        if (centerGridTile == null)
        {
            Debug.LogWarning("No GridTile found near the building position.");
            return;
        }

        // Set central RoadTile
        _centerGridRoad = RoadManager.Instance.GetTileByGridPosition(centerGridTile.GridPosition);

        // === Get all covered tiles based on Dimensions ===
        List<RoadTile> occupiedTiles = new List<RoadTile>();
        Vector2Int centerPos = centerGridTile.GridPosition;

        int width = Mathf.RoundToInt(Dimensions.x);
        int height = Mathf.RoundToInt(Dimensions.y);

        int xOffset = width % 2 == 0 ? width / 2 - 1 : width / 2;
        int yOffset = height % 2 == 0 ? height / 2 - 1 : height / 2;

        for (int x = -xOffset; x <= xOffset + (width % 2 == 0 ? 1 : 0); x++)
        {
            for (int y = -yOffset; y <= yOffset + (height % 2 == 0 ? 1 : 0); y++)
            {
                Vector2Int pos = new Vector2Int(centerPos.x + x, centerPos.y + y);
                RoadTile tile = RoadManager.Instance.GetTileByGridPosition(pos);
                if (tile != null)
                {
                    occupiedTiles.Add(tile);
                }
                else
                {
                    Debug.LogWarning($"Missing RoadTile at position {pos}");
                }
            }
        }

        // === Assign tiles to this object ===
        SetTiles(occupiedTiles);
    }
    private void UpdateConnectionPoints()
    {
        _connectionTiles = new List<RoadTile>();

        if (_centerGridRoad == null)
        {
            Debug.LogWarning("Center road tile not set.");
            return;
        }

        Vector2Int center = _centerGridRoad.Tile.GridPosition;

        int width = Mathf.RoundToInt(Dimensions.x);
        int height = Mathf.RoundToInt(Dimensions.y);

        int halfW = width / 2;
        int halfH = height / 2;

        // Four edge centers (relative to center)
        Vector2Int right = center + new Vector2Int(halfW + 1, 0);
        Vector2Int left = center + new Vector2Int(-halfW - 1, 0);
        Vector2Int top = center + new Vector2Int(0, halfH + 1);
        Vector2Int bottom = center + new Vector2Int(0, -halfH - 1);

        TryAddConnectionTile(right);
        TryAddConnectionTile(left);
        TryAddConnectionTile(top);
        TryAddConnectionTile(bottom);

        Vector2Int topRight = center + new Vector2Int(halfW + 1, halfH + 1);
        Vector2Int topLeft = center + new Vector2Int(-halfW - 1, halfH + 1);
        Vector2Int bottomRight = center + new Vector2Int(halfW + 1, -halfH - 1);
        Vector2Int bottomLeft = center + new Vector2Int(-halfW - 1, -halfH - 1);

        TryAddBuildingEdgeTile(topRight);
        TryAddBuildingEdgeTile(topLeft);
        TryAddBuildingEdgeTile(bottomRight);
        TryAddBuildingEdgeTile(bottomLeft);
    }
    private void TryAddConnectionTile(Vector2Int pos)
    {
        var tile = RoadManager.Instance.GetTileByGridPosition(pos);
        if (tile != null)
        {
            tile.BaseBuildingObj = this;
            _connectionTiles.Add(tile);
        }
        else
        {
            Debug.Log($"Connection point not found or not a road at {pos}");
        }
    }

    private void TryAddBuildingEdgeTile(Vector2Int pos)
    {
        var tile = RoadManager.Instance.GetTileByGridPosition(pos);

        if (tile != null)
        {
            SetTile(tile);
        }
        else
        {
            Debug.Log($"Connection point not found or not a road at {pos}");
        }
    }
    public override bool CanConnectToRoad()
    {
        return CanConnect;
    }
    public override void AnyConnectionConnected()
    {
        base.AnyConnectionConnected();
        currentConnectionCount++;
    }
    override public void AnyConnectionDisconnected()
    {
        base.AnyConnectionDisconnected();
        currentConnectionCount--;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        DrawCircle(transform.position, range, 36); // Draw a circle to visualize the range
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        Vector3 oldPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            angle = 2 * Mathf.PI * i / segments;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(oldPoint, newPoint);
            oldPoint = newPoint;
        }
    }
}
