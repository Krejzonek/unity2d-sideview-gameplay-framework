using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum GamepadMenuAction
{
    Stats,
    Equipment,
    Consumables
}

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }

    public event Action<GameInputAction> ActionStarted;
    public event Action<GameInputAction> ActionEnded;
    public event Action<GamepadMenuAction> MenuActionStarted;
    public event Action GamepadBackStarted;
    public event Action<bool> GamepadConfirmChanged;
    public event Action<GameInputAction, InputDeviceType, string> BindingChanged;
    public event Action InteractStarted;
    public event Action PauseStarted;

    public Vector2 Movement => mobileMovementActive ? mobileMovement : keyboardMovement;
    public Vector2 MobileAim => mobileAim.sqrMagnitude > 0.04f ? mobileAim : gamepadAim;
    public Vector2 GamepadAim => gamepadAim;
    public bool MobileAimActive => !GamepadUIOpen && (mobileAim.sqrMagnitude > 0.04f || gamepadAim.sqrMagnitude > 0.04f);
    public bool GamepadUIOpen => gamepadUIOpen || GamepadUIState.IsOpen;
    public bool IsGamepadActive { get; private set; }

    private InputAction movementAction;
    private InputAction aimAction;
    private InputAction pauseAction;

    private readonly Dictionary<GameInputAction, InputAction> actions = new Dictionary<GameInputAction, InputAction>();
    private readonly Dictionary<GamepadMenuAction, InputAction> menuActions = new Dictionary<GamepadMenuAction, InputAction>();
    private readonly HashSet<GameInputAction> pcHeldActions = new HashSet<GameInputAction>();
    private readonly HashSet<GameInputAction> mobileHeldActions = new HashSet<GameInputAction>();

    private Vector2 keyboardMovement;
    private Vector2 mobileMovement;
    private Vector2 mobileAim;
    private Vector2 gamepadAim;
    private bool mobileMovementActive;
    private bool gamepadUIOpen;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private GameInputAction rebindingAction;
    private InputDeviceType rebindingDevice;

    private const string BindingKeyPrefix = "InputBinding_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        GameSettings.Load();
        CreateInputActions();
        LoadBindings();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        CancelRebinding();
        DisableInputActions();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        movementAction?.Dispose();
        aimAction?.Dispose();
        pauseAction?.Dispose();

        foreach (InputAction action in actions.Values)
            action.Dispose();

        foreach (InputAction action in menuActions.Values)
            action.Dispose();
    }

    private void Update()
    {
        if (movementAction != null)
        {
            keyboardMovement = movementAction.ReadValue<Vector2>();

            if (movementAction.activeControl != null &&
                movementAction.activeControl.device is Gamepad &&
                keyboardMovement.sqrMagnitude > 0.04f)
            {
                IsGamepadActive = true;
            }
        }

        if (aimAction != null)
        {
            gamepadAim = aimAction.ReadValue<Vector2>();

            if (gamepadAim.magnitude < 0.2f)
                gamepadAim = Vector2.zero;

            gamepadAim = Vector2.ClampMagnitude(gamepadAim, 1f);

            if (gamepadAim.sqrMagnitude > 0.04f)
                IsGamepadActive = true;
        }
    }

    private void CreateInputActions()
    {
        movementAction = new InputAction("Movement", InputActionType.Value, expectedControlType: "Vector2");

        movementAction.AddCompositeBinding("2DVector")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        movementAction.AddCompositeBinding("2DVector")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        movementAction.AddBinding("<Gamepad>/leftStick");

        aimAction = new InputAction("Aim", InputActionType.Value, expectedControlType: "Vector2");
        aimAction.AddBinding("<Gamepad>/rightStick");

        AddButtonAction(GameInputAction.Jump, "<Keyboard>/space", "<Gamepad>/buttonSouth");
        AddButtonAction(GameInputAction.Attack, "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
        AddButtonAction(GameInputAction.Defend, "<Mouse>/rightButton", "<Gamepad>/leftShoulder");
        AddButtonAction(GameInputAction.Skill1, "<Keyboard>/q", "<Gamepad>/buttonWest");
        AddButtonAction(GameInputAction.Crouch, "<Keyboard>/leftCtrl", "<Gamepad>/buttonEast");
        AddButtonAction(GameInputAction.Interact, "<Keyboard>/e", "<Gamepad>/buttonNorth");

        AddMenuAction(GamepadMenuAction.Stats, "<Gamepad>/dpad/up");
        AddMenuAction(GamepadMenuAction.Equipment, "<Gamepad>/dpad/right");
        AddMenuAction(GamepadMenuAction.Consumables, "<Gamepad>/dpad/down");

        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseAction.performed += context =>
        {
            if (context.control.device is Gamepad)
                IsGamepadActive = true;

            PauseStarted?.Invoke();
        };
    }

    private void AddButtonAction(GameInputAction gameAction, string keyboardBinding, string gamepadBinding)
    {
        InputAction action = new InputAction(gameAction.ToString(), InputActionType.Button);

        if (!string.IsNullOrEmpty(keyboardBinding))
            action.AddBinding(keyboardBinding);

        if (!string.IsNullOrEmpty(gamepadBinding))
            action.AddBinding(gamepadBinding);

        action.performed += context => SetPCActionState(gameAction, true, context);
        action.canceled += context => SetPCActionState(gameAction, false, context);

        actions.Add(gameAction, action);
    }

    private void AddMenuAction(GamepadMenuAction menuAction, string binding)
    {
        InputAction action = new InputAction(menuAction.ToString(), InputActionType.Button);
        action.AddBinding(binding);

        action.performed += context =>
        {
            if (context.control.device is Gamepad)
            {
                IsGamepadActive = true;
                MenuActionStarted?.Invoke(menuAction);
            }
        };

        menuActions.Add(menuAction, action);
    }

    private bool IsMovementAction(GameInputAction gameAction)
    {
        return gameAction == GameInputAction.MoveLeft ||
               gameAction == GameInputAction.MoveRight;
    }

    private string GetMovementPartName(GameInputAction gameAction)
    {
        switch (gameAction)
        {
            case GameInputAction.MoveLeft:
                return "Left";
            case GameInputAction.MoveRight:
                return "Right";
            default:
                return null;
        }
    }

    private int GetBindingIndex(GameInputAction gameAction, InputDeviceType device)
    {
        if (IsMovementAction(gameAction))
        {
            if (device != InputDeviceType.Keyboard)
                return -1;

            string partName = GetMovementPartName(gameAction);

            for (int i = 0; i < movementAction.bindings.Count; i++)
            {
                InputBinding binding = movementAction.bindings[i];

                if (binding.isPartOfComposite &&
                    binding.name == partName &&
                    binding.path != null &&
                    binding.path.StartsWith("<Keyboard>"))
                {
                    return i;
                }
            }

            return -1;
        }

        if (!actions.ContainsKey(gameAction))
            return -1;

        InputAction action = actions[gameAction];

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (device == InputDeviceType.Keyboard &&
                binding.path != null &&
                (binding.path.StartsWith("<Keyboard>") || binding.path.StartsWith("<Mouse>")))
            {
                return i;
            }

            if (device == InputDeviceType.Gamepad &&
                binding.path != null &&
                binding.path.StartsWith("<Gamepad>") &&
                !binding.isComposite)
            {
                return i;
            }
        }

        return -1;
    }

    private InputAction GetTargetAction(GameInputAction gameAction)
    {
        return IsMovementAction(gameAction) ? movementAction :
            actions.ContainsKey(gameAction) ? actions[gameAction] : null;
    }

    private string GetBindingKey(GameInputAction gameAction, InputDeviceType device)
    {
        return BindingKeyPrefix + gameAction + "_" + device;
    }

    private void LoadBindings()
    {
        foreach (GameInputAction gameAction in Enum.GetValues(typeof(GameInputAction)))
        {
            LoadBinding(gameAction, InputDeviceType.Keyboard);
            LoadBinding(gameAction, InputDeviceType.Gamepad);
        }
    }

    private void LoadBinding(GameInputAction gameAction, InputDeviceType device)
    {
        int bindingIndex = GetBindingIndex(gameAction, device);

        if (bindingIndex < 0)
            return;

        string key = GetBindingKey(gameAction, device);

        if (!PlayerPrefs.HasKey(key))
            return;

        InputAction action = GetTargetAction(gameAction);

        if (action == null)
            return;

        string savedPath = PlayerPrefs.GetString(key);
        action.ApplyBindingOverride(bindingIndex, savedPath);
    }

    public void StartRebinding(GameInputAction gameAction, InputDeviceType device)
    {
        int bindingIndex = GetBindingIndex(gameAction, device);
        InputAction action = GetTargetAction(gameAction);

        if (bindingIndex < 0 || action == null)
            return;

        CancelRebinding();

        rebindingAction = gameAction;
        rebindingDevice = device;

        bool wasEnabled = action.enabled;

        if (wasEnabled)
            action.Disable();

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding(device == InputDeviceType.Gamepad ? "<Keyboard>" : "<Gamepad>")
            .WithControlsExcluding(device == InputDeviceType.Gamepad ? "<Mouse>" : "<Touchscreen>")
            .WithCancelingThrough(device == InputDeviceType.Keyboard ? "<Keyboard>/escape" : "<Gamepad>/buttonEast")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                string path = action.bindings[bindingIndex].effectivePath;

                if (!string.IsNullOrEmpty(path))
                {
                    PlayerPrefs.SetString(GetBindingKey(gameAction, device), path);
                    PlayerPrefs.Save();

                    BindingChanged?.Invoke(
                        gameAction,
                        device,
                        GetBindingDisplayName(gameAction, device)
                    );
                }

                operation.Dispose();
                rebindingOperation = null;

                if (wasEnabled)
                    action.Enable();
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                rebindingOperation = null;

                if (wasEnabled)
                    action.Enable();
            });

        rebindingOperation.Start();
    }

    public void CancelRebinding()
    {
        if (rebindingOperation == null)
            return;

        rebindingOperation.Cancel();
        rebindingOperation.Dispose();
        rebindingOperation = null;
    }

    public string GetBindingDisplayName(GameInputAction gameAction, InputDeviceType device)
    {
        int bindingIndex = GetBindingIndex(gameAction, device);
        InputAction action = GetTargetAction(gameAction);

        if (bindingIndex < 0 || action == null)
            return "-";

        string path = action.bindings[bindingIndex].effectivePath;

        if (string.IsNullOrEmpty(path))
            return "-";

        return InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    public void ResetBindings()
    {
        CancelRebinding();

        foreach (InputAction action in actions.Values)
            action.RemoveAllBindingOverrides();

        movementAction.RemoveAllBindingOverrides();

        foreach (GameInputAction gameAction in Enum.GetValues(typeof(GameInputAction)))
        {
            PlayerPrefs.DeleteKey(GetBindingKey(gameAction, InputDeviceType.Keyboard));
            PlayerPrefs.DeleteKey(GetBindingKey(gameAction, InputDeviceType.Gamepad));
        }

        PlayerPrefs.Save();

        foreach (GameInputAction gameAction in Enum.GetValues(typeof(GameInputAction)))
        {
            BindingChanged?.Invoke(
                gameAction,
                InputDeviceType.Keyboard,
                GetBindingDisplayName(gameAction, InputDeviceType.Keyboard)
            );

            BindingChanged?.Invoke(
                gameAction,
                InputDeviceType.Gamepad,
                GetBindingDisplayName(gameAction, InputDeviceType.Gamepad)
            );
        }
    }

    private void EnableInputActions()
    {
        movementAction?.Enable();
        aimAction?.Enable();
        pauseAction?.Enable();

        foreach (InputAction action in actions.Values)
            action.Enable();

        foreach (InputAction action in menuActions.Values)
            action.Enable();
    }

    private void DisableInputActions()
    {
        movementAction?.Disable();
        aimAction?.Disable();
        pauseAction?.Disable();

        foreach (InputAction action in actions.Values)
            action.Disable();

        foreach (InputAction action in menuActions.Values)
            action.Disable();

        pcHeldActions.Clear();
        mobileHeldActions.Clear();
        gamepadAim = Vector2.zero;
        mobileAim = Vector2.zero;
        gamepadUIOpen = false;
    }

    private void SetPCActionState(GameInputAction gameAction, bool pressed, InputAction.CallbackContext context)
    {
        bool gamepadInput = context.control.device is Gamepad;

        if (gamepadInput)
            IsGamepadActive = true;

        if (gamepadInput && GamepadUIOpen)
        {
            if (gameAction == GameInputAction.Jump)
                GamepadConfirmChanged?.Invoke(pressed);
            else if (gameAction == GameInputAction.Crouch && pressed)
                GamepadBackStarted?.Invoke();

            return;
        }

        if (gameAction == GameInputAction.Attack &&
            pressed &&
            !gamepadInput &&
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (gameAction == GameInputAction.Interact && pressed)
        {
            if (gamepadInput)
                InteractStarted?.Invoke();

            return;
        }

        SetActionState(pcHeldActions, gameAction, pressed);
    }

    private void SetActionState(HashSet<GameInputAction> source, GameInputAction gameAction, bool pressed)
    {
        bool wasHeld = IsActionHeld(gameAction);

        if (pressed)
            source.Add(gameAction);
        else
            source.Remove(gameAction);

        bool isHeld = IsActionHeld(gameAction);

        if (!wasHeld && isHeld)
            ActionStarted?.Invoke(gameAction);
        else if (wasHeld && !isHeld)
            ActionEnded?.Invoke(gameAction);
    }

    public bool IsActionHeld(GameInputAction gameAction)
    {
        return pcHeldActions.Contains(gameAction) || mobileHeldActions.Contains(gameAction);
    }

    public void SetMobileMovement(Vector2 value, bool active)
    {
        mobileMovement = Vector2.ClampMagnitude(new Vector2(value.x, 0f), 1f);
        mobileMovementActive = active;
    }

    public void SetMobileAim(Vector2 value, bool active)
    {
        mobileAim = active ? Vector2.ClampMagnitude(value, 1f) : Vector2.zero;
    }

    public void SetMobileAction(GameInputAction gameAction, bool pressed)
    {
        SetActionState(mobileHeldActions, gameAction, pressed);
    }

    public void SetGamepadUIOpen(bool open)
    {
        gamepadUIOpen = open;
    }

    public void ResetMobileInput()
    {
        mobileMovement = Vector2.zero;
        mobileMovementActive = false;
        mobileAim = Vector2.zero;

        foreach (GameInputAction gameAction in new List<GameInputAction>(mobileHeldActions))
            SetActionState(mobileHeldActions, gameAction, false);
    }
}