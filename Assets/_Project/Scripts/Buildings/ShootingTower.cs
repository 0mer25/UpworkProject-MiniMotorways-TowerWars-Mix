using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ShootingTower : BaseBuilding
{
    [Header("Shooting Tower Settings")]
    [SerializeField] private List<TowerBuildingData> shootingDatas;
    [SerializeField] private float baseFireRate = 6.0f; // Base fire rate of the tower
    private float FireRate => baseFireRate / level;
    private TowerBuildingData ShootingData => level == -1 ? shootingDatas[0] : shootingDatas[level - 1];
    private float Range => ShootingData.attackRange; // Range of the tower
    [SerializeField] private GameObject projectilePrefab; // Prefab for the projectile
    [SerializeField] private Transform projectileSpawnPoint; // Point where the projectile spawns
    [SerializeField] private Transform rangeGfx;
    [SerializeField] private LayerMask carLayerMask; // Layer mask to filter car objects

    [Header("Visuals")]
    [SerializeField] private List<GameObject> upgradeTowers;

    private float nextFireTime = 0.0f; // Time when the tower can fire again

    void OnValidate()
    {
        rangeGfx.localScale = new Vector3(Range * 2, Range * 2, 1);
    }
    void Update()
    {
        // Find the closest target within range
        Collider[] targets = Physics.OverlapSphere(transform.position, Range, carLayerMask);

        if (targets.Length == 0) return;

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
            nextFireTime = Time.time + FireRate;
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
            target.GetComponent<BaseCar>()?.TakeDamage(ShootingData.attackDamage);
            DestroyBullet(projectile);
        });
    }
    private void DestroyBullet(GameObject bullet)
    {
        Destroy(bullet);
    }

    protected override void UpdateGfx(int newLevel)
    {
        rangeGfx.localScale = new Vector3(Range * 2, Range * 2, 1);
        if (newLevel == 1)
        {
            if (!upgradeTowers[0].activeSelf)
            {
                upgradeTowers[0].SetActive(true);
                upgradeTowers[0].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);

                if (upgradeTowers[1].activeSelf)
                {
                    upgradeTowers[1].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                        .OnComplete(() => upgradeTowers[1].SetActive(false));
                }

                return;
            }
        }
        else if (newLevel == 2)
        {
            if (!upgradeTowers[1].activeSelf)
            {
                upgradeTowers[1].SetActive(true);
                upgradeTowers[1].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);
            }

            if (upgradeTowers[0].activeSelf)
            {
                upgradeTowers[0].SetActive(false);
                upgradeTowers[0].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            }
            else if (!upgradeTowers[2].activeSelf)
            {
                upgradeTowers[2].SetActive(false);
                upgradeTowers[2].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            }
        }
        else if (newLevel == 3)
        {
            if (!upgradeTowers[2].activeSelf)
            {
                upgradeTowers[2].SetActive(true);
                upgradeTowers[2].transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);

                if (upgradeTowers[1].activeSelf)
                {
                    upgradeTowers[1].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                        .OnComplete(() => upgradeTowers[1].SetActive(false));
                }
            }
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        DrawCircle(transform.position, Range, 36); // Draw a circle to visualize the range
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
