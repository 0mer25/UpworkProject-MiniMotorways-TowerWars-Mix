using UnityEngine;

public class LevelTextUpdater : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnLevelLoaded>(OnLevelLoaded);
    }
    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnLevelLoaded>(OnLevelLoaded);
    }

    private void OnLevelLoaded(EventManager.OnLevelLoaded loaded)
    {
        levelText.text = "Level " + (loaded.level.levelIndex + 1);
    }
}
