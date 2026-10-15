using UnityEngine;

public class Cauldron : MonoBehaviour
{
    [Header("Recipes")]
    [SerializeField] private CraftingRecipe[] recipes;

    [Header("Consumable System")]
    [SerializeField] private ConsumableManager consumableManager;

    private void Start()
    {
        if (consumableManager == null)
            consumableManager = FindFirstObjectByType<ConsumableManager>();
    }

    public CraftingRecipe[] GetRecipes()
    {
        return recipes;
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        if (recipe == null)
            return false;

        if (consumableManager == null)
            consumableManager = FindFirstObjectByType<ConsumableManager>();

        if (consumableManager == null)
            return false;

        IngredientInventory ingredientInventory = consumableManager.GetIngredientInventory();

        if (ingredientInventory == null)
            return false;

        foreach (CraftingRecipe.IngredientRequirement requirement in recipe.ingredients)
        {
            if (!ingredientInventory.HasIngredient(requirement.ingredient, requirement.amount))
                return false;
        }

        return true;
    }

    public bool Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
            return false;

        if (consumableManager == null)
            return false;

        IngredientInventory ingredientInventory = consumableManager.GetIngredientInventory();
        ConsumableInventory consumableInventory = consumableManager.GetConsumableInventory();

        if (ingredientInventory == null || consumableInventory == null)
            return false;

        if (TeamManager.Instance == null || TeamManager.Instance.CurrentPlayer == null)
            return false;

        Player player = TeamManager.Instance.CurrentPlayer;
        Stats stats = player.GetStats();

        if (stats == null)
            return false;

        foreach (CraftingRecipe.IngredientRequirement requirement in recipe.ingredients)
        {
            if (!ingredientInventory.RemoveIngredient(requirement.ingredient, requirement.amount))
                return false;
        }

        float luck = stats.luck;

        ConsumableData consumable = Instantiate(recipe.result);
        consumable.rarity = LuckSystem.GetConsumableRarity(luck);

        ApplyCookingLuck(consumable, luck);

        consumableInventory.AddConsumable(consumable, recipe.resultAmount);

        ConsumableHotbar hotbar = FindFirstObjectByType<ConsumableHotbar>();

        if (hotbar != null)
            hotbar.RefreshContent();

        return true;
    }

    private void ApplyCookingLuck(ConsumableData consumable, float luck)
    {
        float positiveMultiplier = LuckSystem.GetCookingPositiveMultiplier(luck);
        float negativeMultiplier = LuckSystem.GetCookingNegativeMultiplier(luck);

        consumable.healAmount *= positiveMultiplier;
        consumable.regenerationAmount *= positiveMultiplier;

        if (consumable.effects == null)
            return;

        foreach (StatEffect effect in consumable.effects)
        {
            if (effect.amount > 0f)
                effect.amount *= positiveMultiplier;
            else if (effect.amount < 0f)
                effect.amount *= negativeMultiplier;
        }
    }

    private void OnMouseDown()
    { 
        OpenCraftingPanel();
    }

    public void OpenCraftingPanel()
    {
        if (CraftingUI.Instance == null)
        {
            Debug.LogError("CraftingUI not found!");
            return;
        }

        CraftingUI.Instance.Open(this);
    }
}