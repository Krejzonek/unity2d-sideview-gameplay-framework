using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VirtualKeyboardUI : MonoBehaviour, ICancelHandler
{
    [Header("Keyboard")]
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private Vector2 keySize = new Vector2(60f, 56f);
    [SerializeField] private float keySpacing = 6f;
    [SerializeField] private float rowSpacing = 8f;
    [SerializeField] private int maxNameLength = 16;

    private TMP_InputField targetInput;
    private GameObject previousSelected;
    private bool capsLock;

    private readonly List<Button> generatedButtons = new List<Button>();
    private readonly List<List<Button>> navigationRows = new List<List<Button>>();
    private readonly List<TMP_Text> letterTexts = new List<TMP_Text>();

    private Button capsButton;

    public bool IsOpen => keyboardPanel != null && keyboardPanel.activeSelf;

    private static readonly string[][] keyboardRows =
    {
        new string[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" },
        new string[] { "a", "s", "d", "f", "g", "h", "j", "k", "l" },
        new string[] { "z", "x", "c", "v", "b", "n", "m" }
    };

    private void Awake()
    {
        if (keyboardPanel == null)
            keyboardPanel = gameObject;

        GenerateKeyboard();
        keyboardPanel.SetActive(false);
    }

    private void GenerateKeyboard()
    {
        VerticalLayoutGroup vertical = keyboardPanel.GetComponent<VerticalLayoutGroup>();

        if (vertical == null)
            vertical = keyboardPanel.AddComponent<VerticalLayoutGroup>();

        vertical.childAlignment = TextAnchor.MiddleCenter;
        vertical.spacing = rowSpacing;
        vertical.padding = new RectOffset(8, 8, 8, 8);
        vertical.childControlWidth = false;
        vertical.childControlHeight = false;
        vertical.childForceExpandWidth = false;
        vertical.childForceExpandHeight = false;

        float keyboardWidth = keySize.x * 10f + keySpacing * 9f;

        foreach (string[] row in keyboardRows)
        {
            GameObject rowObject = CreateRow(keyboardWidth);
            List<Button> buttons = new List<Button>();

            foreach (string key in row)
            {
                string letter = key;

                Button button = CreateKey(rowObject.transform, GetLetter(letter), () => AddCharacter(GetLetter(letter)));
                TMP_Text text = button.GetComponentInChildren<TMP_Text>();

                letterTexts.Add(text);
                buttons.Add(button);
            }

            navigationRows.Add(buttons);
        }

        GameObject actionRow = CreateRow(keyboardWidth);
        List<Button> actionButtons = new List<Button>();

        capsButton = CreateKey(actionRow.transform, "CAPS", ToggleCapsLock, 75f);
        actionButtons.Add(capsButton);
        actionButtons.Add(CreateKey(actionRow.transform, "BACK", Backspace, 78f));
        actionButtons.Add(CreateKey(actionRow.transform, "SPACE", AddSpace, 100f));
        actionButtons.Add(CreateKey(actionRow.transform, "DONE", Close, 78f));

        navigationRows.Add(actionButtons);

        SetupKeyboardNavigation();
    }

    private GameObject CreateRow(float width)
    {
        GameObject rowObject = new GameObject("KeyboardRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObject.transform.SetParent(keyboardPanel.transform, false);

        RectTransform rect = rowObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, keySize.y);

        HorizontalLayoutGroup horizontal = rowObject.GetComponent<HorizontalLayoutGroup>();
        horizontal.childAlignment = TextAnchor.MiddleCenter;
        horizontal.spacing = keySpacing;
        horizontal.childControlWidth = false;
        horizontal.childControlHeight = false;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;

        LayoutElement layout = rowObject.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = keySize.y;
        layout.minWidth = width;
        layout.minHeight = keySize.y;

        return rowObject;
    }

    private Button CreateKey(Transform parent, string label, UnityEngine.Events.UnityAction action, float width = -1f)
    {
        float actualWidth = width > 0f ? width : keySize.x;

        GameObject keyObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        keyObject.transform.SetParent(parent, false);

        RectTransform rect = keyObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(actualWidth, keySize.y);

        Image image = keyObject.GetComponent<Image>();
        image.color = new Color32(55, 55, 65, 255);

        Button button = keyObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(() => action?.Invoke());

        ColorBlock colors = button.colors;
        colors.normalColor = new Color32(55, 55, 65, 255);
        colors.highlightedColor = new Color32(100, 120, 160, 255);
        colors.selectedColor = new Color32(100, 120, 160, 255);
        colors.pressedColor = new Color32(130, 150, 190, 255);
        colors.disabledColor = new Color32(55, 55, 65, 255);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        LayoutElement layout = keyObject.GetComponent<LayoutElement>();
        layout.preferredWidth = actualWidth;
        layout.preferredHeight = keySize.y;
        layout.minWidth = actualWidth;
        layout.minHeight = keySize.y;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(keyObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = label.Length > 1 ? 13f : 20f;
        text.color = Color.white;
        text.raycastTarget = false;

        generatedButtons.Add(button);

        return button;
    }

    private void SetupKeyboardNavigation()
    {
        for (int row = 0; row < navigationRows.Count; row++)
        {
            List<Button> buttons = navigationRows[row];

            for (int column = 0; column < buttons.Count; column++)
            {
                int previousRow = Mathf.Max(0, row - 1);
                int nextRow = Mathf.Min(navigationRows.Count - 1, row + 1);

                int upColumn = GetNearestColumn(column, buttons.Count, navigationRows[previousRow].Count);
                int downColumn = GetNearestColumn(column, buttons.Count, navigationRows[nextRow].Count);

                Navigation navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = buttons[Mathf.Max(0, column - 1)],
                    selectOnRight = buttons[Mathf.Min(buttons.Count - 1, column + 1)],
                    selectOnUp = navigationRows[previousRow][upColumn],
                    selectOnDown = navigationRows[nextRow][downColumn]
                };

                buttons[column].navigation = navigation;
            }
        }
    }

    private int GetNearestColumn(int column, int currentCount, int targetCount)
    {
        if (currentCount <= 1 || targetCount <= 1)
            return 0;

        float normalizedPosition = (float)column / (currentCount - 1);
        return Mathf.RoundToInt(normalizedPosition * (targetCount - 1));
    }

    public void Open(TMP_InputField input)
    {
        if (input == null || keyboardPanel == null || IsOpen)
            return;

        targetInput = input;
        capsLock = false;

        UpdateLetterLabels();
        UpdateCapsButton();

        if (EventSystem.current != null)
            previousSelected = EventSystem.current.currentSelectedGameObject;

        targetInput.DeactivateInputField();
        keyboardPanel.SetActive(true);

        if (EventSystem.current != null && generatedButtons.Count > 0)
            EventSystem.current.SetSelectedGameObject(generatedButtons[0].gameObject);
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        TMP_InputField inputToRestore = targetInput;

        keyboardPanel.SetActive(false);

        if (EventSystem.current != null)
        {
            if (inputToRestore != null && inputToRestore.gameObject.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(inputToRestore.gameObject);
            else if (previousSelected != null && previousSelected.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousSelected);
            else
                EventSystem.current.SetSelectedGameObject(null);
        }

        targetInput = null;
        previousSelected = null;
    }

    public void OnCancel(BaseEventData eventData)
    {
        eventData.Use();
    }

    private void AddCharacter(string character)
    {
        if (targetInput == null || targetInput.text.Length >= maxNameLength)
            return;

        targetInput.text += character;
        targetInput.caretPosition = targetInput.text.Length;
    }

    private void AddSpace()
    {
        if (targetInput == null || targetInput.text.Length >= maxNameLength)
            return;

        if (targetInput.text.Length > 0 && !targetInput.text.EndsWith(" "))
            AddCharacter(" ");
    }

    private void Backspace()
    {
        if (targetInput == null || targetInput.text.Length == 0)
            return;

        targetInput.text = targetInput.text.Substring(0, targetInput.text.Length - 1);
        targetInput.caretPosition = targetInput.text.Length;
    }

    private string GetLetter(string letter)
    {
        return capsLock ? letter.ToUpper() : letter.ToLower();
    }

    private void ToggleCapsLock()
    {
        capsLock = !capsLock;

        UpdateLetterLabels();
        UpdateCapsButton();
    }

    private void UpdateLetterLabels()
    {
        int index = 0;

        foreach (string[] row in keyboardRows)
        {
            foreach (string letter in row)
            {
                if (index < letterTexts.Count && letterTexts[index] != null)
                    letterTexts[index].text = GetLetter(letter);

                index++;
            }
        }
    }

    private void UpdateCapsButton()
    {
        if (capsButton == null || capsButton.image == null)
            return;

        capsButton.image.color = capsLock
            ? new Color32(80, 120, 180, 255)
            : new Color32(55, 55, 65, 255);
    }
}