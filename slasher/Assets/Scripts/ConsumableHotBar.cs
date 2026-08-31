using UnityEngine;
using UnityEngine.UI;

public class ConsumableHotbar : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;

    [Header("Items")]
    [SerializeField] private ConsumableData item1;
    [SerializeField] private ConsumableData item2;

    [Header("Player")]
    [SerializeField] private ConsumableManager consumableManager;

    private void Start()
    {
        button1.onClick.AddListener(UseItem1);
        button2.onClick.AddListener(UseItem2);
    }

    private void UseItem1()
    {
        if (item1 == null)
            return;

        consumableManager.UseConsumable(item1);
    }

    private void UseItem2()
    {
        if (item2 == null)
            return;

        consumableManager.UseConsumable(item2);
    }
}