using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressTeamHolder : MonoBehaviour
{
    public Team team;
    [SerializeField] private Image buildingImage;
    [SerializeField] private GameObject closedObject;
    [SerializeField] private GameObject openedObject;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI timerTextOutline;
    public bool isOpened = false;
    private float remainingTime = 0f;

    void Update()
    {
        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(remainingTime).ToString();
                timerTextOutline.text = timerText.text;
            }
        }
        else if (isOpened && timerText.gameObject.activeSelf)
        {
            closedObject.SetActive(false);
            timerText.gameObject.SetActive(false);
            OpenImage(team);

            EventManager.TriggerEvent(new EventManager.OnTimerForSpawnCompleted(this));
        }
    }

    public void SetImage(Color color)
    {
        if (buildingImage != null)
        {
            buildingImage.color = color;
        }
    }

    public void SetImage(Team team)
    {
        if (buildingImage != null)
        {
            this.team = team;
            buildingImage.color = GetTeamColor(team);
        }
    }

    public void OpenImage(Team team)
    {
        this.team = team;
        SetImage(GetTeamColor(this.team));

        buildingImage.gameObject.SetActive(true);
        openedObject.SetActive(true);

        closedObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        isOpened = true;
    }

    public void StartTimer(float time)
    {
        isOpened = true;
        remainingTime = time;
        timerText.gameObject.SetActive(true);
    }
    

    private Color GetTeamColor(Team team)
    {
        return team switch
        {
            Team.Blue => Color.blue,
            Team.Red => Color.red,
            _ => Color.gray,
        };
    }
}
