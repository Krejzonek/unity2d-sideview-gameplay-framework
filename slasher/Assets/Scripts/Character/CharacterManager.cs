using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [SerializeField] private int maxCharacters = 4;

    private List<CharacterData> characters = new();
    private List<bool> deadCharacters = new();

    private int selectedCharacterIndex = -1;

    private bool characterSwitchMode;
    private bool pendingRespawn;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint;

    private bool characterSwitchPositionSaved;
    private Vector3 characterSwitchPosition;

    public List<CharacterData> Characters => characters;
    public CharacterData SelectedCharacter => selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count ? characters[selectedCharacterIndex] : null;
    public int SelectedCharacterIndex => selectedCharacterIndex;

    public bool CharacterSwitchMode => characterSwitchMode;

    public bool PendingRespawn => pendingRespawn;

    public Vector3 CheckpointPosition => checkpointPosition;
    public bool HasCheckpoint => hasCheckpoint;

    public bool CharacterSwitchPositionSaved => characterSwitchPositionSaved;
    public Vector3 CharacterSwitchPosition => characterSwitchPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool CreateCharacter(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
            return false;

        if (characters.Count >= maxCharacters)
            return false;

        CharacterData character = new CharacterData(characterName.Trim());
        character.characterClass = null;

        characters.Add(character);
        deadCharacters.Add(false);

        selectedCharacterIndex = characters.Count - 1;

        return true;
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
            return;

        selectedCharacterIndex = index;
    }

    public void SetCharacterClass(Class characterClass)
    {
        if (SelectedCharacter == null)
            return;

        SelectedCharacter.characterClass = characterClass;
    }

    public void UpdateCharacterFromPlayer(Player player, Stats stats)
    {
        if (SelectedCharacter == null)
            return;

        UpdateCharacterFromPlayer(SelectedCharacter, player, stats);
    }

    public void UpdateCharacterFromPlayer(CharacterData character, Player player, Stats stats)
    {
        if (character == null || player == null || stats == null)
            return;

        character.characterName = character.characterName;
        character.level = player.lvl;
        character.currentEXP = player.currentEXP;
        character.characterClass = stats.characterClass;
        character.freePoint = stats.freePoint;

        stats.GetStatPoints(out float healthPoint, out float staminaPoint, out float speedPoint, out float attackPoint, out float luckPoint);

        character.healthPoint = healthPoint;
        character.staminaPoint = staminaPoint;
        character.speedPoint = speedPoint;
        character.attackPoint = attackPoint;
        character.luckPoint = luckPoint;
    }

    public void ApplyCharacterToPlayer(CharacterData character, Player player, Stats stats)
    {
        if (character == null || player == null || stats == null)
            return;

        player.lvl = character.level;
        player.currentEXP = character.currentEXP;

        stats.freePoint = character.freePoint;
        stats.SetStatPoints(character.healthPoint, character.staminaPoint, character.speedPoint, character.attackPoint, character.luckPoint);
        stats.characterClass = character.characterClass;
        stats.UpdateStats();
    }

    public void SetCharacterDead(int index, bool dead)
    {
        if (index < 0 || index >= characters.Count)
            return;

        while (deadCharacters.Count < characters.Count)
            deadCharacters.Add(false);

        deadCharacters[index] = dead;
    }

    public bool IsCharacterDead(int index)
    {
        if (index < 0 || index >= deadCharacters.Count)
            return false;

        return deadCharacters[index];
    }

    public void StartCharacterSwitch()
    {
        characterSwitchMode = true;
    }

    public void EndCharacterSwitch()
    {
        characterSwitchMode = false;
    }

    public void SaveCharacterSwitchPosition(Vector3 position)
    {
        characterSwitchPosition = position;
        characterSwitchPositionSaved = true;
    }

    public void ClearCharacterSwitchPosition()
    {
        characterSwitchPositionSaved = false;
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
    }

    public void StartRespawn(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
        pendingRespawn = true;
        characterSwitchMode = false;
        characterSwitchPositionSaved = false;

        for (int i = 0; i < characters.Count; i++)
            SetCharacterDead(i, false);
    }

    public void ClearRespawn()
    {
        pendingRespawn = false;
    }

    public void ResetDeathState()
    {
        characterSwitchMode = false;
        pendingRespawn = false;
        characterSwitchPositionSaved = false;

        for (int i = 0; i < characters.Count; i++)
            SetCharacterDead(i, false);
    }

    public bool AreAllCharactersDead()
    {
        if (characters.Count == 0)
            return false;

        for (int i = 0; i < characters.Count; i++)
        {
            if (!IsCharacterDead(i))
                return false;
        }

        return true;
    }
}