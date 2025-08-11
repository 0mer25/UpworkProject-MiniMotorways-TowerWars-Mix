using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RoadButton : MonoBehaviour
{
    [SerializeField] private Button roadButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private RectTransform bgRectTransform;

    void OnEnable()
    {
        roadButton.onClick.AddListener(OnRoadButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    private void OnRoadButtonClicked()
    {
        EventManager.TriggerEvent(new EventManager.OnAnyRoadButtonPressed(false));

        bgRectTransform.DOLocalMoveY(110f, 0.25f);
    }

    private void OnDeleteButtonClicked()
    {
        EventManager.TriggerEvent(new EventManager.OnAnyRoadButtonPressed(true));
        bgRectTransform.DOLocalMoveY(-22f, 0.25f);
    }
}
