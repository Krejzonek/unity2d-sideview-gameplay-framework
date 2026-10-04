using System.Collections.Generic;
using UnityEngine;

public class IngredientInventory : MonoBehaviour
{
    [System.Serializable]
    public class IngredientStack
    {
        public IngredientData ingredient;
        public int amount;
    }

    [SerializeField] private List<IngredientStack> ingredients = new();

    public void AddIngredient(IngredientData ingredient, int amount)
    {
        if (ingredient == null || amount <= 0)
            return;

        IngredientStack stack = ingredients.Find(x => x.ingredient == ingredient);

        if (stack != null)
        {
            stack.amount += amount;
        }
        else
        {
            ingredients.Add(new IngredientStack
            {
                ingredient = ingredient,
                amount = amount
            });
        }
    }

    public bool HasIngredient(IngredientData ingredient, int amount)
    {
        IngredientStack stack = ingredients.Find(x => x.ingredient == ingredient);

        if (stack == null)
            return false;

        return stack.amount >= amount;
    }

    public bool RemoveIngredient(IngredientData ingredient, int amount)
    {
        if (!HasIngredient(ingredient, amount))
            return false;

        IngredientStack stack = ingredients.Find(x => x.ingredient == ingredient);

        stack.amount -= amount;

        if (stack.amount <= 0)
            ingredients.Remove(stack);

        return true;
    }

    public int GetAmount(IngredientData ingredient)
    {
        IngredientStack stack = ingredients.Find(x => x.ingredient == ingredient);

        if (stack == null)
            return 0;

        return stack.amount;
    }

    public List<IngredientStack> GetIngredients()
    {
        return ingredients;
    }
}