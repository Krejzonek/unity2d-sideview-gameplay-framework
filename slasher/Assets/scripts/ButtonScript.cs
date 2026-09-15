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
        eqPanel.SetActive(false);
        selectPanel.SetActive(false);
        consumablePanel.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public void StatsButton()
    {
        if (statsPanelVisible)
        {
            statsPanel.SetActive(false);
            statsPanelVisible = false;
            player.SetCanAttack(true);
            return;
        }

        if (eqPanelVisible || consumablePanelVisible)
            return;

        player.SetCanAttack(false);
        statsPanel.SetActive(true);
        statsPanelVisible = true;
    }

    public void eqButton()
    {
        if (eqPanelVisible)
        {
            eqPanel.SetActive(false);
            eqPanelVisible = false;
            player.SetCanAttack(true);
            return;
        }

        if (statsPanelVisible || consumablePanelVisible)
            return;

        player.SetCanAttack(false);
        eqPanel.SetActive(true);
        eqPanelVisible = true;
    }

    public void SelectButton()
    {
        selectPanel.SetActive(false);
        player.SetCanAttack(false);
    }

    public void ConsumableButton()
    {
        if (consumablePanelVisible)
        {
            consumablePanel.SetActive(false);
            consumablePanelVisible = false;
            player.SetCanAttack(true);
            return;
        }

        if (statsPanelVisible || eqPanelVisible)
            return;

        player.SetCanAttack(false);
        consumablePanel.SetActive(true);
        consumablePanelVisible = true;
    }
}