using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private Room room;
    [SerializeField] private RoomPoints roomPoints;

    private bool activated;
    private HashSet<Player> playersInside = new HashSet<Player>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player player = other.GetComponent<Player>();

        if (player == null)
            return;

        CloseWorldPanels();

        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null && room != null)
            cameraFollow.SetBounds(room.GetMapBounds());

        if (activated)
            return;

        playersInside.Add(player);
        CheckTeamInside();
    }

    private void CheckTeamInside()
    {
        if (TeamManager.Instance == null || room == null)
            return;

        List<Player> team = TeamManager.Instance.Team;

        foreach (Player player in team)
        {
            if (player == null || player.IsDead())
                continue;

            if (!playersInside.Contains(player))
                return;
        }

        activated = true;

        CloseWorldPanels();

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            generator.SetCurrentRoom(room);

        if (!room.IsCleared())
        {
            if (roomPoints == null)
                roomPoints = GetComponentInParent<RoomPoints>();

            if (roomPoints != null)
                roomPoints.CloseTheDoor();

            foreach (Player player in team)
            {
                if (player == null || player.IsDead())
                    continue;

                BotController botController = player.GetComponent<BotController>();

                if (botController != null)
                    botController.SetCombatRoom(room);
            }

            TeamManager.Instance.SetFormationMode(BotFormationMode.Combat);
        }
        else
        {
            if (roomPoints == null)
                roomPoints = GetComponentInParent<RoomPoints>();

            if (roomPoints != null)
                roomPoints.OpenTheDoor();
        }

        if (generator != null)
            generator.GenerateNextRoom();
    }

    private void CloseWorldPanels()
    {
        if (CraftingUI.Instance != null)
            CraftingUI.Instance.CloseAfterRoomChange();

        ClassSelector[] classSelectors = FindObjectsByType<ClassSelector>(FindObjectsSortMode.None);

        foreach (ClassSelector selector in classSelectors)
        {
            if (selector != null)
                selector.CloseClassPanel();
        }

        ResurrectionStatue[] statues = FindObjectsByType<ResurrectionStatue>(FindObjectsSortMode.None);

        foreach (ResurrectionStatue statue in statues)
        {
            if (statue != null)
                statue.ClosePanel();    
        }
    }
}