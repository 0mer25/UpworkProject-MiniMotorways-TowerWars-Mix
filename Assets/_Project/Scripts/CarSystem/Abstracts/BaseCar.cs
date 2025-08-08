using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseCar : MonoBehaviour, IDamagable
{
    [SerializeField] protected CarStatSO carStats;
    [SerializeField] private ParticleSystem blowUpParticlePrefab;
    [SerializeField] private ParticleSystem flameParticle;
    [SerializeField] private MeshRenderer carMeshRenderer;
    [SerializeField] private List<Transform> crashPoints;
    [SerializeField] private float distanceThreshold = 0.1f;
    [SerializeField] private float randomPosRange = 3f;
    [SerializeField] private ParticleSystem smokeParticle;

    public int Health { get => remainHealth; set => remainHealth = value; }
    public int Damage { get => carStats.health; }
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            BlowUp();
        }
    }

    private int remainHealth;

    protected NavMeshAgent agent;
    public Team team;
    protected SpawnerBuilding spawner;
    public SpawnerBuilding Spawner => spawner;
    private List<RoadTile> _roadMap;
    private int _currentTile = -1;
    private RoadTile _currentRoadTile;
    private bool _isMovementStarted = false;
    private bool _isMovingToNextTile = false;
    public bool isBlowUp = false;
    protected bool canDuplicate = true;
    private bool checkForRoad = false;
    private bool isDestroyed = false;

    void OnEnable()
    {
        remainHealth = carStats.health;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseCar>(out var car))
        {
            if (isBlowUp) return;
            if (car.team == team) return;

            if (GetInstanceID() > car.GetInstanceID()) return;

            car.TakeDamage(Damage);
            TakeDamage(car.Damage);
        }

        if (other.TryGetComponent<MultiplierGate>(out var gate))
        {
            if (canDuplicate)
            {
                for (int i = 0; i < gate.GetMultiplier() - 1; i++)
                {
                    StartCoroutine(DuplicateForGate());
                }
            }
            else
            {
                canDuplicate = true;
            }

        }
    }

    private IEnumerator DuplicateForGate()
    {
        yield return new WaitForSeconds(0.2f);
        GameObject carGO = Instantiate(gameObject, transform.position - transform.forward * 2f, transform.rotation);
        BaseCar car = carGO.GetComponent<BaseCar>();
        car.canDuplicate = false;
        car.SpawnCar(spawner);
        car.Initialize(_currentRoadTile, _roadMap[_roadMap.Count - 1], team, 0, transform.parent);
    }

    public virtual void SpawnCar(SpawnerBuilding spawnerBuilding)
    {
        agent = GetComponent<NavMeshAgent>();
        spawner = spawnerBuilding;
    }

    public virtual void Initialize(RoadTile startTile, RoadTile target, Team team, int startTileIndex = 0, Transform levelParent = null)
    {
        if (levelParent != null)
        {
            transform.SetParent(levelParent);
        }

        this.team = team;
        _currentRoadTile = startTile;
        SetTarget(target, startTileIndex);

        DOVirtual.DelayedCall(1.5f, () =>
        {
            checkForRoad = true;
        });
    }

    public virtual void BlowUp()
    {
        if (isBlowUp) return;
        isBlowUp = true;

        GetComponent<Collider>().isTrigger = false;
        gameObject.layer = LayerMask.NameToLayer("IgnoreExceptGround");
        CameraShaker.Instance.Shake(0.5f, 0.6f, 12, 45f);

        if (blowUpParticlePrefab != null)
        {
            ParticleSpawner.PlayParticleEffect(blowUpParticlePrefab, transform.position);
        }

        if (flameParticle != null)
        {
            flameParticle.Play();
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 forceDir = new Vector3(
            Random.Range(-randomPosRange, randomPosRange),
            Random.Range(2f, randomPosRange),
            Random.Range(-randomPosRange, randomPosRange)
        ).normalized;

        float forcePower = Random.Range(3f, 7f);
        rb.AddForce(forceDir * forcePower, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            Random.Range(1f, 5f),
            Random.Range(1f, 5f),
            Random.Range(1f, 5f)
        );
        rb.AddTorque(torque, ForceMode.Impulse);

        Invoke(nameof(DestroyTheCarToThePool), 3f);
    }

    private void Update()
    {
        if (isBlowUp) return;

        if (agent == null || !_isMovementStarted || !agent.isActiveAndEnabled || agent.gameObject == null)
        {
            return;
        }

        if (!agent.isOnNavMesh)
            return;

        if (checkForRoad)
        {
            CheckForRoad();
        }

        if (_isMovementStarted && !_isMovingToNextTile)
        {
            MoveToNextTile();
        }

        if (agent == null || !_isMovementStarted || !agent.isActiveAndEnabled || agent.gameObject == null)
        {
            return;
        }

        if (_isMovingToNextTile && agent.remainingDistance <= (agent.stoppingDistance + distanceThreshold) && !agent.pathPending)
        {
            ArriveAtTile();
        }
    }

    private void ArriveAtTile()
    {
        _currentRoadTile = _roadMap[_currentTile];
        _currentTile++;
        _isMovingToNextTile = false;
    }

    private void MoveToNextTile()
    {
        if (_roadMap == null || _currentTile >= _roadMap.Count)
        {
            _isMovementStarted = false;
            Debug.Log("Destination reached.");
            return;
        }

        var nextTile = _roadMap[_currentTile];
        _isMovingToNextTile = true;
        agent.SetDestination(nextTile.Tile.transform.position);
    }

    private void StartMovement(int startTileIndex = 0)
    {
        _isMovementStarted = true;
        _currentTile = startTileIndex;
        _isMovingToNextTile = false;
    }

    private void SetTarget(RoadTile targetTile, int startTileIndex = 0)
    {
        _roadMap = RoadManager.Instance.ShortestRoadPath(_currentRoadTile, targetTile);
        if (_roadMap != null && _roadMap.Count > 1)
        {
            Vector3 dir = _roadMap[1].Tile.transform.position - _roadMap[0].Tile.transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = lookRot;
            }
        }
        StartMovement(startTileIndex);
    }
    public void DestroyForTowerCollision()
    {
        Destroy(gameObject);
    }

    private void CheckForRoad()
    {
        if (!Physics.CheckSphere(transform.position, 0.5f, LayerMask.GetMask("Road")))
        {
            DestroyIfNotRoad();
            return;
        }
    }

    private void DestroyIfNotRoad()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        ParticleSpawner.PlayParticleEffect(smokeParticle, transform.position);
        transform.DOScale(Vector3.zero, 0.15f).OnComplete(() =>
        {
            DestroyTheCarToThePool();
        });
    }

    private void DestroyTheCarToThePool()
    {
        PoolManager.Instance.Despawn(gameObject);
    }
}
