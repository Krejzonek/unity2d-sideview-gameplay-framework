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
    private Player player;
   
    private void Start()
    {
        statsPanel.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        eqPanel.SetActive(false);
    }
    public void StatsButton()
    {
        if (!statsPanelVisible)
        {
            player.SetCanAttack(false);
            statsPanel.SetActive(true);           
            statsPanelVisible = true;
        }
        else
        {
            player.SetCanAttack(true);
            statsPanel.SetActive(false);            
            statsPanelVisible = false;
        }
    }
    public void eqButton()
    {
        if (!eqPanelVisible)
        {
            player.SetCanAttack(false);
            eqPanel.SetActive(true);       
            eqPanelVisible = true;
        }
        else
        {
            player.SetCanAttack(true);
            eqPanel.SetActive(false);
            eqPanelVisible = false;
        }

    }
    public void SelectButton()
    {
        selectPanel.SetActive(false);
        player.SetCanAttack(true);
    }
    public void ConsumableButton()
    {
        if (!consumablePanelVisible) 
        {
            player.SetCanAttack(false);
            consumablePanel.SetActive(true);
            consumablePanelVisible = true;
        }
        else
        {
            player.SetCanAttack(true);
            consumablePanel.SetActive(false);        
            consumablePanelVisible = false;
        }
    }

    
}
