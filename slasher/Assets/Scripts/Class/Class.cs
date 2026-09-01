using UnityEngine;

[CreateAssetMenu(fileName = "ClassBonus", menuName = "Game/Class Bonus")]
public class Class : ScriptableObject
{
    public CharacterClass characterClass;

    [Header("Class Bonus")]
    public float healthBonus;
    public float staminaBonus;
    public float attackBonus;
    public float speedBonus;
}