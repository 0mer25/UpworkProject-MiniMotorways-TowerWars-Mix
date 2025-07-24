using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnerBuilding : MonoBehaviour, IBuilding
{
    [SerializeField] private List<SpawnerBuildingData> spawnerData;
    [SerializeField] private List<int> levelLimits;
    [SerializeField] private int health = 1;
    [SerializeField] private int startLevel = -1;

    public bool CanConnect { get => currentConnectionCount < MaxConnectionCount; set => CanConnect = value; }
    [SerializeField] private CarPrefabsHolder carPrefabsHolder;
    [SerializeField] private MaterialHolder materialHolder;
    [SerializeField] private List<MeshRenderer> meshRenderers;
    [SerializeField] private TextMeshPro healthText;

    public Team team;


    private int MaxConnectionCount => CurrentData.maxConnectionCount;
    private int level = -1;
    private int currentConnectionCount = 0;
    private SpawnerBuildingData CurrentData => spawnerData[level];


    private Material defaultMaterial;

    void Awake()
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

    void OnEnable()
    {
        SetForStart();
    }

    private void SetForStart()
    {
        ChangeTeam(team);
        healthText.text = health.ToString();

        UpdateGfx(level);

        // Start Spawning cars
    }



    private void SpawnCar(Transform spawnPoint)
    {
        var car = Instantiate(CurrentData.spawnPrefab, spawnPoint.position, Quaternion.identity);
        // car.GetComponent<Car>().Initialize(startGrid, targetGrid, team);
    }

    private void ChangeTeam(Team newTeam)
    {
        team = newTeam;
        SetMaterialToTeam();
    }

    private void SetMaterialToTeam()
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

    private void UpdateGfx(int newLevel)
    {
        if (newLevel == 1)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            return;
        }

        transform.GetChild(newLevel - 2).gameObject.SetActive(false);
        transform.GetChild(newLevel - 1).gameObject.SetActive(true);
    }


    public void CollisionWithCar(Team team)
    {
        if (team == this.team)
        {
            health++;

            if (level >= levelLimits.Count)
            {
                // Check if max level reached
                return;
            }

            if (health >= levelLimits[level])
            {
                UpgradeBuilding();
            }
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


    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseCar>(out BaseCar car))
        {
            CollisionWithCar(car.team);
            car.BlowUp();
        }
    }
    
}
