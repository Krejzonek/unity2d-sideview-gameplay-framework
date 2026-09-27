using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private TMP_Text[] characterTexts;

    [Header("Create Character")]
    [SerializeField] private GameObject createPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text errorText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private int selectedSlot = -1;

    private void Start()
    {
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
                characterTexts[i].text = CharacterManager.Instance.Characters[i].characterName + "\nLvl " + CharacterManager.Instance.Characters[i].level;
            else
                characterTexts[i].text = "EMPTY";
        }
    }

    public void SelectSlot(int index)
    {
        if (CharacterManager.Instance == null)
            return;

        if (index < CharacterManager.Instance.Characters.Count)
        {
            CharacterManager.Instance.SelectCharacter(index);
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        selectedSlot = index;
        nameInput.text = "";
        errorText.text = "";
        createPanel.SetActive(true);
        nameInput.Select();
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
        createPanel.SetActive(false);
    }
}