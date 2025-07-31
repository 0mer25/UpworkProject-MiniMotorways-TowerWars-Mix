using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private List<BaseLevel> levels;
    private BaseLevel currentLevel;
    void Awake()
    {
        Instance = this;
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
        currentLevel = levels[PlayerPrefs.GetInt("LevelIndex", 0)];
    }

    public void ResetLevel()
    {
        Debug.Log("Level Reset");
    }

    public void NextLevel()
    {
        PlayerPrefs.SetInt("LevelIndex", PlayerPrefs.GetInt("LevelIndex", 0) + 1);
        Debug.Log("Proceeding to Next Level");
    }

    public void LevelEnded(bool success)
    {
        if (success)
        {
            NextLevel();
        }
        else
        {
            ResetLevel();
        }
    }
}
