using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MobileLayoutEditor : MonoBehaviour
{
    [Header("Editor")]
    [SerializeField] private GameObject editorRoot;
    [SerializeField] private TMP_Dropdown controlDropdown;
    [SerializeField] private Slider sizeSlider;

    [Header("Joysticks")]
    [SerializeField] private RectTransform movementJoystick;
    [SerializeField] private RectTransform aimJoystick;

    [Header("Action Buttons")]
    [SerializeField] private RectTransform[] actionButtons;

    private RectTransform selectedControl;
    private Vector2 movementStartPosition;
    private Vector2 aimStartPosition;
    private Vector2[] buttonsStartPositions;
    private Vector2[] defaultButtonPositions;

    private Vector2 defaultMovementPosition = new Vector2(74f, 58f);
    private Vector2 defaultAimPosition = new Vector2(-116f, 58f);

    private float movementStartScale = 1f;
    private float aimStartScale = 1f;
    private bool isOpen;

    private void Awake()
    {
        GameSettings.Load();

        if (actionButtons == null)
            actionButtons = new RectTransform[0];

        defaultButtonPositions = new Vector2[actionButtons.Length];

        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
                defaultButtonPositions[i] = actionButtons[i].anchoredPosition;
        }

        if (controlDropdown != null)
        {
            controlDropdown.ClearOptions();
            controlDropdown.AddOptions(new List<string>
            {
                "Movement Joystick",
                "Aim Joystick"
            });

            controlDropdown.onValueChanged.AddListener(SelectControl);
        }

        if (sizeSlider != null)
        {
            sizeSlider.minValue = 0.5f;
            sizeSlider.maxValue = 2f;
            sizeSlider.SetValueWithoutNotify(1f);
            sizeSlider.onValueChanged.AddListener(ChangeSize);
        }

        isOpen = false;

        if (editorRoot != null)
            editorRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (controlDropdown != null)
            controlDropdown.onValueChanged.RemoveListener(SelectControl);

        if (sizeSlider != null)
            sizeSlider.onValueChanged.RemoveListener(ChangeSize);
    }

    public void OpenEditor()
    {
        GameSettings.Load();

        SaveCurrentLayout();
        ApplySavedLayout();

        isOpen = true;

        if (editorRoot != null)
            editorRoot.SetActive(true);

        if (controlDropdown != null)
            controlDropdown.SetValueWithoutNotify(0);

        SelectControl(0);
    }

    private void SaveCurrentLayout()
    {
        movementStartPosition = GameSettings.MovementPosition;
        aimStartPosition = GameSettings.AimPosition;
        movementStartScale = GameSettings.MovementScale;
        aimStartScale = GameSettings.AimScale;

        buttonsStartPositions = new Vector2[actionButtons.Length];

        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
                buttonsStartPositions[i] = GameSettings.GetActionButtonPosition(i, defaultButtonPositions[i]);
            else
                buttonsStartPositions[i] = Vector2.zero;
        }
    }

    public void SelectControl(int index)
    {
        index = Mathf.Clamp(index, 0, 1);
        selectedControl = index == 0 ? movementJoystick : aimJoystick;

        if (controlDropdown != null)
            controlDropdown.SetValueWithoutNotify(index);

        if (sizeSlider != null && selectedControl != null)
            sizeSlider.SetValueWithoutNotify(selectedControl.localScale.x);
    }

    public void SelectDraggedControl(RectTransform control)
    {
        if (!isOpen || control == null)
            return;

        selectedControl = control;

        if (control == movementJoystick)
        {
            if (controlDropdown != null)
                controlDropdown.SetValueWithoutNotify(0);

            if (sizeSlider != null)
                sizeSlider.SetValueWithoutNotify(control.localScale.x);
        }
        else if (control == aimJoystick)
        {
            if (controlDropdown != null)
                controlDropdown.SetValueWithoutNotify(1);

            if (sizeSlider != null)
                sizeSlider.SetValueWithoutNotify(control.localScale.x);
        }
    }

    private void ChangeSize(float value)
    {
        if (!isOpen || selectedControl == null)
            return;

        if (selectedControl != movementJoystick && selectedControl != aimJoystick)
            return;

        selectedControl.localScale = Vector3.one * value;
    }

    public void SaveLayout()
    {
        if (movementJoystick == null || aimJoystick == null)
            return;

        Vector2[] positions = new Vector2[actionButtons.Length];

        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
                positions[i] = actionButtons[i].anchoredPosition;
            else if (buttonsStartPositions != null && i < buttonsStartPositions.Length)
                positions[i] = buttonsStartPositions[i];
        }

        GameSettings.SetMobileLayout(
            movementJoystick.anchoredPosition,
            aimJoystick.anchoredPosition,
            movementJoystick.localScale.x,
            aimJoystick.localScale.x,
            positions
        );

        SaveCurrentLayout();

        CloseEditor();

        MobileControls[] controls = FindObjectsByType<MobileControls>(FindObjectsSortMode.None);

        foreach (MobileControls control in controls)
        {
            if (control != null)
                control.ApplySettings();
        }
    }

    public void CancelLayout()
    {
        if (isOpen)
            ApplySavedLayout();

        CloseEditor();
    }

    public void ResetLayout()
    {
        if (movementJoystick != null)
        {
            movementJoystick.anchoredPosition = defaultMovementPosition;
            movementJoystick.localScale = Vector3.one;
        }

        if (aimJoystick != null)
        {
            aimJoystick.anchoredPosition = defaultAimPosition;
            aimJoystick.localScale = Vector3.one;
        }

        if (actionButtons != null)
        {
            for (int i = 0; i < actionButtons.Length; i++)
            {
                if (actionButtons[i] != null && i < defaultButtonPositions.Length)
                    actionButtons[i].anchoredPosition = defaultButtonPositions[i];
            }
        }

        if (sizeSlider != null && selectedControl != null)
            sizeSlider.SetValueWithoutNotify(1f);
    }

    private void ApplySavedLayout()
    {
        if (movementJoystick != null)
        {
            movementJoystick.anchoredPosition = movementStartPosition;
            movementJoystick.localScale = Vector3.one * movementStartScale;
        }

        if (aimJoystick != null)
        {
            aimJoystick.anchoredPosition = aimStartPosition;
            aimJoystick.localScale = Vector3.one * aimStartScale;
        }

        if (actionButtons != null && buttonsStartPositions != null)
        {
            for (int i = 0; i < actionButtons.Length && i < buttonsStartPositions.Length; i++)
            {
                if (actionButtons[i] != null)
                    actionButtons[i].anchoredPosition = buttonsStartPositions[i];
            }
        }

        if (sizeSlider != null && selectedControl != null)
            sizeSlider.SetValueWithoutNotify(selectedControl.localScale.x);
    }

    private void CloseEditor()
    {
        isOpen = false;
        selectedControl = null;

        if (editorRoot != null)
            editorRoot.SetActive(false);
    }

    public bool IsOpen => isOpen;
}