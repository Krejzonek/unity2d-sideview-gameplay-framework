# Architecture Overview

The project is organized as cooperating gameplay systems rather than one monolithic controller.

## Main data and runtime flow

1. **Content data** is authored as ScriptableObjects: classes, skills, equipment, consumables, recipes, loot tables, boss data, special-room data, NPC data, dialogue and trades.
2. **Scene objects and prefabs** reference those assets and provide Unity components, colliders, transforms, renderers and UI elements.
3. **Runtime managers/controllers** read the configured data and coordinate behavior: `CharacterManager`, `TeamManager`, `GameInputManager`, `EquipmentManager`, `ClassSkillManager`, `LootManager`, `RoomGenerator`, `MapSaveSystem`, and the UI managers.
4. **Gameplay actors** such as `Player`, `EnemyAI`, `BossAI` and `NPC` perform local behavior and communicate with the relevant systems.

## Typical dependencies

- `Player` uses `Stats`, input, equipment and class-skill functionality; attacks can create `Projectile` instances or use melee hitboxes.
- `Stats` represents the current combat attributes. Character/class data and equipped items contribute to the values used during play.
- `EquipmentData` describes an item; `WeaponVisualProfile` describes how a weapon model is assembled/held; `WeaponVisualController` and `WeaponVisualEquipment` handle its visual presentation.
- `EnemyAI` receives damage and status effects; `BossAI` adds boss-specific attack patterns and phase behavior using boss data assets.
- `RoomGenerator`, room prefabs, room points and spawners form the room progression flow. Room completion depends on enemy state.
- NPCs use `NPCData` to expose dialogue and activities. Activity-specific ScriptableObjects provide blacksmith/trader configuration.
- `MapSaveSystem` serializes runtime state. `MapAssetRegistry` is used to resolve saved content references back to project assets.

## Singleton-style managers

Several systems expose static `Instance` access. Ensure that the required manager exists in the active scene and that scene transitions do not leave duplicate manager objects. Do not add multiple copies of a manager unless its implementation explicitly supports that arrangement.

## Extending safely

Prefer adding a new ScriptableObject asset or a new specialized component over editing shared core logic. Keep serialized references explicit, preserve existing prefab hierarchies, and test scene transitions and save/load after changing persistent data structures.
