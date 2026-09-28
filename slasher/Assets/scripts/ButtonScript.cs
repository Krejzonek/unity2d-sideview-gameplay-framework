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

    private Player GetCurrentPlayer()
    {
        if (TeamManager.Instance == null)
            return null;

        return TeamManager.Instance.CurrentPlayer;
    }

    private void Start()
    {
        statsPanel.SetActive(false);
        eqPanel.SetActive(false);
        selectPanel.SetActive(false);
        consumablePanel.SetActive(false);
    }

    public void StatsButton()
    {
        Player player = GetCurrentPlayer();

        if (player == null)
            return;

        if (statsPanelVisible)
        {
            statsPanel.SetActive(false);
            statsPanelVisible = false;

            CloseDescriptionPanel();

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
        Player player = GetCurrentPlayer();

        if (player == null)
            return;

        if (eqPanelVisible)
        {
            eqPanel.SetActive(false);
            eqPanelVisible = false;

            EquipmentManager equipmentManager = player.GetComponent<EquipmentManager>();

            if (equipmentManager != null)
                equipmentManager.CloseItemList();

            CloseDescriptionPanel();

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
        Player player = GetCurrentPlayer();

        if (player == null)
            return;

        selectPanel.SetActive(false);
        CloseDescriptionPanel();
        player.SetCanAttack(false);
    }

    public void ConsumableButton()
    {
        Player player = GetCurrentPlayer();

        if (player == null)
            return;

        if (consumablePanelVisible)
        {
            consumablePanel.SetActive(false);
            consumablePanelVisible = false;

            CloseDescriptionPanel();

            player.SetCanAttack(true);
            return;
        }

        if (statsPanelVisible || eqPanelVisible)
            return;

        player.SetCanAttack(false);
        consumablePanel.SetActive(true);
        consumablePanelVisible = true;
    }

    private void CloseDescriptionPanel()
    {
        if (ItemDescriptionPanel.Instance != null)
            ItemDescriptionPanel.Instance.Close();
    }
}