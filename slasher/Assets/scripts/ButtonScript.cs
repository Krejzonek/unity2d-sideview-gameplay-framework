using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;

    private bool statsPanelVisible = false;
    private void Start()
    {
        statsPanel.SetActive(false);
    }
    public void StatsButton()
    {
        if (!statsPanelVisible)
        {
            statsPanel.SetActive(true);
            statsPanelVisible = true;
        }
        else
        {
            statsPanel.SetActive(false);
            statsPanelVisible = false;
        }
    }
}
