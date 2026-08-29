using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class UiEq : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Stats stats;

    [Header("UI")]
    [SerializeField] private TMP_Text statsText;

    private void Update()
    {
        statsText.text = " HP   " + stats.health + "<pos=50%>STA  " + stats.stamina + "\n" +" SPD  " + stats.speed + "<pos=50%>ATK  " + stats.attack + "\n" +" DEF  " + stats.defence + "<pos=50%>LCK  " + stats.luck + "\n" +" EXP  " + stats.expBonus;
    }
}
