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

        if (roomPoints == null)
            roomPoints = GetComponentInParent<RoomPoints>();

        if (!room.IsCleared())
        {
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
        else if (roomPoints != null)
        {
            roomPoints.OpenTheDoor();
        }

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            generator.GenerateNextRoom();
    }
}