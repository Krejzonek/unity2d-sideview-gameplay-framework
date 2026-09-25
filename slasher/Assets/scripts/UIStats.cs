using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    [SerializeField] private Stats stats;
    [SerializeField] private Player player;

    [Header("Stats")]
    [SerializeField] private TMP_Text freePoints;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text stamina;
    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text luck;

    [Header("UI")]
    [SerializeField] private Image classImage;
    [SerializeField] private TMP_Text className;
    [SerializeField] private TMP_Text lvltext;

    private bool statsPanelVisible = false;

    private void Update()
    {
        freePoints.text = $"Points: {stats.freePoint}";
        health.text = $"Health: {stats.health}";
        stamina.text = $"Stamina: {stats.stamina}";
        speed.text = $"Speed: {stats.speed}";
        attack.text = $"Attack: {stats.attack}";
        luck.text = $"Luck: {stats.luck}";

        classImage.color = stats.characterClass != null ? stats.characterClass.classColor : Color.gray;
        className.text = stats.characterClass != null ? stats.characterClass.className : "None";
        lvltext.text = $"LVL: {player.lvl}";
    }

    
}
