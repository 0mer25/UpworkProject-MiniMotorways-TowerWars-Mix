using UnityEngine;
using DG.Tweening;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    private Transform camTransform;
    private Vector3 originalPos;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        camTransform = Camera.main.transform;
        originalPos = camTransform.localPosition;
    }

    public void Shake(float duration = 0.3f, float strength = 0.5f, int vibrato = 10, float randomness = 90f)
    {
        camTransform.DOKill();
        camTransform.localPosition = originalPos;

        camTransform.DOShakePosition(duration, strength, vibrato, randomness, false, true)
            .OnComplete(() => camTransform.localPosition = originalPos);
    }
}
