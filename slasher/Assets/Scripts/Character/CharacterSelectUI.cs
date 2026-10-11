using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
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

        if (manager == null)
            return;

        for (int i = 0; i < characterTexts.Length; i++)
        {
            CharacterData character = i < manager.Characters.Count ? manager.Characters[i] : null;

            if (character != null)
            {
                characterTexts[i].text = character.characterName + "\nLvl " + character.level;

                bool dead = characterSwitchMode && manager.IsCharacterDead(i);
                Color slotColor = character.characterClass != null ? character.characterClass.classColor : Color.gray;

                if (characterButtons != null && i < characterButtons.Length && characterButtons[i] != null)
                {
                    Button button = characterButtons[i];
                    button.gameObject.SetActive(true);
                    button.interactable = !dead;
                    button.transition = Selectable.Transition.None;
                    button.image.color = dead ? Color.gray : slotColor;
                }

                characterTexts[i].color = Color.white;
            }
            else
            {
                characterTexts[i].text = "EMPTY";
                characterTexts[i].color = new Color32(0x32, 0x32, 0x32, 0xFF);

                if (characterButtons != null && i < characterButtons.Length && characterButtons[i] != null)
                {
                    characterButtons[i].gameObject.SetActive(true);
                    characterButtons[i].interactable = true;
                    characterButtons[i].transition = Selectable.Transition.None;
                    characterButtons[i].image.color = Color.white;

                    TMP_Text buttonText = characterButtons[i].GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                        buttonText.color = new Color32(0x32, 0x32, 0x32, 0xFF);
                }
            }
        }
    }

    public void SelectSlot(int index)
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null || index < 0 || index >= characterTexts.Length)
            return;

        if (characterSwitchMode && index < manager.Characters.Count)
        {
            SelectAliveCharacter(index);
            return;
        }

        if (index < manager.Characters.Count && manager.Characters[index] != null)
        {
            manager.SelectCharacter(index);
            SceneManager.LoadScene(gameSceneName);
            return;
        }

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

        if (selectedSlot < 0 || selectedSlot >= characterTexts.Length || selectedSlot < manager.Characters.Count)
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
            manager.Characters.Clear();
            manager.EndCharacterSwitch();
            characterSwitchMode = false;
        }

        Refresh();
    }
}