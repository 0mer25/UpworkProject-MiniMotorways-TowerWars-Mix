using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseCar : MonoBehaviour
{
    [SerializeField] protected CarStatSO carStats;
    [SerializeField] private ParticleSystem blowUpParticlePrefab;
    [SerializeField] private ParticleSystem flameParticle;
    [SerializeField] private MeshRenderer carMeshRenderer;
    [SerializeField] private List<Transform> crashPoints;

    public float explosionDuration = 1.5f;
    public float randomPosRange = 3f;

    protected NavMeshAgent agent;
    public Team team;
    protected SpawnerBuilding spawner;
    public SpawnerBuilding Spawner => spawner;
    private List<RoadTile> _roadMap;
    private int _currentTile = -1;
    private RoadTile _currentRoadTile;
    private bool _isMovementStarted = false;
    private bool _isMovingToNextTile = false;
    protected bool isBlowUp = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseCar>(out var car))
        {
            if (isBlowUp) return;
            if (car.team != TeamExtensions.GetEnemyTeam(team)) return;

            car.BlowUp();
            BlowUp();
        }

        if(other.TryGetComponent<MultiplierGate>(out var gate))
        {
            for(int i = 0; i < gate.GetMultiplier(); i++)
            {
                StartCoroutine(DuplicateForGate());
            }
        }
    }

    private IEnumerator DuplicateForGate()
    {
        yield return new WaitForSeconds(0.2f);
        Instantiate(gameObject, transform.position, transform.rotation);
    }

    public virtual void SpawnCar(SpawnerBuilding spawnerBuilding)
    {
        agent = GetComponent<NavMeshAgent>();
        spawner = spawnerBuilding;
    }

    public virtual void Initialize(RoadTile startTile, RoadTile target, Team team, int startTileIndex = 0)
    {
        this.team = team;
        _currentRoadTile = startTile;
        SetTarget(target, startTileIndex);
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

        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        if(isBlowUp) return;

        if (_isMovementStarted && !_isMovingToNextTile)
        {
            MoveToNextTile();
        }

        if (_isMovingToNextTile && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
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
}
