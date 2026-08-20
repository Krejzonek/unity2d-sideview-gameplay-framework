using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStats : MonoBehaviour
{
    [SerializeField] private Stats stats;

    [Header("TextField")]
    [SerializeField] private TMP_Text freePoints;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text stamina;
    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text luck;

    private bool statsPanelVisible = false;

    private void Update()
    {
        freePoints.text = $"Points: {stats.freePoint}";
        health.text = $"Health: {stats.health}";
        stamina.text = $"Stamina: {stats.stamina}";
        speed.text = $"Speed: {stats.speed}";
        attack.text = $"Attack: {stats.attack}";
        luck.text = $"Luck: {stats.luck}";
    }

    
}
