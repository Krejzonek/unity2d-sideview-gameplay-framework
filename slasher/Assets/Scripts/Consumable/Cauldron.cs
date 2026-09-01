using UnityEngine;

public class Cauldron : MonoBehaviour
{
    [Header("Recipes")]
    [SerializeField] private CraftingRecipe[] recipes;

    [Header("Player Systems")]
    private IngredientInventory ingredientInventory;
    private ConsumableInventory consumableInventory;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null) return;
        
        ingredientInventory = playerObject.GetComponent<IngredientInventory>();

        consumableInventory = playerObject.GetComponent<ConsumableInventory>();
       
    }

    public CraftingRecipe[] GetRecipes()
    {
        return recipes;
    }
    public bool CanCraft(CraftingRecipe recipe)
    {
        if (recipe == null)
            return false;

        if (ingredientInventory == null)
            return false;

        foreach (CraftingRecipe.IngredientRequirement requirement in recipe.ingredients)
        {
            if (!ingredientInventory.HasIngredient(requirement.ingredient,requirement.amount))
            {
                return false;
            }
        }

        return true;
    }
    public bool Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
            return false;

        if (consumableInventory == null)
            return false;

        foreach (CraftingRecipe.IngredientRequirement requirement in recipe.ingredients)
        {
            ingredientInventory.RemoveIngredient(requirement.ingredient,requirement.amount);
        }

        consumableInventory.AddConsumable(recipe.result,recipe.resultAmount);

        return true;
    }
    private void OnMouseDown()
    {
        OpenCraftingPanel();
    }

    public void OpenCraftingPanel()
    {
        if (CraftingUI.Instance == null)
        {
            Debug.LogError("Nie znaleziono CraftingUI!");
            return;
        }

        CraftingUI.Instance.Open(this);
    }
}