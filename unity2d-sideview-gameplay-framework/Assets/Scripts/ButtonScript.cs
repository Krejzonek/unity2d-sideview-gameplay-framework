using System;
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

    private GameInputManager inputManager;

    private bool AnyPanelVisible => statsPanelVisible || eqPanelVisible || consumablePanelVisible;

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

        UpdateGamepadUIState();
    }

    private void OnEnable()
    {
        inputManager = GameInputManager.Instance;

        if (inputManager != null)
        {
            inputManager.MenuActionStarted += OnMenuActionStarted;
            inputManager.GamepadBackStarted += CloseCurrentPanel;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.MenuActionStarted -= OnMenuActionStarted;
            inputManager.GamepadBackStarted -= CloseCurrentPanel;
            inputManager.SetGamepadUIOpen(false);
        }
    }

    private void OnMenuActionStarted(GamepadMenuAction action)
    {
        switch (action)
        {
            case GamepadMenuAction.Stats:
                if (statsPanelVisible)
                    CloseCurrentPanel();
                else
                {
                    CloseCurrentPanel();
                    StatsButton();
                }
                break;

            case GamepadMenuAction.Equipment:
                if (eqPanelVisible)
                    CloseCurrentPanel();
                else
                {
                    CloseCurrentPanel();
                    eqButton();
                }
                break;

            case GamepadMenuAction.Consumables:
                if (consumablePanelVisible)
                    CloseCurrentPanel();
                else
                {
                    CloseCurrentPanel();
                    ConsumableButton();
                }
                break;
        }

        UpdateGamepadUIState();
    }

    private void UpdateGamepadUIState()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager != null)
            inputManager.SetGamepadUIOpen(AnyPanelVisible);
    }

    public void StatsButton()
    {
        Player player = GetCurrentPlayer();

        if (player == null || statsPanel == null)
            return;

        if (statsPanelVisible)
        {
            statsPanel.SetActive(false);
            statsPanelVisible = false;
            CloseDescriptionPanel();
            UpdateGamepadUIState();
            return;
        }

        if (eqPanelVisible || consumablePanelVisible)
            return;

        if (statsPanelManager != null)
            statsPanelManager.Refresh();

        statsPanel.SetActive(true);
        statsPanelVisible = true;
        UpdateGamepadUIState();
    }

    public void eqButton()
    {
        Player player = GetCurrentPlayer();

        if (player == null || eqPanel == null)
            return;

        if (eqPanelVisible)
        {
            eqPanel.SetActive(false);
            eqPanelVisible = false;
            CloseDescriptionPanel();
            UpdateGamepadUIState();
            return;
        }

        if (statsPanelVisible || consumablePanelVisible)
            return;

        if (equipmentPanelManager != null)
            equipmentPanelManager.Refresh();

        eqPanel.SetActive(true);
        eqPanelVisible = true;
        UpdateGamepadUIState();
    }

    public void ConsumableButton()
    {
        Player player = GetCurrentPlayer();

        if (player == null || consumablePanel == null)
            return;

        if (consumablePanelVisible)
        {
            consumablePanel.SetActive(false);
            consumablePanelVisible = false;
            CloseDescriptionPanel();
            UpdateGamepadUIState();
            return;
        }

        if (statsPanelVisible || eqPanelVisible)
            return;

        consumablePanel.SetActive(true);

        ConsumableHotbar hotbar = consumablePanel.GetComponentInParent<ConsumableHotbar>();

        if (hotbar != null)
            hotbar.ShowAllItems();

        consumablePanelVisible = true;
        UpdateGamepadUIState();
    }

    private void CloseCurrentPanel()
    {
        if (statsPanelVisible)
            StatsButton();
        else if (eqPanelVisible)
            eqButton();
        else if (consumablePanelVisible)
            ConsumableButton();
    }

    private void CloseDescriptionPanel()
    {
        if (ItemDescriptionPanel.Instance != null)
            ItemDescriptionPanel.Instance.Close();
    }
}