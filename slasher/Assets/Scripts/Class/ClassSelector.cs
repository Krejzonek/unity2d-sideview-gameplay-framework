using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelector : MonoBehaviour
{
    [SerializeField] private GameObject uiClass;
    [SerializeField] private LayerMask classStationLayer;

    [Header("Skill Panel")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private TMP_Text skillClassName;

    [Header("Stat Bonuses")]
    [SerializeField] private TMP_Text statBonuses;

    [Header("Passive Skill 1")]
    [SerializeField] private TMP_Text passive1Name;
    [SerializeField] private TMP_Text passive1Description;

    [Header("Passive Skill 2")]
    [SerializeField] private TMP_Text passive2Name;
    [SerializeField] private TMP_Text passive2Description;

    [Header("Active Skill")]
    [SerializeField] private TMP_Text activeName;
    [SerializeField] private TMP_Text activeDescription;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button chooseButton;

    private Camera cam;
    private Player player;
    private Class selectedClass;

    private void Start()
    {
        cam = Camera.main;

        if (uiClass != null)
            uiClass.SetActive(false);

        if (skillPanel != null)
            skillPanel.SetActive(false);

        if (backButton != null)
            backButton.onClick.AddListener(CloseSkillPanel);

        if (chooseButton != null)
            chooseButton.onClick.AddListener(ChooseClass);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(mousePosition, classStationLayer);

        if (hit != null && hit.transform == transform)
        {
            bool isOpen = uiClass.activeSelf;
            uiClass.SetActive(!isOpen);

            player = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

            if (player != null)
            {
                if (isOpen)
                    StartCoroutine(EnableAttack());
                else
                    player.SetCanAttack(false);
            }

            if (isOpen && skillPanel != null)
                skillPanel.SetActive(false);
        }
    }

    public void ShowClass(Class classData)
    {
        if (classData == null)
            return;

        selectedClass = classData;

        if (skillClassName != null)
            skillClassName.text = classData.className;

        if (statBonuses != null)
            statBonuses.text = "HP   " + classData.healthBonus + "<pos=50%>STA  " + classData.staminaBonus + "\n" +
                    "SPD  " + classData.speedBonus + "<pos=50%>ATK  " + classData.attackBonus + "\n" +
                    "DEF  " + classData.defenceBonus;

        if (classData.passiveSkill1 != null)
        {
            passive1Name.text = classData.passiveSkill1.skillName;
            passive1Description.text = classData.passiveSkill1.description;
        }
        else
        {
            passive1Name.text = "";
            passive1Description.text = "";
        }

        if (classData.passiveSkill2 != null)
        {
            passive2Name.text = classData.passiveSkill2.skillName;
            passive2Description.text = classData.passiveSkill2.description;
        }
        else
        {
            passive2Name.text = "";
            passive2Description.text = "";
        }

        if (classData.activeSkill != null)
        {
            activeName.text = classData.activeSkill.skillName;
            activeDescription.text = classData.activeSkill.description;
        }
        else
        {
            activeName.text = "";
            activeDescription.text = "";
        }

        if (skillPanel != null)
            skillPanel.SetActive(true);
    }

    private void CloseSkillPanel()
    {
        if (skillPanel != null)
            skillPanel.SetActive(false);
    }

    private void ChooseClass()
    {
        if (selectedClass == null)
            return;

        player = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

        if (player != null)
        {
            Stats stats = player.GetComponent<Stats>();

            if (stats != null)
                stats.SetClass(selectedClass);
        }

        if (skillPanel != null)
            skillPanel.SetActive(false);

        if (uiClass != null)
            uiClass.SetActive(false);

        if (player != null)
            StartCoroutine(EnableAttack());
    }

    private IEnumerator EnableAttack()
    {
        yield return null;

        if (player != null)
            player.SetCanAttack(true);
    }
}