using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetterItemPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform characterList;
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void Show(ItemData item)
    {
        if (item == null || CharacterManager.Instance == null || characterList == null)
            return;

        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        foreach (Transform child in characterList)
            Destroy(child.gameObject);

        bool hasBetterCharacter = false;

        foreach (CharacterData character in CharacterManager.Instance.Characters)
        {
            if (character == null)
                continue;

            ItemData equippedItem = equipmentManager.GetEquippedItemForCharacter(character, item.equipmentType);

            float newPower = equipmentManager.GetEquipmentPowerForCharacter(item, character);
            float oldPower = equippedItem != null ? equipmentManager.GetEquipmentPowerForCharacter(equippedItem, character) : 0f;

            if (newPower <= oldPower)
                continue;

            GameObject cardObject = Instantiate(characterCardPrefab, characterList);
            BetterItemCharacterButton card = cardObject.GetComponent<BetterItemCharacterButton>();

            if (card != null)
                card.Setup(character, item, newPower - oldPower);

            hasBetterCharacter = true;
        }

        if (!hasBetterCharacter)
            return;

        if (titleText != null)
            titleText.text = "This item is better for these characters:";

        popupPanel.SetActive(true);
    }

    public void Close()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}