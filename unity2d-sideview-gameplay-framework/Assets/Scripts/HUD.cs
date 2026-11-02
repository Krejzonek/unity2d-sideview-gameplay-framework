using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Bars")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider expSlider;

    [Header("Text")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private TMP_Text lvlText;

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

        if (healthSlider != null)
        {
            healthSlider.maxValue = stats.health;
            healthSlider.value = player.GetHealth();
        }

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = stats.stamina;
            staminaSlider.value = player.GetStamina();
        }

        if (expSlider != null)
        {
            expSlider.maxValue = player.lvl * 100;
            expSlider.value = player.currentEXP;
        }

        if (healthText != null)
            healthText.text = Mathf.Round(player.GetHealth()) + "/" + Mathf.Round(stats.health);

        if (staminaText != null)
            staminaText.text = Mathf.Round(player.GetStamina()) + "/" + Mathf.Round(stats.stamina);

        if (lvlText != null)
            lvlText.text = $"Lvl {player.lvl}";
    }
}