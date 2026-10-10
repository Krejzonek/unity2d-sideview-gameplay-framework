using UnityEngine;

public class MapAssetRegistry : MonoBehaviour
{
    public static MapAssetRegistry Instance { get; private set; }

    [SerializeField] private ItemData[] items;
    [SerializeField] private IngredientData[] ingredients;
    [SerializeField] private ConsumableData[] consumables;
    [SerializeField] private Class[] classes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public ItemData GetItem(string name)
    {
        if (items == null) return null;
        foreach (ItemData item in items)
            if (item != null && item.itemName == name)
                return item;
        return null;
    }

    public IngredientData GetIngredient(string name)
    {
        if (ingredients == null) return null;
        foreach (IngredientData item in ingredients)
            if (item != null && item.itemName == name)
                return item;
        return null;
    }

    public ConsumableData GetConsumable(string name)
    {
        if (consumables == null) return null;
        foreach (ConsumableData item in consumables)
            if (item != null && item.itemName == name)
                return item;
        return null;
    }

    public Class GetClass(string name)
    {
        if (classes == null) return null;
        foreach (Class item in classes)
            if (item != null && item.className == name)
                return item;
        return null;
    }
}