# Getting Started

## Requirements

- Unity **2022.3 LTS** (the project was developed with Unity 2022.3.61f1).
- The packages declared by the project's `Packages/manifest.json`.
- A desktop build target for the keyboard/mouse and gamepad workflows. Mobile controls are also included, but a mobile build should be tested on the intended device.

## Open and run the sample

1. Clone or download the repository.
2. Open the project folder from Unity Hub using a compatible Unity 2022.3 editor.
3. Let Unity resolve packages and import assets. Resolve any package or API errors before continuing.
4. Open `Assets/Scenes/MainMenu.unity` to start the game.
5. Start the game from `MainMenu` and follow the normal navigation through `CharacterSelect` into `Game`.
6. Do not launch `CharacterSelect.unity` or `Game.unity` directly. The character generation and initialization process depends on data prepared during the normal scene flow.
7. Check the Console for missing references and runtime exceptions.

## First configuration pass

Before making content, inspect these areas:

- `Assets/Scenes/` — sample scene setup.
- `Assets/Prefab/` — player, NPC, UI, projectile, loot, and room prefabs.
- `Assets/ScriptableObject/` — sample classes, skills, equipment, recipes, loot tables, boss data, and input settings.
- `Assets/NPCData/` — NPC definitions, dialogue assets, activity definitions, and trade data.
- `Assets/Scripts/` — runtime systems grouped by feature.

Do not delete sample ScriptableObjects until you have checked which prefabs/scenes reference them. Unity references are serialized by asset GUID, not by the visible filename alone.

## Recommended test order

1. Start the game from `MainMenu` and verify scene navigation.
2. Complete character selection and verify that the selected character is generated correctly in gameplay.
3. Test movement, jumping, facing, attack, defense, and stamina.
4. Test one melee weapon and one ranged weapon.
5. Test enemy damage, death, room completion, and loot pickup.
6. Test equipment, consumables, crafting, and NPC interactions.
7. Test checkpoint/respawn and save/load after making a meaningful change.
8. Test keyboard/mouse, gamepad, and mobile controls separately.

## Common setup problems

- **Missing reference:** select the affected scene object or prefab and assign the required component, prefab, transform, UI element, or ScriptableObject in the Inspector.
- **Nothing happens on interaction:** check the collider, interaction component, input action, event hookup, and active UI state.
- **A weapon has no visible model:** check its `EquipmentData` reference, `WeaponVisualProfile`, weapon prefab, and part settings.
- **A room does not unlock:** check its enemy spawner and the room's enemy/alive tracking setup.
- **Save data cannot resolve an asset:** check the asset registry and make sure the relevant asset is registered and included in the build.
