# Extension Guide

## General workflow

1. Find the closest existing ScriptableObject, component or prefab that already solves a similar problem.
2. Duplicate the data asset/prefab rather than rebuilding its hierarchy from scratch.
3. Add the smallest new behavior required.
4. Keep configuration in ScriptableObjects when the value is content-specific and should be reusable.
5. Keep runtime state in components/managers rather than modifying shared asset data during play.
6. Update save/load if the new state must survive scene changes or application restarts.
7. Update UI, input and localization/player-facing text where the feature is visible.
8. Test normal use, invalid configuration, death/respawn, scene transition and save/load where relevant.

## Add a new item

Create an `EquipmentData`, `ConsumableData` or `IngredientData` asset as appropriate. Connect it to a loot table, NPC trade, recipe, starting inventory or other acquisition source. For equipment, test stat changes and visuals. For consumables, test use effects and stack counts. For ingredients, test crafting requirements.

## Add a new enemy

Duplicate a compatible enemy prefab, configure `EnemyAI`, colliders/physics/visuals and add it to the room spawn configuration. Verify it participates in room alive-count/completion handling.

## Add a new NPC activity

Derive a new activity data type from `NPCActivityData`, implement its behavior using `NPCActivityContext`, create its data asset and connect the activity UI/prefabs. Assign the activity asset to `NPCData.activities`.

## Add a new skill

Add the data type/enum value and implement its runtime behavior in `ClassSkillManager`. Update the class asset, UI presentation and any save logic if skill state persists. A new enum entry alone is not a complete skill.

## Add a new stat

Update `Stats`, bonus application, equipment/class data if relevant, UI display, save serialization and restore logic. Search the project for every read/write of the existing stats before considering the change complete.

## Code style and compatibility

Keep Unity serialization constraints in mind: renaming serialized fields can break prefab/asset data unless migration is handled. Avoid changing public field names/types casually. Keep editor-only scripts under `Assets/Editor/` and do not put editor APIs in runtime scripts.
