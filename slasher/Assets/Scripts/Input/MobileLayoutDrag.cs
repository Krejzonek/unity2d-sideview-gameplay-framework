using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLayoutDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private MobileLayoutEditor editor;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        parentRect = rectTransform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (editor == null || !editor.IsOpen)
            return;

        editor.SelectDraggedControl(rectTransform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (editor == null || !editor.IsOpen || rectTransform == null || parentRect == null)
            return;

        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, out Vector2 currentPointer))
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position - eventData.delta, eventCamera, out Vector2 previousPointer))
            return;

        rectTransform.anchoredPosition += currentPointer - previousPointer;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 corner = parentRect.InverseTransformPoint(corners[i]);
            min = Vector2.Min(min, corner);
            max = Vector2.Max(max, corner);
        }

        Rect parentBounds = parentRect.rect;
        Vector2 correction = Vector2.zero;

        if (min.x < parentBounds.xMin)
            correction.x = parentBounds.xMin - min.x;
        else if (max.x > parentBounds.xMax)
            correction.x = parentBounds.xMax - max.x;

        if (min.y < parentBounds.yMin)
            correction.y = parentBounds.yMin - min.y;
        else if (max.y > parentBounds.yMax)
            correction.y = parentBounds.yMax - max.y;

        rectTransform.anchoredPosition += correction;
    }
}