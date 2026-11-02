using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class GamepadUICursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursorVisual;
    [SerializeField] private float cursorSpeed = 900f;

    private GameInputManager inputManager;
    private Canvas canvas;
    private Mouse virtualMouse;
    private InputAction submitAction;
    private bool submitSuppressed;
    private Vector2 cursorPosition;
    private bool confirmPressed;

    private void Awake()
    {
        inputManager = GameInputManager.Instance;
        canvas = GetComponentInParent<Canvas>();

        cursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        virtualMouse = InputSystem.AddDevice<Mouse>("Gamepad Virtual Mouse");
        UpdateVirtualMouse(Vector2.zero);

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

        confirmPressed = false;
        SetSubmitSuppressed(false);

        if (virtualMouse != null && virtualMouse.added)
            UpdateVirtualMouse(Vector2.zero);
    }

    private void OnDestroy()
    {
        SetSubmitSuppressed(false);

        if (virtualMouse != null && virtualMouse.added)
            InputSystem.RemoveDevice(virtualMouse);
    }

    private void SetSubmitSuppressed(bool suppress)
    {
        if (submitAction == null)
        {
            InputSystemUIInputModule module = FindObjectOfType<InputSystemUIInputModule>();

            if (module == null || module.submit == null)
                return;

            submitAction = module.submit.action;

            if (submitAction == null)
                return;
        }

        if (suppress)
        {
            if (submitAction.enabled)
                submitAction.Disable();

            submitSuppressed = true;
        }
        else if (submitSuppressed)
        {
            submitAction.Enable();
            submitSuppressed = false;
        }
    }

    private void Update()
    {
        if (inputManager == null || virtualMouse == null || !virtualMouse.added)
            return;

        bool cursorActive = GamepadUIState.IsOpen || inputManager.GamepadUIOpen;

        SetSubmitSuppressed(cursorActive);

        if (cursorVisual != null && cursorVisual.gameObject.activeSelf != cursorActive)
        {
            cursorVisual.gameObject.SetActive(cursorActive);

            if (cursorActive)
            {
                cursorVisual.SetAsLastSibling();
                UpdateVirtualMouse(Vector2.zero);
            }
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

            UpdateVirtualMouse(delta);
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

    private void UpdateVirtualMouse(Vector2 delta)
    {
        MouseState state = new MouseState
        {
            position = cursorPosition,
            delta = delta
        };

        if (confirmPressed)
            state = state.WithButton(MouseButton.Left);

        InputSystem.QueueStateEvent(virtualMouse, state);
    }

    private void OnConfirmChanged(bool pressed)
    {
        if (virtualMouse == null || !virtualMouse.added)
            return;

        if (!GamepadUIState.IsOpen && (inputManager == null || !inputManager.GamepadUIOpen))
            return;

        confirmPressed = pressed;
        UpdateVirtualMouse(Vector2.zero);
    }

    private void OnBack()
    {
        if (virtualMouse == null || !virtualMouse.added)
            return;

        confirmPressed = false;
        UpdateVirtualMouse(Vector2.zero);
    }
}