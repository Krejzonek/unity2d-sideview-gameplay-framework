using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject eqPanel;
    [SerializeField] private GameObject selectPanel;

    private bool statsPanelVisible = false;
    private bool eqPanelVisible = false;
   
    private void Start()
    {
        statsPanel.SetActive(false);
        eqPanel.SetActive(false);
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
    public void eqButton()
    {
        if (!eqPanelVisible)
        {
            eqPanel.SetActive(true);
            eqPanelVisible = true;
        }
        else
        {
            eqPanel.SetActive(false);
            eqPanelVisible = false;
        }

    }
    public void SelectButton()
    {
        selectPanel.SetActive(false);     
    }
}
