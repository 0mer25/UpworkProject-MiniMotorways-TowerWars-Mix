using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite defaultCursorSprite;
    [SerializeField] private Sprite clickCursorSprite;

    private Canvas _canvas;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = (RectTransform)transform;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            cursorImage.sprite = clickCursorSprite;
        
        if (Input.GetMouseButtonUp(0))
            cursorImage.sprite = defaultCursorSprite;

        PositionSelf();
    }

    private void PositionSelf()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform, 
            Input.mousePosition, 
            _canvas.worldCamera, 
            out localPoint);
    
        _rectTransform.anchoredPosition = localPoint;
    }
}
