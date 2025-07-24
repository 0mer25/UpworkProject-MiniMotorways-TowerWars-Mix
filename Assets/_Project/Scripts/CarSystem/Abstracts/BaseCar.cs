using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseCar : MonoBehaviour
{
    [SerializeField] protected CarStatSO carStats;

    protected NavMeshAgent agent;
    public Team team;
    protected SpawnerBuilding spawner;
    public SpawnerBuilding Spawner => spawner;
    private List<RoadTile> _roadMap;
    private int _currentTile = -1;
    private RoadTile _currentRoadTile;
    private bool _isMovementStarted = false;
    private bool _isMovingToNextTile = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseCar>(out var car))
        {
            if (isBlowUp) return;
            if (car.team != TeamExtensions.GetEnemyTeam(team)) return;

            car.BlowUp();
            BlowUp();
        }
    }

    public virtual void SpawnCar(SpawnerBuilding spawnerBuilding)
    {
        agent = GetComponent<NavMeshAgent>();
        spawner = spawnerBuilding;
    }

    public virtual void Initialize(RoadTile startTile, RoadTile target, Team team)
    {
        this.team = team;
        _currentRoadTile = startTile;
        SetTarget(target);
    }

    protected bool isBlowUp = false;

    public virtual void BlowUp()
    {
        if (isBlowUp) return;
        isBlowUp = true;
        Destroy(gameObject);
    }

    private void Update()
    {
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


    private void StartMovement()
    {
        _isMovementStarted = true;
        _currentTile = 0;
        _isMovingToNextTile = false;
    }


    private void SetTarget(RoadTile targetTile)
    {
        _roadMap = RoadManager.Instance.ShortestRoadPath(_currentRoadTile, targetTile);
        if (_roadMap != null && _roadMap.Count > 1)
        {
            // Get direction from start to next tile
            Vector3 dir = _roadMap[1].Tile.transform.position - _roadMap[0].Tile.transform.position;
            dir.y = 0f; // Ensure we only rotate on Y

            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = lookRot;
            }
        }
        StartMovement();
    }
}
