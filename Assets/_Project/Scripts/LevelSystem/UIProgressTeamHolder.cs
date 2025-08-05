using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressTeamHolder : MonoBehaviour
{
    public Team team;
    [SerializeField] private Image buildingImage;
    [SerializeField] private GameObject closedObject;
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
            OpenImage();

            // Event For Building Spawn
        }
    }

    public void SetImage(Color color)
    {
        if (buildingImage != null)
        {
            buildingImage.color = color;
        }
    }

    public void OpenImage()
    {
        buildingImage.gameObject.SetActive(true);
    }

    public void StartTimer(float time)
    {
        isOpened = true;
        remainingTime = time;
    }
}
