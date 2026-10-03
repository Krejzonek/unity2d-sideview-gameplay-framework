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

    private CharacterData character;
    private Player player;
    private Stats stats;

    public void Setup(CharacterData characterData)
    {
        character = characterData;
        FindPlayer();
        Refresh();
    }

    private void Update()
    {
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
                stats = player.GetStats();
                return;
            }
        }

        player = null;
        stats = null;
    }

    public void Refresh()
    {
        if (character == null)
            return;

        if (player == null || stats == null)
            FindPlayer();

        if (freePoints != null)
            freePoints.text = $"Points: {character.freePoint}";

        if (stats != null)
        {
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
        }

        if (characterNameText != null)
            characterNameText.text = character.characterName;

        if (classImage != null)
            classImage.color = character.characterClass != null ? character.characterClass.classColor : Color.gray;

        if (className != null)
            className.text = character.characterClass != null ? character.characterClass.className : "None";

        if (lvltext != null)
            lvltext.text = $"LVL: {character.level}";
    }

    public void HealthAdd()
    {
        if (stats == null)
            FindPlayer();

        if (stats == null)
            return;

        stats.HealthAdd();
        Refresh();
    }

    public void StaminaAdd()
    {
        if (stats == null)
            FindPlayer();

        if (stats == null)
            return;

        stats.StaminaAdd();
        Refresh();
    }

    public void SpeedAdd()
    {
        if (stats == null)
            FindPlayer();

        if (stats == null)
            return;

        stats.SpeedAdd();
        Refresh();
    }

    public void AttackAdd()
    {
        if (stats == null)
            FindPlayer();

        if (stats == null)
            return;

        stats.AttackAdd();
        Refresh();
    }

    public void LuckAdd()
    {
        if (stats == null)
            FindPlayer();

        if (stats == null)
            return;

        stats.LuckAdd();
        Refresh();
    }
}