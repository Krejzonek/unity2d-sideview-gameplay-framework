# Project Structure

## Runtime code

| Folder / file | Responsibility                                                                                           |
| ------------- | -------------------------------------------------------------------------------------------------------- |
| `Character/`  | Character data, character selection/management, team management, bot control and formation.              |
| `Class/`      | Class definitions, class tiers, active/passive skill data and runtime skill handling.                    |
| `Consumable/` | Consumable definitions, inventory, hotbar, ingredient inventory, recipes and crafting UI.                |
| `DropSystem/` | Loot tables, chest loot, item drops, rarity, luck and pickup notifications.                              |
| `Enemy/`      | Standard enemy AI, boss AI, boss attack data and attack warnings.                                        |
| `Equipment/`  | Equipment data, equipment slots/manager, melee hitbox and weapon visual configuration.                   |
| `Input/`      | Input abstraction, rebinding, gamepad UI navigation, mobile controls and interaction interface.          |
| `Menu/`       | Main menu, pause/settings UI, scene loading and selection UI helpers.                                    |
| `NPC/`        | NPC data, dialogue, activities, blacksmith and trader activities.                                        |
| `Player/`     | Player controller, stats, projectile, animation controller and stat bonus data.                          |
| `Room/`       | Room definitions, generation, enemy spawning, room triggers and special-room data.                       |
| `SaveSystem/` | Checkpoints, asset registry and map/game-state serialization.                                            |
| `Timer/`      | Fight preparation timer.                                                                                 |
| Root scripts  | Shared UI helpers, HUD, camera follow, damage numbers, elemental/status-effect systems and stats panels. |
| `Editor/`     | Custom Inspector/editor utilities; these are editor-only tools, not runtime gameplay components.         |

## Content folders

- `Prefab/` contains reusable scene objects, including the player, NPC, weapons, projectiles, UI elements, loot and room examples.
- `ScriptableObject/` contains data assets for classes, skills, equipment, consumables, recipes, loot tables, boss settings, room settings and input settings.
- `NPCData/` contains sample NPC definitions, dialogue assets, NPC types/activities and trade examples.
- `Scenes/` contains `MainMenu`, `CharacterSelect`, and `Game`.
- `Animation/` contains the sample player controller and idle/run clips.
- `Audio/` contains the audio mixer.

## Important distinction

A script defines behavior or a data type; a prefab supplies a reusable GameObject hierarchy; a ScriptableObject asset supplies reusable configuration. Creating a new script alone does not automatically create a usable game object or content entry. Usually all three layers must be connected in the Inspector.
