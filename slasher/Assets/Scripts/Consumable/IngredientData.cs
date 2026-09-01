using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Items/Ingredient")]
public class IngredientData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
}