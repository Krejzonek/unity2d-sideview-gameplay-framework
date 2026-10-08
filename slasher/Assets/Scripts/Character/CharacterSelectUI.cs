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
        characterSwitchMode = CharacterManager.Instance != null && CharacterManager.Instance.CharacterSwitchMode;

        if (createPanel != null)
            createPanel.SetActive(false);

        Refresh();
    }

    private void Refresh()
    {
        if (CharacterManager.Instance == null)
            return;

        for (int i = 0; i < characterTexts.Length; i++)
        {
            if (i < CharacterManager.Instance.Characters.Count)
            {
                CharacterData character = CharacterManager.Instance.Characters[i];

                characterTexts[i].text = character.characterName + "\nLvl " + character.level;

                bool dead = characterSwitchMode && CharacterManager.Instance.IsCharacterDead(i);

                if (characterButtons != null && i < characterButtons.Length && characterButtons[i] != null)
                {
                    characterButtons[i].gameObject.SetActive(true);

                    if (characterSwitchMode)
                    {
                        characterButtons[i].interactable = !dead;

                        if (dead)
                            characterButtons[i].image.color = Color.gray;
                        else if (character.characterClass != null)
                            characterButtons[i].image.color = character.characterClass.classColor;
                        else
                            characterButtons[i].image.color = Color.white;
                    }
                    else
                    {
                        characterButtons[i].interactable = true;

                        if (character.characterClass != null)
                            characterButtons[i].image.color = character.characterClass.classColor;
                        else
                            characterButtons[i].image.color = Color.gray;
                    }

                    characterTexts[i].color = Color.white;
                }
            }
            else
            {
                characterTexts[i].text = "EMPTY";

                if (characterButtons != null && i < characterButtons.Length && characterButtons[i] != null)
                {
                    characterButtons[i].gameObject.SetActive(true);
                    characterButtons[i].interactable = true;
                    characterButtons[i].image.color = Color.white;
                }
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (CharacterManager.Instance == null)
            return;

        if (characterSwitchMode && index < CharacterManager.Instance.Characters.Count)
        {
            SelectAliveCharacter(index);
            return;
        }

        if (index < CharacterManager.Instance.Characters.Count)
        {
            CharacterManager.Instance.SelectCharacter(index);
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
        if (index < 0 || index >= CharacterManager.Instance.Characters.Count)
            return;

        if (CharacterManager.Instance.IsCharacterDead(index))
            return;

        CharacterManager.Instance.SelectCharacter(index);
        CharacterManager.Instance.EndCharacterSwitch();

        SceneManager.LoadScene(gameSceneName);
    }

    public void CreateCharacter()
    {
        if (CharacterManager.Instance == null)
            return;

        string characterName = nameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(characterName))
        {
            errorText.text = "Enter character name.";
            return;
        }

        if (!CharacterManager.Instance.CreateCharacter(characterName))
        {
            errorText.text = "Cannot create character.";
            return;
        }

        CharacterManager.Instance.SelectCharacter(selectedSlot);

        createPanel.SetActive(false);

        SceneManager.LoadScene(gameSceneName);
    }

    public void CancelCreate()
    {
        selectedSlot = -1;

        if (createPanel != null)
            createPanel.SetActive(false);
    }
}