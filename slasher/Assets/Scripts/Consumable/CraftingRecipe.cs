using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public class IngredientRequirement
    {
        public IngredientData ingredient;
        public int amount;
    }

    [Header("Recipe")]
    public string recipeName;
    public Sprite icon;

    [Header("Ingredients")]
    public List<IngredientRequirement> ingredients = new();

    [Header("Result")]
    public ConsumableData result;
    public int resultAmount = 1;
}