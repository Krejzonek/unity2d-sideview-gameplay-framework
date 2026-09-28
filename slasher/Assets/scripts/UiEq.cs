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

    private void Update()
    {
        if (TeamManager.Instance == null)
            return;

        Player player = TeamManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        Stats stats = player.GetStats();

        if (stats == null)
            return;

        CharacterData character = player.GetCharacterData();

        if (characterNameText != null)
            characterNameText.text = character != null ? character.characterName : "None";

        if (classText != null)
            classText.text = stats.characterClass != null ? stats.characterClass.className : "None";

        if (classImage != null)
            classImage.color = stats.characterClass != null ? stats.characterClass.classColor : Color.gray;

        if (levelText != null)
            levelText.text = $"LVL: {player.lvl}";

        if (statsText != null)
            statsText.text = $" HP  {stats.health} <pos=50%>STA  {stats.stamina}\n SPD  {stats.speed}<pos=50%>ATK  {stats.attack} \n DEF  {stats.defence}<pos=50%>LCK  {stats.luck}\n EXP  {stats.expBonus}";
    }
}