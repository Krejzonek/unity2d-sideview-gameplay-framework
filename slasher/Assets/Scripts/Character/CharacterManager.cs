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
        if (SelectedCharacter == null || player == null || stats == null)
            return;

        SelectedCharacter.level = player.lvl;
        SelectedCharacter.currentEXP = player.currentEXP;
        SelectedCharacter.characterClass = stats.characterClass;
        SelectedCharacter.freePoint = stats.freePoint;

        stats.GetStatPoints(out float healthPoint, out float staminaPoint, out float speedPoint, out float attackPoint, out float luckPoint);

        SelectedCharacter.healthPoint = healthPoint;
        SelectedCharacter.staminaPoint = staminaPoint;
        SelectedCharacter.speedPoint = speedPoint;
        SelectedCharacter.attackPoint = attackPoint;
        SelectedCharacter.luckPoint = luckPoint;
    }

    public void ApplyCharacterToPlayer(Player player, Stats stats)
    {
        if (SelectedCharacter == null || player == null || stats == null)
            return;

        player.lvl = SelectedCharacter.level;
        player.currentEXP = SelectedCharacter.currentEXP;

        stats.freePoint = SelectedCharacter.freePoint;
        stats.SetStatPoints(SelectedCharacter.healthPoint, SelectedCharacter.staminaPoint, SelectedCharacter.speedPoint, SelectedCharacter.attackPoint, SelectedCharacter.luckPoint);
        stats.SetClass(SelectedCharacter.characterClass);
    }
}