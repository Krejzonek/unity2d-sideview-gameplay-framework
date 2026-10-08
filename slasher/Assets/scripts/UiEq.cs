using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiEq : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Image classImage;

    [Header("Item Selection")]
    [SerializeField] private GameObject itemSelectionPanel;
    [SerializeField] private Transform itemListContent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Button closeButton;

    private CharacterData character;
    private Player player;
    private EquipmentManager equipmentManager;
    private EquipmentSlot selectedSlot;

    public void Setup(CharacterData characterData)
    {
        character = characterData;
        equipmentManager = FindFirstObjectByType<EquipmentManager>();
        FindPlayer();
        Refresh();
    }

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseItemList);
            closeButton.onClick.AddListener(CloseItemList);
        }
    }

    private void Update()
    {
        if (character == null)
            return;

        if (player == null)
            FindPlayer();

        Refresh();
    }

    private void FindPlayer()
    {
        if (character == null)
            return;

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player currentPlayer in players)
        {
            if (currentPlayer != null && currentPlayer.GetCharacterData() == character)
            {
                player = currentPlayer;
                return;
            }
        }
    }

    public void OpenItemList(EquipmentSlot slot)
    {
        if (slot == null || character == null)
            return;

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        if (itemSelectionPanel == null || itemListContent == null || itemButtonPrefab == null)
            return;

        selectedSlot = slot;

        

        itemSelectionPanel.SetActive(true);

        RefreshItemList();
    }

    private void RefreshItemList()
    {
        if (itemListContent == null || selectedSlot == null || itemButtonPrefab == null)
            return;

        foreach (Transform child in itemListContent)
            Destroy(child.gameObject);

        var items = equipmentManager.GetInventory()
            .Where(item => item != null)
            .Where(item => selectedSlot.CanEquip(item))
            .Where(item => !equipmentManager.IsItemEquippedByOtherCharacter(item, character))
            .OrderByDescending(item => equipmentManager.GetEquipmentPowerForCharacter(item, character));

        foreach (ItemData item in items)
        {
            GameObject button = Instantiate(itemButtonPrefab, itemListContent);

            ItemButton itemButton = button.GetComponent<ItemButton>();

            if (itemButton != null)
                itemButton.Setup(item, this);
        }
    }

    public void ShowItemDescription(ItemData item)
    {
        if (item == null)
            return;

        if (ItemDescriptionPanel.Instance == null)
            return;

        ItemDescriptionPanel.Instance.ShowEquipment(item, this);
    }

    public void CloseItemList()
    {
        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);

        selectedSlot = null;

        
    }

    public void SelectItem(ItemData item)
    {
        if (item == null || selectedSlot == null)
            return;

        if (!selectedSlot.CanEquip(item))
            return;

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        if (equipmentManager.IsItemEquippedByOtherCharacter(item, character))
            return;

        equipmentManager.EquipItem(selectedSlot, item);

        CloseItemList();
    }

    private void Refresh()
    {
        if (character == null)
            return;

        Class characterClass = character.characterClass;

        if (characterNameText != null)
            characterNameText.text = character.characterName;

        if (classText != null)
            classText.text = characterClass != null ? characterClass.className : "None";

        if (classImage != null)
            classImage.color = characterClass != null ? characterClass.classColor : Color.gray;

        if (levelText != null)
            levelText.text = $"LVL: {character.level}";

        if (statsText == null)
            return;

        if (player != null)
        {
            Stats stats = player.GetStats();

            if (stats != null)
            {
                statsText.text = $" HP  {stats.health} <pos=50%>STA  {stats.stamina}\n SPD  {stats.speed}<pos=50%>ATK  {stats.attack} \n DEF  {stats.defence}<pos=50%>LCK  {stats.luck}\n EXP  {stats.expBonus}";
                return;
            }
        }

        statsText.text = $" HP  {character.healthPoint} <pos=50%>STA  {character.staminaPoint}\n SPD  {character.speedPoint}<pos=50%>ATK  {character.attackPoint} \n DEF  0<pos=50%>LCK  {character.luckPoint}\n EXP  1";
    }
}