using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NameInputKeyboardTrigger : MonoBehaviour
{
    [Header("Keyboard")]
    [SerializeField] private VirtualKeyboardUI virtualKeyboard;

    [Header("Input")]
    [SerializeField] private TMP_InputField nameInput;

    [Header("Navigation")]
    [SerializeField] private Selectable downTarget;
    [SerializeField] private Selectable rightTarget;
    [SerializeField] private Selectable upTarget;
    [SerializeField] private Selectable leftTarget;

    private bool submitReleased = true;
    private bool wasKeyboardOpen;
    private Vector2 previousDirection;

    private void Update()
    {
        if (Gamepad.current == null || EventSystem.current == null)
            return;

        bool keyboardOpen = virtualKeyboard != null && virtualKeyboard.IsOpen;

        if (keyboardOpen)
        {
            wasKeyboardOpen = true;
            previousDirection = Vector2.zero;
            return;
        }

        if (wasKeyboardOpen)
        {
            wasKeyboardOpen = false;
            submitReleased = false;
            previousDirection = Vector2.zero;

            return;
        }

        if (!Gamepad.current.buttonSouth.isPressed)
        {
            submitReleased = true;
        }

        if (nameInput == null)
            return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == nameInput.gameObject)
        {
            Vector2 direction = Gamepad.current.leftStick.ReadValue();

            if (direction.sqrMagnitude < 0.25f)
                direction = Gamepad.current.dpad.ReadValue();

            if (direction.magnitude < 0.6f)
                previousDirection = Vector2.zero;
            else if (previousDirection.magnitude < 0.6f)
                SelectDirection(direction);

            previousDirection = direction;
        }
        else
            previousDirection = Vector2.zero;

        if (!Gamepad.current.buttonSouth.wasPressedThisFrame || !submitReleased)
            return;

        submitReleased = false;

        if (virtualKeyboard == null || !nameInput.gameObject.activeInHierarchy)
            return;

        if (EventSystem.current.currentSelectedGameObject != nameInput.gameObject)
            return;

        virtualKeyboard.Open(nameInput);
    }

    private void SelectDirection(Vector2 direction)
    {
        Selectable target = null;

        if (direction.y < -0.6f)
            target = downTarget;
        else if (direction.y > 0.6f)
            target = upTarget;
        else if (direction.x > 0.6f)
            target = rightTarget;
        else if (direction.x < -0.6f)
            target = leftTarget;

        if (target == null || !target.gameObject.activeInHierarchy || !target.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }
}