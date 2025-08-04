using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private List<BaseLevel> levels;
    private BaseLevel currentLevel;
    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnLevelCompleted>(OnLevelCompleted);
        EventManager.RegisterEvent<EventManager.OnLevelFailed>(OnLevelFailed);
        EventManager.RegisterEvent<EventManager.OnResetButtonPressed>(OnResetButtonPressed);
    }
    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnLevelCompleted>(OnLevelCompleted);
        EventManager.DeregisterEvent<EventManager.OnLevelFailed>(OnLevelFailed);
        EventManager.DeregisterEvent<EventManager.OnResetButtonPressed>(OnResetButtonPressed);
    }

    void Start()
    {
        InitializeLevel();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            var emptyAreas = RoadManager.Instance.FindAll3x3EmptyAreaCenters();
            if (emptyAreas != null)
            {
                int randomIndex = Random.Range(0, emptyAreas.Count);
                var randomArea = emptyAreas[randomIndex];

                currentLevel.SpawnBuildings(new Vector3(randomArea.x * 2, 0, randomArea.y * 2));
            }
            else
            {
                Debug.Log("No empty 3x3 area found.");
            }
        }
#endif
    }

    public void InitializeLevel()
    {
        EventManager.TriggerEvent(new EventManager.OnLevelLoading(currentLevel));

        currentLevel = Instantiate(levels[PlayerPrefs.GetInt("LevelIndex", 0)], Vector3.zero, Quaternion.identity);

        EventManager.TriggerEvent(new EventManager.OnLevelLoaded(currentLevel));
    }

    public void ResetLevel()
    {
        currentLevel.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
        {
            Destroy(currentLevel.gameObject);
            currentLevel = null;
        });
        DOVirtual.DelayedCall(0.55f, () =>
        {
            InitializeLevel();
        });
    }

    public void NextLevel()
    {
        PlayerPrefs.SetInt("LevelIndex", PlayerPrefs.GetInt("LevelIndex", 0) + 1);
    }

    public void LevelEnded(bool success)
    {
        if (success)
        {
            //NextLevel();
            ResetLevel();
        }
        else
        {
            ResetLevel();
        }
    }

    private void OnLevelFailed(EventManager.OnLevelFailed failed)
    {
        LevelEnded(false);
    }

    private void OnLevelCompleted(EventManager.OnLevelCompleted completed)
    {
        LevelEnded(true);
    }

    private void OnResetButtonPressed(EventManager.OnResetButtonPressed pressed)
    {
        ResetLevel();
    }
    

}
