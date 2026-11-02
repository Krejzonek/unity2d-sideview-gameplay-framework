# NPC System

The NPC system is data-driven. An NPC GameObject uses the `NPC` component and references an `NPCData` ScriptableObject. The data asset contains the NPC name, portrait, dialogue list and available activity assets.

## Create an NPC

1. Duplicate `Assets/Prefab/NPC.prefab` so the expected collider, interaction and UI references are preserved.
2. Create an `NPCData` asset.
3. Set the NPC name and portrait.
4. Assign one or more `NPCDialogue` assets.
5. Assign one or more activity assets derived from `NPCActivityData`.
6. Assign the new data asset to the NPC component on the prefab/scene instance.
7. Place the NPC in a scene and test its interaction range and dialogue/activity UI.

## Dialogues

`NPCDialogue` is a ScriptableObject identified by a dialogue ID. `NPCDialogueManager` coordinates the dialogue UI. Create dialogue assets using the project's available asset menu and assign them to the NPC data. Keep IDs unique where the dialogue manager uses them as identifiers. The supplied `Hint1` and `Hint2` assets are examples, not mandatory content.

## Activities

`NPCActivityData` is the base data type for NPC activities. `NPCActivityContext` provides the active NPC and dialogue UI to an activity. The included specialized activities are:

- `NPCBlacksmithActivity` — blacksmith interaction; the sample opening text describes combining two items of the same type and rarity to upgrade.
- `NPCTraderActivity` — buying/selling and configured trade entries.

An activity asset is assigned through the NPC's `NPCData.activities` array. The NPC prefab must have the relevant UI/prefab references for the selected activity.

## Trader setup

1. Create an `NPCTraderActivity` asset.
2. Set whether buying and selling are allowed.
3. Add `NPCTradeData` entries to its trades array.
4. For each trade, choose the required item type and assign the matching required ingredient/consumable/equipment asset.
5. Choose the reward type and assign the matching reward asset.
6. Assign the activity to an NPCData asset.
7. Test valid trades, missing required items, inventory changes and repeated use.

## Blacksmith setup

1. Create/configure a blacksmith activity asset.
2. Assign it to the NPC's activities list.
3. Ensure the blacksmith UI and item-button prefab references are assigned on the relevant UI/prefab setup.
4. Test the allowed item selection and upgrade result. The sample behavior expects two items of the same type and rarity; use the actual runtime validation as the source of truth.

## Extending activities

Create a class derived from `NPCActivityData`, implement the activity behavior using `NPCActivityContext`, and connect its UI/prefabs. Keep the activity independent of the NPC's dialogue data so the same activity can be reused by multiple NPCs.
