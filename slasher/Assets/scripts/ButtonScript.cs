using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject eqPanel;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject consumablePanel;

    private bool statsPanelVisible = false;
    private bool eqPanelVisible = false;
    public bool consumablePanelVisible = false;
    
   
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
    public void ConsumableButton()
    {
        if (!consumablePanelVisible) 
        {
            consumablePanel.SetActive(true);
            consumablePanelVisible = true;
        }
        else
        {
            consumablePanel.SetActive(false);
            consumablePanelVisible = false;
        }
    }

    
}
