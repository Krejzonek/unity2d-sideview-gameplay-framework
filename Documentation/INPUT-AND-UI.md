# Input and UI

## Input abstraction

`GameInputManager` is the central input layer. It exposes movement/aim values and events for gameplay actions, menu actions, interaction, pause and binding changes. `GameInputAction` defines the gameplay action identifiers. Use this layer when adding new gameplay input so keyboard, gamepad and mobile support can remain coordinated.

`GameInputSettings` is a ScriptableObject for mobile input options such as virtual joystick/action-button enablement and joystick side. `SettingsRebindPanel` and `RebindButton` support rebinding UI.

## Add a gameplay action

1. Add or reuse the relevant `GameInputAction` value.
2. Configure the corresponding input binding in the project's input setup.
3. Make sure `GameInputManager` reads and raises the action event.
4. Subscribe the gameplay component to the action event and unsubscribe when appropriate.
5. Add a rebind UI entry if the action should be user-configurable.
6. Test keyboard/mouse and gamepad separately.

Do not read the same action independently in several scripts if the action is already provided by `GameInputManager`; duplicate input paths can cause double activation.

## Gamepad UI

`GamepadInteraction`, `GamepadUICursor`, `GamepadUIState` and `NameInputGamepadNavigation` help manage navigation and focus. Panels should register/open/close consistently so `GamepadUIState` reflects whether a gamepad-controlled UI panel is active.

## Mobile controls

The mobile input components include `MobileControls`, `VirtualJoystick`, `MobileActionButton`, `MobileLayoutEditor` and `MobileLayoutDrag`. Configure the mobile controls object and its references in the scene/canvas. Test joystick movement, aim, action buttons, layout editing and safe-area/responsive behavior on an actual device.

## Interactions

`IInteractable` is the interaction contract used by interactable objects. Implement it on a component that can be found by the interaction flow, then ensure the object has an appropriate collider and is in the expected interaction range. NPCs and room interactions are examples of world interactions.

## UI components

The project includes `HUD`, `UIStats`, `StatsPanelManager`, `EquipmentPanelManager`, `ItemDescriptionPanel`, inventory item buttons, `ConfirmationDialog`, `PauseMenu` and settings/menu scripts. Duplicate existing prefabs where possible so layout, navigation and serialized references remain intact. When creating a new panel, verify mouse interaction, gamepad focus, panel-open attack blocking and responsive canvas behavior.
