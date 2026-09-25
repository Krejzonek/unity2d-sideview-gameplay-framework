using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class UiEq : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Stats stats;
    [SerializeField] private Player player;

    [Header("UI")]
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Image classImage;

    private void Update()
    {
        classText.text = stats.characterClass != null ? stats.characterClass.className : "None";
        classImage.color = stats.characterClass != null ? stats.characterClass.classColor : Color.gray;
        levelText.text = $"LVL: {player.lvl}";
        statsText.text = $" HP  {stats.health} <pos=50%>STA  {stats.stamina}\n SPD  {stats.speed}<pos=50%>ATK  {stats.attack} \n DEF  {stats.defence}<pos=50%>LCK  {stats.luck}\n EXP  {stats.expBonus}";
    }
}
