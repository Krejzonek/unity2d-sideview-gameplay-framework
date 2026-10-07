using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    [Header("Team")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spacing = 2f;

    [Header("Combo")]
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private float comboResetTime = 3f;

    private List<Player> team = new List<Player>();
    private int currentPlayerIndex;
    private BotFormationMode formationMode = BotFormationMode.Idle;
    private float formationSpeed = 5f;

    private int combo;
    private float comboTimer;

    public Player CurrentPlayer => team.Count > 0 ? team[currentPlayerIndex] : null;
    public List<Player> Team => team;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateTeam();
        UpdateComboText();
    }

    private void Update()
    {
        if (formationMode == BotFormationMode.Idle)
        {
            UpdateFormationSpeed();
            UpdateFormation();
            SetTeamFormationSpeed();
        }
        else
        {
            ClearTeamFormationSpeed();
        }

        UpdateCombo();
    }

    private void UpdateCombo()
    {
        if (combo <= 0)
            return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
        {
            combo = 0;
            comboTimer = 0f;
            UpdateComboText();
        }
    }

    public void RegisterComboHit()
    {
        combo++;
        comboTimer = comboResetTime;

        UpdateComboText();
    }

    public float GetComboDamageMultiplier()
    {
        return 1f + combo * 0.01f;
    }

    public int GetCombo()
    {
        return combo;
    }

    private void UpdateComboText()
    {
        if (comboText == null)
            return;

        if (combo > 0)
            comboText.text = "COMBO x" + combo;
        else
            comboText.text = "";
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

        IgnoreTeamCollisions();
        UpdateFormationSpeed();

        SetFormationMode(BotFormationMode.Idle);
        UpdateFormation();
        SetTeamFormationSpeed();
    }

    private void IgnoreTeamCollisions()
    {
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i] == null)
                continue;

            Collider2D[] collidersA = team[i].GetComponentsInChildren<Collider2D>();

            for (int j = i + 1; j < team.Count; j++)
            {
                if (team[j] == null)
                    continue;

                Collider2D[] collidersB = team[j].GetComponentsInChildren<Collider2D>();

                foreach (Collider2D colliderA in collidersA)
                {
                    foreach (Collider2D colliderB in collidersB)
                    {
                        Physics2D.IgnoreCollision(colliderA, colliderB);
                    }
                }
            }
        }
    }

    private void UpdateFormationSpeed()
    {
        float slowestSpeed = Mathf.Infinity;

        foreach (Player player in team)
        {
            if (player == null || player.IsDead())
                continue;

            Stats stats = player.GetStats();

            if (stats == null)
                continue;

            if (stats.speed < slowestSpeed)
                slowestSpeed = stats.speed;
        }

        if (slowestSpeed != Mathf.Infinity)
            formationSpeed = slowestSpeed;
    }

    private void SetTeamFormationSpeed()
    {
        foreach (Player player in team)
        {
            if (player == null || player.IsDead())
                continue;

            player.SetFormationSpeed(formationSpeed);
        }
    }

    private void ClearTeamFormationSpeed()
    {
        foreach (Player player in team)
        {
            if (player == null)
                continue;

            player.ClearFormationSpeed();
        }
    }

    private void UpdateFormation()
    {
        if (formationMode != BotFormationMode.Idle)
            return;

        Player leader = CurrentPlayer;

        if (leader == null || leader.IsDead())
            return;

        List<Player> formation = new List<Player>();

        foreach (Player player in team)
        {
            if (player == null || player.IsDead())
                continue;

            formation.Add(player);
        }

        formation = formation
            .OrderBy(player => GetFormationPosition(player))
            .ToList();

        int leaderIndex = formation.IndexOf(leader);

        if (leaderIndex < 0)
            return;

        for (int i = 0; i < formation.Count; i++)
        {
            Player player = formation[i];

            if (player == leader)
                continue;

            BotController botController = player.GetComponent<BotController>();

            if (botController == null)
                continue;

            int relativePosition = i - leaderIndex;

            botController.SetFormationPosition(relativePosition);
        }
    }

    private int GetFormationPosition(Player player)
    {
        if (player == null)
            return 0;

        Stats stats = player.GetStats();

        if (stats == null || stats.characterClass == null)
            return 0;

        return stats.characterClass.formationPosition;
    }

    public float GetFormationSpeed()
    {
        return formationSpeed;
    }

    public void SetFormationMode(BotFormationMode mode)
    {
        formationMode = mode;

        foreach (Player player in team)
        {
            if (player == null || !player.IsBot())
                continue;

            BotController botController = player.GetComponent<BotController>();

            if (botController != null)
                botController.SetFormationMode(mode);
        }

        if (mode == BotFormationMode.Idle)
        {
            UpdateFormationSpeed();
            UpdateFormation();
            SetTeamFormationSpeed();
        }
        else
        {
            ClearTeamFormationSpeed();
        }
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

        IgnoreTeamCollisions();
        UpdateFormationSpeed();
        UpdateFormation();
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