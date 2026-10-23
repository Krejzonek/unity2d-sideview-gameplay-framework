using UnityEngine;

[CreateAssetMenu(fileName = "New Trader Activity", menuName = "NPC/Activities/Trader")]
public class NPCTraderActivity : NPCActivityData
{
    [Header("Trading Settings")]
    public bool allowBuying = true;
    public bool allowSelling = true;

    [TextArea(2, 5)]
    public string openingText = "Welcome! Take a look at my wares.";

    public override void Execute(NPCActivityContext context)
    {
        context.ShowText(openingText);
    }
}