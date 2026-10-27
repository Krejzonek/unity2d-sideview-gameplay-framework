using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private TMP_Text[] characterTexts;
    [SerializeField] private Button[] characterButtons;

    [Header("Create Character")]
    [SerializeField] private GameObject createPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text errorText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private int selectedSlot = -1;
    private bool characterSwitchMode;

    private void Awake()
    {
        if (createPanel != null)
            createPanel.SetActive(false);
    }

    private void Start()
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager != null && manager.Characters.Count == 0)
            manager.LoadCharactersFromSave();

        characterSwitchMode = manager != null && manager.CharacterSwitchMode;

        if (createPanel != null)
            createPanel.SetActive(false);

        Refresh();
    }

    private void Refresh()
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null || characterTexts == null)
            return;

        for (int i = 0; i < characterTexts.Length; i++)
        {
            CharacterData character = i < manager.Characters.Count ? manager.Characters[i] : null;
            Button button = characterButtons != null && i < characterButtons.Length ? characterButtons[i] : null;

            if (characterTexts[i] != null)
                characterTexts[i].enabled = true;

            if (character != null)
            {
                bool dead = manager.IsCharacterDead(i);
                Color slotColor = character.characterClass != null ? character.characterClass.classColor : Color.gray;
                slotColor.a = dead ? 0.45f : 1f;

                if (characterTexts[i] != null)
                {
                    characterTexts[i].text = character.characterName + "\nLvl " + character.level;
                    characterTexts[i].color = Color.white;
                    characterTexts[i].canvasRenderer.SetAlpha(1f);
                }

                if (button != null)
                {
                    button.gameObject.SetActive(true);
                    button.enabled = true;
                    button.interactable = !dead;
                    button.transition = Selectable.Transition.None;

                    if (button.image != null)
                    {
                        button.image.enabled = true;
                        button.image.color = slotColor;
                        button.image.canvasRenderer.SetAlpha(1f);
                        button.image.CrossFadeAlpha(1f, 0f, true);
                        button.targetGraphic = button.image;
                    }

                    TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

                    if (buttonText != null)
                    {
                        buttonText.enabled = true;
                        buttonText.canvasRenderer.SetAlpha(1f);
                    }
                }
            }
            else
            {
                if (characterTexts[i] != null)
                {
                    characterTexts[i].text = "EMPTY";
                    characterTexts[i].color = new Color32(0x32, 0x32, 0x32, 0xFF);
                    characterTexts[i].canvasRenderer.SetAlpha(1f);
                }

                if (button == null)
                    continue;

                button.gameObject.SetActive(true);
                button.enabled = true;
                button.interactable = manager.Characters.Count < characterTexts.Length;
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };

                if (button.image != null)
                {
                    button.image.enabled = true;
                    button.image.color = Color.white;
                    button.image.canvasRenderer.SetAlpha(1f);
                    button.image.CrossFadeAlpha(1f, 0f, true);
                    button.targetGraphic = button.image;
                }

                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                colors.disabledColor = Color.white;
                colors.colorMultiplier = 1f;
                button.colors = colors;

                CanvasGroup[] canvasGroups = button.GetComponentsInChildren<CanvasGroup>(true);

                foreach (CanvasGroup canvasGroup in canvasGroups)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }

                TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text buttonText in texts)
                {
                    buttonText.enabled = true;
                    buttonText.color = new Color32(0x32, 0x32, 0x32, 0xFF);
                    buttonText.canvasRenderer.SetAlpha(1f);
                }
            }
        }
    }

    public void SelectSlot(int index)
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null || index < 0 || index >= characterTexts.Length)
            return;

        if (index < manager.Characters.Count)
        {
            if (manager.Characters[index] == null || manager.IsCharacterDead(index))
                return;

            if (characterSwitchMode)
            {
                SelectAliveCharacter(index);
                return;
            }

            manager.SelectCharacter(index);
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        if (index != manager.Characters.Count || manager.Characters.Count >= characterTexts.Length)
            return;

        selectedSlot = index;

        if (nameInput != null)
            nameInput.text = "";

        if (errorText != null)
            errorText.text = "";

        if (createPanel != null)
            createPanel.SetActive(true);

        if (nameInput != null)
            nameInput.Select();
    }

    private void SelectAliveCharacter(int index)
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null || index < 0 || index >= manager.Characters.Count)
            return;

        if (manager.Characters[index] == null || manager.IsCharacterDead(index))
            return;

        manager.SelectCharacter(index);
        manager.EndCharacterSwitch();
        SceneManager.LoadScene(gameSceneName);
    }

    public void CreateCharacter()
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null)
            return;

        string characterName = nameInput != null ? nameInput.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(characterName))
        {
            if (errorText != null)
                errorText.text = "Enter character name.";

            return;
        }

        if (selectedSlot < 0 || selectedSlot >= characterTexts.Length || selectedSlot != manager.Characters.Count)
        {
            if (errorText != null)
                errorText.text = "Cannot create character.";

            return;
        }

        if (!manager.CreateCharacter(characterName))
        {
            if (errorText != null)
                errorText.text = "Cannot create character.";

            return;
        }

        manager.SelectCharacter(manager.Characters.Count - 1);
        manager.EndCharacterSwitch();

        if (createPanel != null)
            createPanel.SetActive(false);

        SceneManager.LoadScene(gameSceneName);
    }

    public void CancelCreate()
    {
        selectedSlot = -1;

        if (createPanel != null)
            createPanel.SetActive(false);
    }

    public void RefreshAfterDelete()
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager != null)
        {
            manager.ClearAllCharacters();
            characterSwitchMode = false;
            selectedSlot = -1;
        }

        if (createPanel != null)
            createPanel.SetActive(false);

        Refresh();
    }
}