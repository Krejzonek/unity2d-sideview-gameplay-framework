using System.Collections;
using UnityEngine;

public class ConsumableManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Stats stats;

    private Coroutine regenerationCoroutine;

    public void UseConsumable(ConsumableData item)
    {
        if (item == null)
            return;

        if (item.consumableType == ConsumableType.Potion)
        {
            UsePotion(item);
        }
        else if (item.consumableType == ConsumableType.Food)
        {
            UseFood(item);
        }
    }

    private void UsePotion(ConsumableData item)
    {
        if (item.healAmount > 0)
        {
            player.Heal(item.healAmount);
        }

        if (item.effects.Count > 0)
        {
            stats.AddBuff(item.effects, item.buffDuration);
        }
    }

    private void UseFood(ConsumableData item)
    {
        if (regenerationCoroutine != null)
        {
            StopCoroutine(regenerationCoroutine);
        }

        regenerationCoroutine = StartCoroutine(RegenerateHealth(item.regenerationAmount,item.regenerationDuration));
    }

    private IEnumerator RegenerateHealth(
        float amount,
        float duration)
    {
        float timer = 0f;
        float healPerSecond = amount / duration;

        while (timer < duration)
        {
            player.Heal(healPerSecond * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }

        regenerationCoroutine = null;
    }
}