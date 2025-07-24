using UnityEngine;
using DG.Tweening;

public class ShootingTower : BaseBuilding, IBuilding
{
    [Header("Shooting Tower Settings")]
    [SerializeField] private float fireRate = 1.0f; // Time in seconds between shots
    [SerializeField] private float range = 10.0f; // Range of the shooting tower
    [SerializeField] private GameObject projectilePrefab; // Prefab for the projectile
    [SerializeField] private Transform projectileSpawnPoint; // Point where the projectile spawns

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
        // Initialize the tower's team and appearance
        ChangeTeam(team);
    }

    void Update()
    {
        // Find the closest target within range
        Collider[] targets = Physics.OverlapSphere(transform.position, range);
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
            Destroy(target.gameObject);
            DestroyBullet(projectile);
        });
    }

    private void DestroyBullet(GameObject bullet)
    {
        // Optionally, you can add effects or sounds here before destroying the bullet
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
