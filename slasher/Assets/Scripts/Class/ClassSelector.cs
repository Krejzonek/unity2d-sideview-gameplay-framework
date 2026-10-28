using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClassSelector : MonoBehaviour
{
    [Header("Class Tiers")]
    [SerializeField] private ClassTier[] availableTiers;
    [SerializeField] private GameObject classCardPrefab;

    [Header("UI")]
    [SerializeField] private GameObject uiClass;
    [SerializeField] private Transform classCardsContainer;
    [SerializeField] private LayerMask classStationLayer;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private TMP_Text className;
    [SerializeField] private TMP_Text statBonuses;

    [Header("Passive Skills")]
    [SerializeField] private TMP_Text passiveSkill1Name;
    [SerializeField] private TMP_Text passiveSkill1Description;
    [SerializeField] private TMP_Text passiveSkill2Name;
    [SerializeField] private TMP_Text passiveSkill2Description;

    [Header("Active Skill")]
    [SerializeField] private TMP_Text activeSkillName;
    [SerializeField] private TMP_Text activeSkillDescription;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button chooseButton;
    [SerializeField] private Button closeButton;

    private Class selectedClass;
    private Player currentPlayer;

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(CloseSkillPanel);

        if (chooseButton != null)
            chooseButton.onClick.AddListener(ChooseClass);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseClassPanel);

        if (uiClass != null)
            uiClass.SetActive(false);

        if (skillPanel != null)
            skillPanel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (uiClass == null || uiClass.activeSelf)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition, classStationLayer);

        foreach (Collider2D hit in hits)
        {
            ClassSelector hitSelector = hit.GetComponentInParent<ClassSelector>();

            if (hitSelector == this)
            {
                Player.BlockAttackUntilMouseRelease();
                OpenClassPanel();
                return;
            }
        }
    }

    public void OpenClassPanel()
    {
        if (uiClass == null || TeamManager.Instance == null)
            return;

        currentPlayer = TeamManager.Instance.CurrentPlayer;

        if (currentPlayer == null || currentPlayer.IsDead())
            return;

        uiClass.SetActive(true);

        if (skillPanel != null)
            skillPanel.SetActive(false);

        selectedClass = null;
        GenerateClassCards();
    }

    private void GenerateClassCards()
    {
        if (classCardPrefab == null || classCardsContainer == null || currentPlayer == null)
            return;

        foreach (Transform child in classCardsContainer)
        {
            if (child.GetComponent<ClassCard>() != null)
                Destroy(child.gameObject);
        }

        CharacterData characterData = currentPlayer.GetCharacterData();

        if (characterData == null)
            return;

        Class currentClass = characterData.characterClass;
        ClassTier currentTier = FindTier(currentClass);

        if (currentTier == null || currentTier.nextClasses == null)
            return;

        HashSet<Class> generatedClasses = new HashSet<Class>();

        foreach (Class nextClass in currentTier.nextClasses)
        {
            if (nextClass == null || !generatedClasses.Add(nextClass))
                continue;

            GameObject cardObject = Instantiate(classCardPrefab, classCardsContainer);
            ClassCard card = cardObject.GetComponent<ClassCard>();

            if (card == null)
            {
                Destroy(cardObject);
                continue;
            }

            card.Setup(nextClass, this);
        }
    }

    private ClassTier FindTier(Class classData)
    {
        if (availableTiers == null)
            return null;

        foreach (ClassTier tier in availableTiers)
        {
            if (tier != null && tier.classData == classData)
                return tier;
        }

        return null;
    }

    public void ShowClass(Class classData)
    {
        if (classData == null)
            return;

        selectedClass = classData;
        ShowClassDetails(classData);

        if (skillPanel != null)
            skillPanel.SetActive(true);
    }

    private void ShowClassDetails(Class classData)
    {
        if (className != null)
            className.text = classData.className;

        if (statBonuses != null)
        {
            statBonuses.text =
                $"Health: +{classData.healthBonus}\n" +
                $"Stamina: +{classData.staminaBonus}\n" +
                $"Attack: +{classData.attackBonus}\n" +
                $"Defence: +{classData.defenceBonus}\n" +
                $"Speed: +{classData.speedBonus}";
        }

        SetPassiveSkillText(classData.passiveSkill1, passiveSkill1Name, passiveSkill1Description);
        SetPassiveSkillText(classData.passiveSkill2, passiveSkill2Name, passiveSkill2Description);
        SetActiveSkillText(classData.activeSkill, activeSkillName, activeSkillDescription);
    }

    private void SetPassiveSkillText(PassiveSkill skill, TMP_Text nameText, TMP_Text descriptionText)
    {
        if (nameText != null)
            nameText.text = skill != null ? skill.skillName : "None";

        if (descriptionText != null)
            descriptionText.text = skill != null ? skill.description : "";
    }

    private void SetActiveSkillText(ActiveSkill skill, TMP_Text nameText, TMP_Text descriptionText)
    {
        if (nameText != null)
            nameText.text = skill != null ? skill.skillName : "None";

        if (descriptionText != null)
            descriptionText.text = skill != null ? skill.description : "";
    }

    private void ChooseClass()
    {
        if (selectedClass == null || currentPlayer == null || currentPlayer.IsDead())
            return;

        CharacterData characterData = currentPlayer.GetCharacterData();

        if (characterData == null)
            return;

        characterData.characterClass = selectedClass;

        Stats stats = currentPlayer.GetComponent<Stats>();

        if (stats != null)
            stats.SetClass(selectedClass);

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.SetCharacterClass(selectedClass);

        if (MapSaveSystem.Instance != null)
            MapSaveSystem.Instance.SaveCharactersNow();

        CloseClassPanel();
    }

    private void CloseSkillPanel()
    {
        if (skillPanel != null)
            skillPanel.SetActive(false);

        selectedClass = null;
    }

    public void CloseClassPanel()
    {
        if (uiClass != null)
            uiClass.SetActive(false);

        if (skillPanel != null)
            skillPanel.SetActive(false);

        selectedClass = null;
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(CloseSkillPanel);

        if (chooseButton != null)
            chooseButton.onClick.RemoveListener(ChooseClass);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseClassPanel);
    }
}