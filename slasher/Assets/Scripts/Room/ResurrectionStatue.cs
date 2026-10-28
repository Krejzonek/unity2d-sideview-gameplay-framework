using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResurrectionStatue : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private LayerMask statueLayer;

    [Header("UI")]
    [SerializeField] private GameObject resurrectionPanel;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private TMP_Text infoText;

    private Camera cam;
    private Player selectedFallenPlayer;
    private Player interactingPlayer;

    private void Start()
    {
        cam = Camera.main;

        if (resurrectionPanel != null)
            resurrectionPanel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || cam == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePosition, statueLayer);

        if (hit != null && hit.transform == transform)
        {
            Player.BlockAttackUntilMouseRelease();
            OpenResurrectionPanel();
        }
    }

    public void OpenResurrectionPanel()
    {
        if (TeamManager.Instance == null)
            return;

        Player currentPlayer = TeamManager.Instance.CurrentPlayer;

        if (currentPlayer == null || currentPlayer.IsDead())
            return;

        if (resurrectionPanel == null || resurrectionPanel.activeSelf)
            return;

        if (buttonsParent == null || buttonPrefab == null)
            return;

        interactingPlayer = currentPlayer;
        selectedFallenPlayer = null;
        resurrectionPanel.SetActive(true);

        ShowFallenCharacters();
    }

    private void ShowFallenCharacters()
    {
        ClearButtons();

        if (infoText != null)
            infoText.text = "Choose an ally to revive.";

        if (TeamManager.Instance == null)
            return;

        bool foundFallen = false;

        foreach (Player player in TeamManager.Instance.Team)
        {
            if (player == null || !player.IsDead() || player.GetCharacterData() == null)
                continue;

            foundFallen = true;
            Player fallenPlayer = player;

            string label = fallenPlayer.GetCharacterData().characterName + " - Lv. " + fallenPlayer.lvl;

            CreateButton(label, () => SelectFallenCharacter(fallenPlayer));
        }

        if (!foundFallen && infoText != null)
            infoText.text = "No fallen allies to revive.";
    }

    private void SelectFallenCharacter(Player player)
    {
        if (player == null || !player.IsDead())
            return;

        selectedFallenPlayer = player;
        ShowPayers();
    }

    private void ShowPayers()
    {
        ClearButtons();

        if (selectedFallenPlayer == null || TeamManager.Instance == null)
        {
            ShowFallenCharacters();
            return;
        }

        string fallenName = selectedFallenPlayer.GetCharacterData() != null
            ? selectedFallenPlayer.GetCharacterData().characterName
            : "Ally";

        if (infoText != null)
            infoText.text = "Choose who pays 1 level to revive " + fallenName + ".";

        bool foundPayer = false;
        List<Player> team = TeamManager.Instance.Team;

        if (selectedFallenPlayer.lvl > 1)
        {
            foundPayer = true;
            Player fallenPlayer = selectedFallenPlayer;
            string name = fallenPlayer.GetCharacterData() != null
                ? fallenPlayer.GetCharacterData().characterName
                : "Ally";

            CreateButton(name + " - Lv. " + fallenPlayer.lvl + " -> " + (fallenPlayer.lvl - 1) + " (Self)", () => ConfirmResurrection(fallenPlayer));
        }

        foreach (Player player in team)
        {
            if (player == null || player == selectedFallenPlayer)
                continue;

            if (player.IsDead() || player.lvl <= 1)
                continue;

            foundPayer = true;
            Player payer = player;
            string name = payer.GetCharacterData() != null
                ? payer.GetCharacterData().characterName
                : "Ally";

            CreateButton(name + " - Lv. " + payer.lvl + " -> " + (payer.lvl - 1), () => ConfirmResurrection(payer));
        }

        if (!foundPayer && infoText != null)
            infoText.text = "No character above level 1 can pay the cost.";

        CreateButton("Back", ShowFallenCharacters);
    }

    private void ConfirmResurrection(Player payer)
    {
        if (selectedFallenPlayer == null || interactingPlayer == null || TeamManager.Instance == null)
        {
            ClosePanel();
            return;
        }

        if (interactingPlayer.IsDead())
        {
            ClosePanel();
            return;
        }

        Vector3 spawnPosition = transform.position;
        spawnPosition.z = selectedFallenPlayer.transform.position.z;

        if (TeamManager.Instance.ReviveAlly(selectedFallenPlayer, payer, spawnPosition))
            ClosePanel();
        else if (infoText != null)
            infoText.text = "Resurrection failed. Try again.";
    }

    private void CreateButton(string label, UnityEngine.Events.UnityAction action)
    {
        Button button = Instantiate(buttonPrefab, buttonsParent);
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
            buttonText.text = label;
    }

    private void ClearButtons()
    {
        if (buttonsParent == null)
            return;

        foreach (Transform child in buttonsParent)
            Destroy(child.gameObject);
    }

    public void ClosePanel()
    {
        if (resurrectionPanel != null)
            resurrectionPanel.SetActive(false);

        ClearButtons();

        selectedFallenPlayer = null;
        interactingPlayer = null;
    }
}