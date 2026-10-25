using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetterItemCharacterButton : MonoBehaviour
{
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private Button changeButton;
    [SerializeField] private Image characterImage;

    private CharacterData character;
    private ItemData item;

    public void Setup(CharacterData characterData, ItemData itemData, float difference)
    {
        character = characterData;
        item = itemData;

        if (characterNameText != null)
            characterNameText.text = character.characterName;

        if (characterImage != null)
        {
            if (character.characterClass == null)
            {
                characterImage.color = Color.gray;
            }
            else
            {
                characterImage.color = character.characterClass.classColor;
            }
        }


        if (powerText != null)
            powerText.text = "+" + difference.ToString("0.##") + " Power";

        if (changeButton != null)
        {
            changeButton.onClick.RemoveAllListeners();
            changeButton.onClick.AddListener(ChangeItem);
        }
    }

    private void ChangeItem()
    {
        if (character == null || item == null)
            return;

        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        equipmentManager.EquipItemToCharacter(character, item);

        BetterItemPopup popup = FindFirstObjectByType<BetterItemPopup>();

        if (popup != null)
            popup.Close();

        if (ItemDescriptionPanel.Instance != null)
            ItemDescriptionPanel.Instance.Close();
    }
}