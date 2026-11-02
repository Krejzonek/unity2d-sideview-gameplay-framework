using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableManager : MonoBehaviour
{
    [SerializeField] private ConsumableInventory consumableInventory;
    [SerializeField] private IngredientInventory ingredientInventory;

    private Dictionary<Player, Coroutine> regenerationCoroutines = new();

    public ConsumableInventory GetConsumableInventory()
    {
        return consumableInventory;
    }

    public IngredientInventory GetIngredientInventory()
    {
        return ingredientInventory;
    }

    public void UseConsumable(ConsumableData item, Player player)
    {
        if (item == null || player == null)
            return;

        if (item.consumableType == ConsumableType.Potion)
            UsePotion(item, player);
        else if (item.consumableType == ConsumableType.Food)
            UseFood(item, player);
    }

    private void UsePotion(ConsumableData item, Player player)
    {
        if (item.healAmount > 0)
            player.Heal(item.healAmount);

        if (item.effects.Count > 0)
        {
            Stats stats = player.GetStats();

            if (stats != null)
            {
                List<StatEffect> effects = new();
                float debuffReduction = item.GetDebuffReduction();

                foreach (StatEffect effect in item.effects)
                {
                    StatEffect newEffect = new StatEffect
                    {
                        statType = effect.statType,
                        amount = effect.amount
                    };

                    if (newEffect.amount < 0f)
                        newEffect.amount *= 1f - debuffReduction;

                    effects.Add(newEffect);
                }

                stats.AddBuff(effects, item.buffDuration);
                player.RefreshStats();
            }
        }
    }

    private void UseFood(ConsumableData item, Player player)
    {
        if (regenerationCoroutines.TryGetValue(player, out Coroutine coroutine))
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        regenerationCoroutines[player] = StartCoroutine(RegenerateHealth(item.regenerationAmount, item.regenerationDuration, player));
    }

    private IEnumerator RegenerateHealth(float amount, float duration, Player player)
    {
        float timer = 0f;
        float healPerSecond = amount / duration;

        while (timer < duration)
        {
            if (player == null)
                yield break;

            player.Heal(healPerSecond * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }

        if (regenerationCoroutines.ContainsKey(player))
            regenerationCoroutines.Remove(player);
    }
}