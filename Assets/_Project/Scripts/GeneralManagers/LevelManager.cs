using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    public void InitializeLevel()
    {
        Debug.Log("Level Initialized");
    }

    public void ResetLevel()
    {
        Debug.Log("Level Reset");
    }

    public void LevelEnded(bool success)
    {
        if (success)
        {
            Debug.Log("Level Completed Successfully");
        }
        else
        {
            ResetLevel();
        }
    }
}
