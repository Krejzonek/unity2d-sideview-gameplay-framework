using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectionBorder : MonoBehaviour
{
    [SerializeField] private RectTransform border;
    [SerializeField] private Vector2 padding = new Vector2(10f, 10f);
    [SerializeField] private float thickness = 3f;
    [SerializeField] private Color borderColor = Color.yellow;

    private readonly Vector3[] corners = new Vector3[4];
    private GameObject previousSelected;

    private RectTransform top;
    private RectTransform bottom;
    private RectTransform left;
    private RectTransform right;

    private void Awake()
    {
        if (border == null)
            border = GetComponent<RectTransform>();

        border.GetComponent<Image>().enabled = false;

        top = CreatePart("Top");
        bottom = CreatePart("Bottom");
        left = CreatePart("Left");
        right = CreatePart("Right");
    }

    private RectTransform CreatePart(string partName)
    {
        GameObject part = new GameObject(partName, typeof(RectTransform), typeof(Image));
        part.transform.SetParent(border, false);

        Image image = part.GetComponent<Image>();
        image.color = borderColor;
        image.raycastTarget = false;

        return part.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (border == null)
            return;

        GameObject selected = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        if (selected == null)
        {
            SetVisible(false);
            previousSelected = null;
            return;
        }

        RectTransform selectedRect = selected.GetComponent<RectTransform>();

        if (selectedRect == null)
        {
            SetVisible(false);
            return;
        }

        if (selected != previousSelected)
        {
            previousSelected = selected;
            ScrollToSelected(selectedRect);
        }

        selectedRect.GetWorldCorners(corners);

        Vector3 min = border.parent.InverseTransformPoint(corners[0]);
        Vector3 max = min;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 corner = border.parent.InverseTransformPoint(corners[i]);
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        border.localPosition = (min + max) * 0.5f;

        float width = max.x - min.x + padding.x * 2f;
        float height = max.y - min.y + padding.y * 2f;

        border.sizeDelta = new Vector2(width, height);

        top.anchoredPosition = new Vector2(0f, height * 0.5f - thickness * 0.5f);
        top.sizeDelta = new Vector2(width, thickness);

        bottom.anchoredPosition = new Vector2(0f, -height * 0.5f + thickness * 0.5f);
        bottom.sizeDelta = new Vector2(width, thickness);

        left.anchoredPosition = new Vector2(-width * 0.5f + thickness * 0.5f, 0f);
        left.sizeDelta = new Vector2(thickness, height);

        right.anchoredPosition = new Vector2(width * 0.5f - thickness * 0.5f, 0f);
        right.sizeDelta = new Vector2(thickness, height);

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        top.gameObject.SetActive(visible);
        bottom.gameObject.SetActive(visible);
        left.gameObject.SetActive(visible);
        right.gameObject.SetActive(visible);
    }

    private void ScrollToSelected(RectTransform target)
    {
        ScrollRect scrollRect = target.GetComponentInParent<ScrollRect>();

        if (scrollRect == null || scrollRect.content == null || scrollRect.content.parent == null)
            return;

        RectTransform viewport = scrollRect.viewport != null
            ? scrollRect.viewport
            : scrollRect.GetComponent<RectTransform>();

        if (viewport == null)
            return;

        Canvas.ForceUpdateCanvases();
        target.GetWorldCorners(corners);

        Vector3 min = viewport.InverseTransformPoint(corners[0]);
        Vector3 max = min;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 corner = viewport.InverseTransformPoint(corners[i]);
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        Rect viewportRect = viewport.rect;
        Vector2 movement = Vector2.zero;

        if (scrollRect.vertical)
        {
            if (max.y > viewportRect.yMax)
                movement.y = max.y - viewportRect.yMax;
            else if (min.y < viewportRect.yMin)
                movement.y = min.y - viewportRect.yMin;
        }

        if (scrollRect.horizontal)
        {
            if (min.x < viewportRect.xMin)
                movement.x = min.x - viewportRect.xMin;
            else if (max.x > viewportRect.xMax)
                movement.x = max.x - viewportRect.xMax;
        }

        if (movement != Vector2.zero)
        {
            Vector3 localMovement = viewport.TransformVector(new Vector3(movement.x, movement.y, 0f));
            Vector3 contentMovement = scrollRect.content.parent.InverseTransformVector(localMovement);

            Vector2 position = scrollRect.content.anchoredPosition;
            position -= new Vector2(contentMovement.x, contentMovement.y);

            scrollRect.content.anchoredPosition = position;
            Canvas.ForceUpdateCanvases();
        }
    }
}