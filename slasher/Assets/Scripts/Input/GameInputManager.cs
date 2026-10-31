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

    public Vector2 Movement => mobileMovementActive ? mobileMovement : keyboardMovement;
    public Vector2 MobileAim => mobileAim.sqrMagnitude > 0.04f ? mobileAim : gamepadAim;
    public Vector2 GamepadAim => gamepadAim;
    public bool MobileAimActive => !gamepadUIOpen && (mobileAim.sqrMagnitude > 0.04f || gamepadAim.sqrMagnitude > 0.04f);
    public bool GamepadUIOpen => gamepadUIOpen;
    public bool IsGamepadActive { get; private set; }

    private InputAction movementAction;
    private InputAction aimAction;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateInputActions();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        movementAction?.Dispose();
        aimAction?.Dispose();

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

            if (movementAction.activeControl != null && movementAction.activeControl.device is Gamepad && keyboardMovement.sqrMagnitude > 0.04f)
                IsGamepadActive = true;
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
        movementAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        movementAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        movementAction.AddBinding("<Gamepad>/leftStick");

        aimAction = new InputAction("Aim", InputActionType.Value, expectedControlType: "Vector2");
        aimAction.AddBinding("<Gamepad>/rightStick");

        AddButtonAction(GameInputAction.Jump, "<Keyboard>/space", "<Gamepad>/buttonSouth");
        AddButtonAction(GameInputAction.Attack, "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
        AddButtonAction(GameInputAction.Defend, "<Mouse>/rightButton", "<Gamepad>/leftShoulder");
        AddButtonAction(GameInputAction.Skill1, "<Keyboard>/q", "<Gamepad>/buttonWest");
        AddButtonAction(GameInputAction.Crouch, "<Keyboard>/leftCtrl", "<Gamepad>/buttonEast");

        AddMenuAction(GamepadMenuAction.Stats, "<Gamepad>/dpad/up");
        AddMenuAction(GamepadMenuAction.Equipment, "<Gamepad>/dpad/right");
        AddMenuAction(GamepadMenuAction.Consumables, "<Gamepad>/dpad/down");
    }

    private void AddButtonAction(GameInputAction gameAction, string keyboardBinding, string gamepadBinding)
    {
        InputAction action = new InputAction(gameAction.ToString(), InputActionType.Button);
        action.AddBinding(keyboardBinding);
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

    private void EnableInputActions()
    {
        movementAction?.Enable();
        aimAction?.Enable();

        foreach (InputAction action in actions.Values)
            action.Enable();

        foreach (InputAction action in menuActions.Values)
            action.Enable();
    }

    private void DisableInputActions()
    {
        movementAction?.Disable();
        aimAction?.Disable();

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

        if (gamepadInput && gamepadUIOpen)
        {
            if (gameAction == GameInputAction.Jump)
                GamepadConfirmChanged?.Invoke(pressed);
            else if (gameAction == GameInputAction.Crouch && pressed)
                GamepadBackStarted?.Invoke();

            return;
        }

        if (gameAction == GameInputAction.Attack && pressed && !gamepadInput && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

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