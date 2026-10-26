using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Death")]
    [SerializeField] private float respawnSpacing = 2f;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button returnToSpawnButton;

    [Header("Scene")]
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";

    private List<Player> team = new List<Player>();
    private int currentPlayerIndex;
    private BotFormationMode formationMode = BotFormationMode.Idle;
    private float formationSpeed = 5f;

    private int combo;
    private float comboTimer;

    private Transform checkpoint;
    private bool respawning;

    public Player CurrentPlayer => currentPlayerIndex >= 0 && currentPlayerIndex < team.Count ? team[currentPlayerIndex] : null;
    public List<Player> Team => team;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (returnToSpawnButton == null && deathPanel != null)
            returnToSpawnButton = deathPanel.GetComponentInChildren<Button>(true);

        if (returnToSpawnButton != null)
            returnToSpawnButton.onClick.AddListener(ReturnToSpawnPoint);

        CreateTeam();
        UpdateComboText();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (returnToSpawnButton != null)
            returnToSpawnButton.onClick.RemoveListener(ReturnToSpawnPoint);
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

    private void UpdateComboText()
    {
        if (comboText == null)
            return;

        comboText.text = combo > 0 ? "COMBO x" + combo : "";
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        if (newCheckpoint == null)
            return;

        checkpoint = newCheckpoint;

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.SetCheckpoint(newCheckpoint.position);
    }

    public void PlayerDied(Player player)
    {
        if (respawning || player == null || player.IsDead() == false)
            return;

        int deadIndex = team.IndexOf(player);

        if (deadIndex >= 0 && CharacterManager.Instance != null)
            CharacterManager.Instance.SetCharacterDead(deadIndex, true);

        StopDeadPlayerFollowing(player);
        player.SaveCharacterData();

        bool allDead = true;

        foreach (Player teamPlayer in team)
        {
            if (teamPlayer != null && !teamPlayer.IsDead())
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            StartDeath();
            return;
        }

        if (player == CurrentPlayer)
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.StartCharacterSwitch();
                CharacterManager.Instance.SaveCharacterSwitchPosition(player.transform.position);
            }

            SceneManager.LoadScene(characterSelectSceneName);
        }
    }

    private void StartDeath()
    {
        if (respawning)
            return;

        respawning = true;

        combo = 0;
        comboTimer = 0f;
        UpdateComboText();

        ApplyDeathPenalty();

        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    private void ApplyDeathPenalty()
    {
        foreach (Player player in team)
        {
            if (player == null || player.GetCharacterData() == null)
                continue;

            player.currentEXP = 0;
            player.SaveCharacterData();
        }
    }

    public void ReturnToSpawnPoint()
    {
        if (!respawning)
            return;

        bool hasCheckpoint = CharacterManager.Instance != null && CharacterManager.Instance.HasCheckpoint;
        Vector3 respawnPosition = hasCheckpoint ? CharacterManager.Instance.CheckpointPosition : spawnPoint != null ? spawnPoint.position : transform.position;

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
        {
            if (hasCheckpoint)
                generator.ResetAfterCheckpoint(respawnPosition);
            else
                generator.ResetToStartRoom();
        }

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.StartRespawn(respawnPosition);

        RespawnTeam(respawnPosition);

        if (deathPanel != null)
            deathPanel.SetActive(false);

        respawning = false;

        if (generator != null && MapSaveSystem.Instance != null)
            MapSaveSystem.Instance.SaveMap(generator);

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.ClearRespawn();
    }

    private void RespawnTeam(Vector3 position)
    {
        int aliveIndex = 0;

        foreach (Player player in team)
        {
            if (player == null)
                continue;

            Vector3 playerPosition = position;
            playerPosition.x += (aliveIndex - (team.Count - 1) / 2f) * respawnSpacing;

            BotController botController = player.GetComponent<BotController>();

            if (botController != null)
                botController.enabled = true;

            player.Respawn(playerPosition);
            aliveIndex++;
        }

        if (CharacterManager.Instance != null)
        {
            for (int i = 0; i < team.Count; i++)
                CharacterManager.Instance.SetCharacterDead(i, false);
        }

        Player currentPlayer = CurrentPlayer;

        foreach (Player player in team)
        {
            if (player == null)
                continue;

            player.SetBotMode(player != currentPlayer);
            player.SaveCharacterData();
        }

        IgnoreTeamCollisions();
        UpdateFormationSpeed();
        UpdateFormation();
        SetTeamFormationSpeed();
    }

    private void CreateTeam()
    {
        CharacterManager characterManager = CharacterManager.Instance;

        if (characterManager == null)
            return;

        List<CharacterData> characters = characterManager.Characters;

        if (characters == null || characters.Count == 0 || playerPrefab == null)
            return;

        int amount = Mathf.Min(characters.Count, 4);
        currentPlayerIndex = Mathf.Clamp(characterManager.SelectedCharacterIndex, 0, amount - 1);

        bool characterSwitch = characterManager.CharacterSwitchPositionSaved;

        List<Vector3> savedPositions = null;
        bool restorePositions = MapSaveSystem.Instance != null && MapSaveSystem.Instance.TryGetSavedTeamPositions(out savedPositions);

        Vector3 teamPosition = spawnPoint != null ? spawnPoint.position : transform.position;

        if (characterSwitch)
            teamPosition = characterManager.CharacterSwitchPosition;
        else if (restorePositions && savedPositions != null && savedPositions.Count > 0)
            teamPosition = savedPositions[0];
        else if (characterManager.HasCheckpoint)
            teamPosition = characterManager.CheckpointPosition;

        float startX = -((amount - 1) * spacing) / 2f;

        for (int i = 0; i < amount; i++)
        {
            Vector3 position = teamPosition;
            bool dead = characterManager.IsCharacterDead(i);

            if (characterSwitch)
            {
                if (dead && restorePositions && savedPositions != null && i < savedPositions.Count)
                    position = savedPositions[i];
                else
                    position.x += startX + i * spacing;
            }
            else if (restorePositions && savedPositions != null && i < savedPositions.Count)
            {
                position = savedPositions[i];
            }
            else
            {
                position.x += startX + i * spacing;
            }

            GameObject characterObject = Instantiate(playerPrefab, position, Quaternion.identity);
            Player player = characterObject.GetComponent<Player>();

            if (player == null)
            {
                Destroy(characterObject);
                continue;
            }

            player.SetCharacterData(characters[i]);
            player.SetTeamManager(this);

            Stats stats = characterObject.GetComponent<Stats>();

            if (stats != null)
                characterManager.ApplyCharacterToPlayer(characters[i], player, stats);

            WeaponVisualEquipment weaponVisual = characterObject.GetComponent<WeaponVisualEquipment>();

            if (weaponVisual != null)
                weaponVisual.RefreshFromCharacter(characters[i]);

            team.Add(player);

            player.SetBotMode(i != currentPlayerIndex);

            if (dead)
            {
                player.SetDead();
                StopDeadPlayerFollowing(player);
            }
        }

        if (characterSwitch)
            StartCoroutine(ForceCharacterSwitchPosition(teamPosition));

        if (characterManager.PendingRespawn)
        {
            Vector3 respawnPosition = characterManager.RespawnPosition;

            for (int i = 0; i < team.Count; i++)
            {
                if (team[i] == null)
                    continue;

                Vector3 position = respawnPosition;
                position.x += (i - (team.Count - 1) / 2f) * respawnSpacing;

                team[i].Respawn(position);
                characterManager.SetCharacterDead(i, false);
            }

            characterManager.ClearRespawn();
        }

        IgnoreTeamCollisions();
        UpdateFormationSpeed();
        SetFormationMode(BotFormationMode.Idle);
        UpdateFormation();
        SetTeamFormationSpeed();

        if (characterManager.AreAllCharactersDead())
            StartDeath();
    }

    private IEnumerator ForceCharacterSwitchPosition(Vector3 position)
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (CharacterManager.Instance == null || !CharacterManager.Instance.CharacterSwitchPositionSaved)
            yield break;

        int aliveCount = 0;

        foreach (Player player in team)
        {
            if (!IsDeadOrMarkedDead(player))
                aliveCount++;
        }

        int aliveIndex = 0;

        foreach (Player player in team)
        {
            if (IsDeadOrMarkedDead(player))
                continue;

            Vector3 playerPosition = position;
            playerPosition.x += (aliveIndex - (aliveCount - 1) / 2f) * spacing;
            player.transform.position = playerPosition;
            aliveIndex++;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        CharacterManager.Instance.ClearCharacterSwitchPosition();
    }

    private bool IsDeadOrMarkedDead(Player player)
    {
        if (player == null || player.IsDead())
            return true;

        CharacterManager manager = CharacterManager.Instance;
        int index = team.IndexOf(player);

        return manager != null && index >= 0 && manager.IsCharacterDead(index);
    }

    private void StopDeadPlayerFollowing(Player player)
    {
        if (player == null)
            return;

        BotController botController = player.GetComponent<BotController>();

        if (botController != null)
            botController.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
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
                        if (colliderA != null && colliderB != null)
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
            if (IsDeadOrMarkedDead(player))
                continue;

            Stats stats = player.GetStats();

            if (stats != null && stats.speed < slowestSpeed)
                slowestSpeed = stats.speed;
        }

        if (slowestSpeed != Mathf.Infinity)
            formationSpeed = slowestSpeed;
    }

    private void SetTeamFormationSpeed()
    {
        foreach (Player player in team)
        {
            if (IsDeadOrMarkedDead(player))
                continue;

            player.SetFormationSpeed(formationSpeed);
        }
    }

    private void ClearTeamFormationSpeed()
    {
        foreach (Player player in team)
        {
            if (player != null)
                player.ClearFormationSpeed();
        }
    }

    private void UpdateFormation()
    {
        if (formationMode != BotFormationMode.Idle)
            return;

        Player leader = CurrentPlayer;

        if (IsDeadOrMarkedDead(leader))
            return;

        List<Player> formation = new List<Player>();

        foreach (Player player in team)
        {
            if (!IsDeadOrMarkedDead(player))
                formation.Add(player);
        }

        formation = formation.OrderBy(GetFormationPosition).ToList();

        int leaderIndex = formation.IndexOf(leader);

        if (leaderIndex < 0)
            return;

        for (int i = 0; i < formation.Count; i++)
        {
            Player player = formation[i];

            if (player == leader)
                continue;

            BotController botController = player.GetComponent<BotController>();

            if (botController != null)
                botController.SetFormationPosition(i - leaderIndex);
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

    public void SaveTeam()
    {
        foreach (Player player in team)
        {
            if (player != null)
                player.SaveCharacterData();
        }
    }

    public bool ReviveAlly(Player fallenPlayer, Player payer, Vector3 spawnPosition)
    {
        if (fallenPlayer == null || payer == null)
            return false;

        if (!team.Contains(fallenPlayer) || !team.Contains(payer))
            return false;

        if (!fallenPlayer.IsDead() || payer.lvl <= 1)
            return false;

        if (payer != fallenPlayer && payer.IsDead())
            return false;

        if (!payer.LoseLevelForResurrection())
            return false;

        int fallenIndex = team.IndexOf(fallenPlayer);

        BotController fallenBotController = fallenPlayer.GetComponent<BotController>();

        if (fallenBotController != null)
            fallenBotController.enabled = true;

        fallenPlayer.Respawn(spawnPosition);
        fallenPlayer.SetBotMode(fallenPlayer != CurrentPlayer);

        if (CharacterManager.Instance != null && fallenIndex >= 0)
            CharacterManager.Instance.SetCharacterDead(fallenIndex, false);

        IgnoreTeamCollisions();

        if (formationMode == BotFormationMode.Idle)
        {
            UpdateFormationSpeed();
            UpdateFormation();
            SetTeamFormationSpeed();
        }

        fallenPlayer.SaveCharacterData();
        payer.SaveCharacterData();

        return true;
    }
}