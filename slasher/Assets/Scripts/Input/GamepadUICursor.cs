using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class GamepadUICursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursorVisual;
    [SerializeField] private float cursorSpeed = 900f;

    private GameInputManager inputManager;
    private Canvas canvas;
    private Mouse virtualMouse;
    private Vector2 cursorPosition;

    private void Awake()
    {
        inputManager = GameInputManager.Instance;
        canvas = GetComponentInParent<Canvas>();

        cursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        virtualMouse = InputSystem.AddDevice<Mouse>("Gamepad Virtual Mouse");
        UpdateVirtualMouse(Vector2.zero, false);

        if (cursorVisual != null)
            cursorVisual.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager != null)
        {
            inputManager.GamepadConfirmChanged += OnConfirmChanged;
            inputManager.GamepadBackStarted += OnBack;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.GamepadConfirmChanged -= OnConfirmChanged;
            inputManager.GamepadBackStarted -= OnBack;
        }

        if (virtualMouse != null && virtualMouse.added)
            InputSystem.QueueStateEvent(virtualMouse, new MouseState());
    }

    private void OnDestroy()
    {
        if (virtualMouse != null && virtualMouse.added)
            InputSystem.RemoveDevice(virtualMouse);
    }

    private void Update()
    {
        if (inputManager == null || virtualMouse == null || !virtualMouse.added)
            return;

        bool cursorActive = inputManager.GamepadUIOpen;

        if (cursorVisual != null && cursorVisual.gameObject.activeSelf != cursorActive)
        {
            cursorVisual.gameObject.SetActive(cursorActive);

            if (cursorActive)
                cursorVisual.SetAsLastSibling();
        }

        if (!cursorActive)
            return;

        Vector2 stick = inputManager.GamepadAim;

        if (stick.sqrMagnitude > 0.04f)
        {
            Vector2 delta = stick * cursorSpeed * Time.unscaledDeltaTime;
            cursorPosition += delta;
            cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0f, Screen.width);
            cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0f, Screen.height);

            UpdateVirtualMouse(delta, false);
        }

        if (cursorVisual != null && canvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                cursorPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            cursorVisual.position = canvas.transform.TransformPoint(localPoint);
        }
    }

    private void UpdateVirtualMouse(Vector2 delta, bool click)
    {
        MouseState state = new MouseState
        {
            position = cursorPosition,
            delta = delta
        };

        if (click)
            state = state.WithButton(MouseButton.Left);

        InputSystem.QueueStateEvent(virtualMouse, state);
    }

    private void OnConfirmChanged(bool pressed)
    {
        if (virtualMouse == null || !virtualMouse.added)
            return;

        MouseState state = new MouseState
        {
            position = cursorPosition
        };

        if (pressed)
            state = state.WithButton(MouseButton.Left);

        InputSystem.QueueStateEvent(virtualMouse, state);
    }

    private void OnBack()
    {
        if (virtualMouse == null || !virtualMouse.added)
            return;

        MouseState state = new MouseState
        {
            position = cursorPosition
        };

        InputSystem.QueueStateEvent(virtualMouse, state);
    }
}