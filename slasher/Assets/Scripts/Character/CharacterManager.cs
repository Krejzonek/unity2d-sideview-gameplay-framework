using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [SerializeField] private int maxCharacters = 4;

    private List<CharacterData> characters = new();
    private int selectedCharacterIndex = -1;

    public List<CharacterData> Characters => characters;
    public CharacterData SelectedCharacter => selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count ? characters[selectedCharacterIndex] : null;
    public int SelectedCharacterIndex => selectedCharacterIndex;

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
        characters.Add(character);

        if (selectedCharacterIndex == -1)
            selectedCharacterIndex = 0;

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

    public void ApplyCharacterToPlayer(Player player, Stats stats)
    {
        if (SelectedCharacter == null)
            return;

        ApplyCharacterToPlayer(SelectedCharacter, player, stats);
    }

    public void ApplyCharacterToPlayer(CharacterData character, Player player, Stats stats)
    {
        if (character == null || player == null || stats == null)
            return;

        player.lvl = character.level;
        player.currentEXP = character.currentEXP;

        stats.freePoint = character.freePoint;
        stats.SetStatPoints(character.healthPoint, character.staminaPoint, character.speedPoint, character.attackPoint, character.luckPoint);
        stats.SetClass(character.characterClass);
    }
}