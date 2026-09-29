using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    [Header("Team")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spacing = 2f;

    private List<Player> team = new List<Player>();
    private int currentPlayerIndex;

    public Player CurrentPlayer => team.Count > 0 ? team[currentPlayerIndex] : null;
    public List<Player> Team => team;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateTeam();
    }

    private void CreateTeam()
    {
        CharacterManager characterManager = CharacterManager.Instance;

        if (characterManager == null)
            return;

        List<CharacterData> characters = characterManager.Characters;

        if (characters == null || characters.Count == 0)
            return;

        if (playerPrefab == null || spawnPoint == null)
            return;

        int amount = Mathf.Min(characters.Count, 4);
        currentPlayerIndex = Mathf.Clamp(characterManager.SelectedCharacterIndex, 0, amount - 1);

        float startX = -((amount - 1) * spacing) / 2f;

        for (int i = 0; i < amount; i++)
        {
            Vector3 position = spawnPoint.position;
            position.x += startX + i * spacing;

            GameObject characterObject = Instantiate(playerPrefab, position, Quaternion.identity);

            Player player = characterObject.GetComponent<Player>();

            if (player == null)
                continue;

            player.SetCharacterData(characters[i]);
            player.SetTeamManager(this);

            Stats stats = characterObject.GetComponent<Stats>();

            if (stats != null)
                CharacterManager.Instance.ApplyCharacterToPlayer(characters[i], player, stats);

            player.SetBotMode(i != currentPlayerIndex);

            team.Add(player);
        }
    }

    private void Update()
    {
        UpdateFormation();
    }

    private void UpdateFormation()
    {
        Player leader = CurrentPlayer;

        if (leader == null)
            return;

        List<Player> sortedTeam = new List<Player>(team);

        sortedTeam.Sort((a, b) => GetFormationPosition(a).CompareTo(GetFormationPosition(b)));

        int leaderIndex = sortedTeam.IndexOf(leader);

        if (leaderIndex < 0)
            return;

        for (int i = 0; i < sortedTeam.Count; i++)
        {
            Player player = sortedTeam[i];

            if (player == null || player == leader || player.IsDead())
                continue;

            int relativePosition = i - leaderIndex;

            BotController botController = player.GetComponent<BotController>();

            if (botController != null)
                botController.SetFormationPosition(relativePosition);
        }
    }

    private int GetFormationPosition(Player player)
    {
        if (player == null)
            return int.MaxValue;

        Stats stats = player.GetStats();

        if (stats == null)
            return int.MaxValue;

        if (stats.characterClass == null)
            return 0;

        return stats.characterClass.formationPosition + 1;
    }

    public void SwitchCharacter(int index)
    {
        if (index < 0 || index >= team.Count)
            return;

        if (index == currentPlayerIndex)
            return;

        Player newPlayer = team[index];

        if (newPlayer == null || newPlayer.IsDead())
            return;

        Player oldPlayer = team[currentPlayerIndex];

        if (oldPlayer != null)
        {
            oldPlayer.SaveCharacterData();
            oldPlayer.SetBotMode(true);
        }

        currentPlayerIndex = index;

        CharacterManager.Instance.SelectCharacter(index);

        newPlayer.SetBotMode(false);
        newPlayer.RefreshStats();
    }

    public void SaveTeam()
    {
        foreach (Player player in team)
        {
            if (player != null)
                player.SaveCharacterData();
        }
    }

    public Player GetPlayer(int index)
    {
        if (index < 0 || index >= team.Count)
            return null;

        return team[index];
    }

    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public Player FindClosestAlly(Vector3 position, Player excludedPlayer = null)
    {
        Player closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Player player in team)
        {
            if (player == null || player == excludedPlayer || player.IsDead())
                continue;

            float distance = Vector2.Distance(position, player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player;
            }
        }

        return closest;
    }
}