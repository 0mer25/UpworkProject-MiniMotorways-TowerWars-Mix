using System.Collections;
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
        StartCoroutine(WaitForReload());
        EventManager.TriggerEvent(new EventManager.OnResetButtonPressed());
    }

    private IEnumerator WaitForReload()
    {
        restartButton.interactable = false;
        yield return new WaitForSeconds(1.2f);
        restartButton.interactable = true;
    }
}