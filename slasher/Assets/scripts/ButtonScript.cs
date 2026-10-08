using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject eqPanel;
    [SerializeField] private GameObject consumablePanel;
    [SerializeField] private StatsPanelManager statsPanelManager;
    [SerializeField] private EquipmentPanelManager equipmentPanelManager;

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
        if (statsPanel != null)
            statsPanel.SetActive(false);

        if (eqPanel != null)
            eqPanel.SetActive(false);

        if (consumablePanel != null)
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
           
            return;
        }

        if (eqPanelVisible || consumablePanelVisible)
            return;

        if (statsPanelManager != null)
            statsPanelManager.Refresh();

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

            CloseDescriptionPanel();

         
            return;
        }

        if (statsPanelVisible || consumablePanelVisible)
            return;

        if (equipmentPanelManager != null)
            equipmentPanelManager.Refresh();

      
        eqPanel.SetActive(true);
        eqPanelVisible = true;
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

        
            return;
        }

        if (statsPanelVisible || eqPanelVisible)
            return;

     
        consumablePanel.SetActive(true);
        consumablePanelVisible = true;
    }

    private void CloseDescriptionPanel()
    {
        if (ItemDescriptionPanel.Instance != null)
            ItemDescriptionPanel.Instance.Close();
    }
}