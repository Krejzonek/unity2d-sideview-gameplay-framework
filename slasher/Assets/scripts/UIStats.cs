using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private TMP_Text freePoints;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text stamina;
    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text luck;

    [Header("UI")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image classImage;
    [SerializeField] private TMP_Text className;
    [SerializeField] private TMP_Text lvltext;

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

        if (freePoints != null)
            freePoints.text = $"Points: {stats.freePoint}";

        if (health != null)
            health.text = $"Health: {stats.health}";

        if (stamina != null)
            stamina.text = $"Stamina: {stats.stamina}";

        if (speed != null)
            speed.text = $"Speed: {stats.speed}";

        if (attack != null)
            attack.text = $"Attack: {stats.attack}";

        if (luck != null)
            luck.text = $"Luck: {stats.luck}";

        if (characterNameText != null)
            characterNameText.text = player.GetCharacterData() != null ? player.GetCharacterData().characterName : "None";

        if (classImage != null)
            classImage.color = stats.characterClass != null ? stats.characterClass.classColor : Color.gray;

        if (className != null)
            className.text = stats.characterClass != null ? stats.characterClass.className : "None";

        if (lvltext != null)
            lvltext.text = $"LVL: {player.lvl}";
    }
}