using System.Collections.Generic;
using System.IO;
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
    private Vector3 respawnPosition;

    private bool characterSwitchPositionSaved;
    private Vector3 characterSwitchPosition;

    public List<CharacterData> Characters => characters;
    public CharacterData SelectedCharacter => selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count ? characters[selectedCharacterIndex] : null;
    public int SelectedCharacterIndex => selectedCharacterIndex;
    public bool CharacterSwitchMode => characterSwitchMode;
    public bool PendingRespawn => pendingRespawn;
    public Vector3 CheckpointPosition => checkpointPosition;
    public bool HasCheckpoint => hasCheckpoint;
    public Vector3 RespawnPosition => respawnPosition;
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

    public void UpdateCharacterFromPlayer(CharacterData character, Player player, Stats stats)
    {
        if (character == null || player == null || stats == null)
            return;

        character.level = player.lvl;
        character.currentEXP = player.currentEXP;

        if (stats.characterClass != null)
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
        respawnPosition = position;
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

    public void LoadCharactersFromSave()
    {
        if (characters.Count > 0)
        {
            Debug.Log("Character load skipped: keeping " + characters.Count + " characters already in memory.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, "mapSave.json");

        if (!File.Exists(path))
        {
            deadCharacters.Clear();
            selectedCharacterIndex = -1;
            return;
        }

        try
        {
            MapSaveSystem.MapData data = JsonUtility.FromJson<MapSaveSystem.MapData>(File.ReadAllText(path));

            if (data == null || data.characters == null || data.characters.Count == 0)
                return;

            RestoreCharacters(data.characters, data.deadCharacters, MapAssetRegistry.Instance);

            if (data.hasCheckpoint)
                SetCheckpoint(data.checkpointPosition);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Failed to load character slots: " + exception.Message);
        }
    }

    public void RestoreCharacters(List<MapSaveSystem.CharacterSaveData> savedCharacters, List<bool> savedDeadCharacters, MapAssetRegistry registry)
    {
        if (savedCharacters == null || savedCharacters.Count == 0)
            return;

        for (int i = 0; i < savedCharacters.Count; i++)
        {
            MapSaveSystem.CharacterSaveData saved = savedCharacters[i];

            if (saved == null)
                continue;

            CharacterData character;

            if (i < characters.Count)
            {
                character = characters[i];
            }
            else
            {
                if (characters.Count >= maxCharacters)
                {
                    Debug.LogWarning("Cannot restore character: maximum character count reached.");
                    break;
                }

                character = new CharacterData(saved.characterName);
                characters.Add(character);
            }

            character.characterName = saved.characterName;
            character.level = saved.level;
            character.currentEXP = saved.currentEXP;
            character.freePoint = saved.freePoint;
            character.healthPoint = saved.healthPoint;
            character.staminaPoint = saved.staminaPoint;
            character.speedPoint = saved.speedPoint;
            character.attackPoint = saved.attackPoint;
            character.luckPoint = saved.luckPoint;

            while (deadCharacters.Count <= i)
                deadCharacters.Add(false);

            deadCharacters[i] = savedDeadCharacters != null && i < savedDeadCharacters.Count && savedDeadCharacters[i];

            if (registry != null)
            {
                if (!string.IsNullOrEmpty(saved.className))
                {
                    Class restoredClass = registry.GetClass(saved.className);

                    if (restoredClass != null)
                        character.characterClass = restoredClass;
                    else
                        Debug.LogWarning("Missing class: " + saved.className);
                }

                if (character.equippedItems == null)
                    character.equippedItems = new List<ItemData>();
                else
                    character.equippedItems.Clear();

                if (saved.equippedItems != null)
                {
                    foreach (string itemName in saved.equippedItems)
                    {
                        ItemData item = registry.GetItem(itemName);

                        if (item != null)
                            character.equippedItems.Add(item);
                        else
                            Debug.LogWarning("Missing equipped item: " + itemName);
                    }
                }
            }
        }

        while (deadCharacters.Count < characters.Count)
            deadCharacters.Add(false);

        while (deadCharacters.Count > characters.Count)
            deadCharacters.RemoveAt(deadCharacters.Count - 1);

        selectedCharacterIndex = characters.Count > 0 ? Mathf.Clamp(selectedCharacterIndex, 0, characters.Count - 1) : -1;
    }

    public void ClearCheckpoint()
    {
        checkpointPosition = Vector3.zero;
        hasCheckpoint = false;
    }
}