using UnityEngine;

[CreateAssetMenu(fileName = "New Blacksmith Activity", menuName = "NPC/Activities/Blacksmith")]
public class NPCBlacksmithActivity : NPCActivityData
{
    [TextArea(2, 5)]
    public string openingText = "Choose two items of the same type and rarity to upgrade.";

    public override void Execute(NPCActivityContext context)
    {
        if (context == null || context.UI == null || context.NPC == null)
            return;

        context.UI.OpenBlacksmith(this);
    }
}