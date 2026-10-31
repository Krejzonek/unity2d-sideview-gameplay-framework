using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private GameInputManager inputManager;
    [SerializeField] private JoystickRole role = JoystickRole.Movement;
    [SerializeField, Range(0.1f, 1f)] private float handleRange = 1f;

    private Canvas canvas;
    private Vector2 input;
    private Vector2 pointerStartLocal;
    private bool dragging;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (background == null)
            background = transform as RectTransform;
    }

    private void OnEnable()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;
    }

    private void OnDisable()
    {
        ReleaseInput();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (background == null || handle == null || inputManager == null)
            return;

        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventCamera, out pointerStartLocal))
            return;

        dragging = true;
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;

        if (role == JoystickRole.Movement)
            inputManager.SetMobileMovement(Vector2.zero, true);
        else
            inputManager.SetMobileAim(Vector2.zero, false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || background == null || handle == null || inputManager == null)
            return;

        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventCamera, out Vector2 localPoint))
            return;

        Vector2 halfSize = background.rect.size * 0.5f;
        Vector2 delta = localPoint - pointerStartLocal;

        if (role == JoystickRole.Movement)
            delta.y = 0f;

        float rangeX = halfSize.x * handleRange;
        float rangeY = halfSize.y * handleRange;

        input = new Vector2(
            rangeX > 0f ? delta.x / rangeX : 0f,
            role == JoystickRole.Movement || rangeY <= 0f ? 0f : delta.y / rangeY
        );

        input = Vector2.ClampMagnitude(input, 1f);

        if (input.magnitude < 0.15f)
            input = Vector2.zero;

        handle.anchoredPosition = new Vector2(
            input.x * rangeX,
            input.y * rangeY
        );

        if (role == JoystickRole.Movement)
            inputManager.SetMobileMovement(input, true);
        else
            inputManager.SetMobileAim(input, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseInput();
    }

    private void ReleaseInput()
    {
        dragging = false;
        input = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager == null)
            return;

        if (role == JoystickRole.Movement)
            inputManager.SetMobileMovement(Vector2.zero, false);
        else
            inputManager.SetMobileAim(Vector2.zero, false);
    }
}