using TMPro;
using UnityEngine;

public class RoadCountManager : MonoBehaviour
{
    public static RoadCountManager Instance { get; private set; }
    [SerializeField] private int roadCount = 0;
    [SerializeField] private TextMeshProUGUI roadCountText;
    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        EventManager.RegisterEvent<EventManager.OnRoadCountChanged>(OnRoadCountChanged);
        SetRoadCount(roadCount); // Initialize road count text
    }

    void OnDisable()
    {
        EventManager.DeregisterEvent<EventManager.OnRoadCountChanged>(OnRoadCountChanged);
    }

    public void IncrementRoadCount(int count)
    {
        SetRoadCount(roadCount + count);
    }

    public void DecrementRoadCount(int count)
    {
        SetRoadCount(roadCount - count);
        if (roadCount < 0)
        {
            roadCount = 0; // Ensure road count does not go negative
        }
    }

    public void ResetRoadCount()
    {
        SetRoadCount(0);
    }

    public void SetRoadCount(int count)
    {
        roadCount = count;

        EventManager.TriggerEvent(new EventManager.OnRoadCountChanged(roadCount));
    }

    public int GetRoadCount()
    {
        return roadCount;
    }

    public bool CanPlaceRoad()
    {
        return roadCount > 0;
    }

    private void OnRoadCountChanged(EventManager.OnRoadCountChanged changed)
    {
        roadCountText.text = changed.newRoadCount.ToString();
    }
}
