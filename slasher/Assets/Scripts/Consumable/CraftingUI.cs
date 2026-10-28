using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance;

    [Header("Main UI")]
    [SerializeField] private GameObject craftingPanel;

    [Header("Recipe List")]
    [SerializeField] private Transform recipeContent;
    [SerializeField] private GameObject recipePrefab;

    [Header("Buttons")]
    [SerializeField] private Button foodButton;
    [SerializeField] private Button potionButton;

    private Cauldron currentCauldron;

    private void Awake()
    {
        Instance = this;

        craftingPanel.SetActive(false);

        foodButton.onClick.AddListener(ShowFood);
        potionButton.onClick.AddListener(ShowPotions);
    }

    public void Open(Cauldron cauldron)
    {
        currentCauldron = cauldron;

        craftingPanel.SetActive(true);

        ShowFood();
    }

    public void Close()
    {
        craftingPanel.SetActive(false);

        currentCauldron = null;
    }

    public void ShowFood()
    {
        ShowRecipes(ConsumableType.Food);
    }

    public void ShowPotions()
    {
        ShowRecipes(ConsumableType.Potion);
    }

    private void ShowRecipes(ConsumableType type)
    {
        ClearList();

        if (currentCauldron == null)
            return;

        CraftingRecipe[] recipes = currentCauldron.GetRecipes();

        foreach (CraftingRecipe recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (recipe.result == null)
                continue;

            if (recipe.result.consumableType != type)
                continue;

            CreateRecipe(recipe);
        }
    }

    private void CreateRecipe(CraftingRecipe recipe)
    {
        GameObject recipeObject = Instantiate(recipePrefab, recipeContent);
        TMP_Text recipeName = recipeObject.transform.Find("RecipeName")?.GetComponent<TMP_Text>();

        if (recipeName != null)
        {
            recipeName.text = recipe.recipeName;
        }

        Image icon = recipeObject.transform.Find("Icon")?.GetComponent<Image>();

        if (icon != null)
        {
            icon.sprite = recipe.icon;
        }
        Transform ingredients = recipeObject.transform.Find("Ingredients");

        if (ingredients != null)
        {
            foreach (CraftingRecipe.IngredientRequirement requirement in recipe.ingredients)
            {
                GameObject ingredientObject = new GameObject("Ingredient");

                ingredientObject.transform.SetParent(ingredients, false);

                TextMeshProUGUI text = ingredientObject.AddComponent<TextMeshProUGUI>();

                text.enableAutoSizing = true;
                text.fontSizeMin = 12;
                text.fontSizeMax = 24;
                text.color = Color.black;

                int currentAmount = GetIngredientAmount(requirement.ingredient);

                string amountColor = currentAmount >= requirement.amount ? "#000000" : "#FF0000";

                text.text = requirement.ingredient.itemName + "   <color=" + amountColor + ">" + currentAmount + " / " + requirement.amount + "</color>";
            }
        }

        Button craftButton =
            recipeObject.transform.Find("CraftButton")
                ?.GetComponent<Button>();

        if (craftButton != null)
        {
            craftButton.interactable =
                currentCauldron.CanCraft(recipe);

            craftButton.onClick.AddListener(() =>
            {
                Craft(recipe, craftButton);
            });
        }
    }

    private void Craft(CraftingRecipe recipe, Button craftButton)
    {
        if (currentCauldron == null)
            return;

        bool success =
            currentCauldron.Craft(recipe);

        if (!success)
            return;
        if (recipe.result.consumableType == ConsumableType.Food)
        {
            ShowFood();
        }
        else
        {
            ShowPotions();
        }
    }

    private int GetIngredientAmount(
        IngredientData ingredient)
    {
        IngredientInventory inventory =
            FindObjectOfType<IngredientInventory>();

        if (inventory == null)
            return 0;

        return inventory.GetAmount(ingredient);
    }

    private void ClearList()
    {
        foreach (Transform child in recipeContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void CloseAfterRoomChange()
    {
        currentCauldron = null;

        if (craftingPanel != null)
            craftingPanel.SetActive(false);

        if (recipeContent != null)
        {
            foreach (Transform child in recipeContent)
                Destroy(child.gameObject);
        }
    }
}