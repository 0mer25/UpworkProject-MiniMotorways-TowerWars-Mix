using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    private Button restartButton;
    void Awake()
    {
        restartButton = GetComponent<Button>();
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }
    private void OnRestartButtonClicked()
    {
        EventManager.TriggerEvent(new EventManager.OnResetButtonPressed());
    }
}