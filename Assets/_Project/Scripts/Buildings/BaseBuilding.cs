using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public abstract class BaseBuilding : GridObj, IBuilding
{
    [SerializeField] private List<int> levelLimits;
    [SerializeField] private int health = 1;
    [SerializeField] private int startLevel = -1;

    public bool CanConnect { get => this is ShootingTower || currentConnectionCount < level + 1; }
    public int Health => health;
    [SerializeField] private MaterialHolder materialHolder;
    [SerializeField] private List<MeshRenderer> meshRenderers;
    [SerializeField] private TextMeshPro healthText;
    protected int level = -1;
    protected int currentConnectionCount = 0;
    private Material defaultMaterial;

    // ---------------------------------------------------

    protected RoadTile _centerGridRoad;
    protected List<RoadTile> _connectionTiles;
    public List<RoadTile> ConnectionTiles => _connectionTiles;
    public RoadTile CenterTile => _centerGridRoad;
    public Team team;
    public Team BuildingTeam => team;

    [SerializeField] protected List<GameObject> connectedPointVisuals;
    [SerializeField] protected List<GameObject> connectionPointsParents;

    protected virtual void Awake()
    {
        if (startLevel == -1)
        {
            level = 1;
        }
        else
        {
            level = startLevel;
        }

        defaultMaterial = meshRenderers[0].materials[0];
    }

    void Start()
    {
        SetForStart();
    }

    private void SetForStart()
    {
        ChangeTeam(team);
        UpdateHealthText();
        UpdateGfx(level);
        UpdateTileLogic();
        UpdateConnectionPoints();
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

    // -------------------------------------------------------

    public virtual void AnyConnectionConnected()
    {
        FindFirstConnectionPointVisual(true).SetActive(true);
        currentConnectionCount++;
    }

    public virtual void AnyConnectionDisconnected()
    {
        FindFirstConnectionPointVisual(false).SetActive(false);
        currentConnectionCount--;
    }

    public virtual bool CanConnectToRoad()
    {
        return CanConnect;
    }

    protected GameObject FindFirstConnectionPointVisual(bool isConnected)
    {
        if (isConnected)
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
        else
        {
            for (int i = connectedPointVisuals.Count - 1; i >= 0; i--)
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
    protected void CloseAllConnectionPointVisuals()
    {
        for (int i = 0; i < connectedPointVisuals.Count; i++)
        {
            for (int j = 0; j < connectedPointVisuals[i].transform.childCount; j++)
            {
                if (connectedPointVisuals[i].transform.GetChild(j).name.Contains("Connected") &&
                    connectedPointVisuals[i].transform.GetChild(j).gameObject.activeSelf == true)
                {
                    connectedPointVisuals[i].transform.GetChild(j).gameObject.SetActive(false);
                }
            }
        }
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseCar>(out BaseCar car))
        {
            if (car.Spawner == this) return;

            CollisionWithCar(car.team);
            car.DestroyForTowerCollision();
        }
    }

    private void CollisionWithCar(Team carTeam)
    {
        if (team == carTeam)
        {
            health++;

            if (level >= levelLimits.Count + 1)
            {
                // Check if max level reached
                UpdateHealthText();
                return;
            }

            if (health >= levelLimits[level - 1])
            {
                UpgradeBuilding();
            }
        }
        else
        {
            if (health == 0)
            {
                ChangeTeam(carTeam);
                return;
            }

            health--;

            if (level <= 1)
            {
                UpdateHealthText();
                return;
            }

            if (health < levelLimits[level - 2])
            {
                DeUpgradeBuilding();
            }

            UpdateGfx(level);
        }

        UpdateHealthText();
    }

    protected void ChangeTeam(Team newTeam)
    {
        Team previousTeam = team;
        team = newTeam;
        SetMaterialToTeam();
        
        EventManager.TriggerEvent(new EventManager.OnAnyBuildingCaptured(previousTeam, team));
    }

    protected void SetMaterialToTeam()
    {
        Material[] mats = new Material[2];
        mats[0] = defaultMaterial;
        mats[1] = team == Team.Blue ? materialHolder.blueMaterial : team == Team.Red ? materialHolder.redMaterial : materialHolder.neutralMaterial;
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.materials = mats;
        }
    }
    
    private void UpgradeBuilding()
    {
        level++;

        UpdateGfx(level);
    }

    private void DeUpgradeBuilding()
    {
        if (level <= 1) return;

        level--;

        UpdateGfx(level);
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString();
        healthText.transform.GetChild(0).GetComponent<TextMeshPro>().text = health.ToString();

        healthText.transform.DOShakeScale(0.2f, 0.1f, 10, 1);
    }

    protected abstract void UpdateGfx(int newLevel);
}
