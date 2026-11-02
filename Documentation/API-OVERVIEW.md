# API Overview

This is a map of the principal public-facing types. It is not a generated exhaustive method reference; consult the corresponding `.cs` file for exact method signatures and current Inspector fields.

| Type                                                     | Main purpose                                                          |
| -------------------------------------------------------- | --------------------------------------------------------------------- |
| `Player`                                                 | Player movement, combat and player runtime state.                     |
| `Stats`                                                  | Runtime character combat attributes.                                  |
| `CharacterData`                                          | Persistent character selection/progression data.                      |
| `CharacterManager`                                       | Character collection, selected character and character/respawn state. |
| `TeamManager`                                            | Team membership and currently controlled player.                      |
| `BotController`                                          | Bot movement/formation behavior.                                      |
| `Class`                                                  | Class ScriptableObject and stat/skill configuration.                  |
| `ClassTier`                                              | Class progression links.                                              |
| `ActiveSkill`, `PassiveSkill`                            | Active/passive skill configuration assets.                            |
| `ClassSkillManager`                                      | Runtime class-skill behavior.                                         |
| `EquipmentData`                                          | Equipment item definition.                                            |
| `EquipmentManager`                                       | Runtime equipment handling.                                           |
| `WeaponVisualProfile`                                    | Weapon prefab, hand/grip and visual-part configuration.               |
| `WeaponVisualController`                                 | Runtime weapon visual handling.                                       |
| `MeleeWeaponHitbox`                                      | Melee hitbox behavior.                                                |
| `Projectile`                                             | Projectile movement/collision and attack payload.                     |
| `EnemyAI`                                                | Standard enemy AI and damage handling.                                |
| `BossAI`                                                 | Boss-specific attack behavior.                                        |
| `BossAttackData`, `BossClassData`                        | Boss attack and boss-class configuration.                             |
| `NPC`, `NPCData`                                         | NPC scene component and data asset.                                   |
| `NPCDialogue`, `NPCDialogueManager`                      | Dialogue data and dialogue UI/runtime flow.                           |
| `NPCActivityData`, `NPCActivityContext`                  | Extensible NPC activity data and runtime context.                     |
| `NPCTraderActivity`, `NPCTradeData`                      | Trader activity and trade entries.                                    |
| `NPCBlacksmithActivity`                                  | Blacksmith activity.                                                  |
| `ConsumableData`, `IngredientData`, `CraftingRecipe`     | Consumable, ingredient and recipe data.                               |
| `LootTable`, `LootDrop`, `LootManager`                   | Loot configuration and drop handling.                                 |
| `RoomGenerator`, `Room`, `RoomPoint`, `RoomEnemySpawner` | Room layout, connections and enemy spawning.                          |
| `GameInputManager`                                       | Centralized input state and action events.                            |
| `IInteractable`                                          | Contract for world interaction components.                            |
| `MapSaveSystem`                                          | Runtime/map serialization and restoration.                            |
| `MapAssetRegistry`                                       | Asset lookup for saved data.                                          |

## Important enums

The code defines enums for gameplay input actions, enemy classes/AI types/affixes/spawn types, weapon/equipment types, item rarity, elements, status effects, active/passive skill types, consumable type/rarity, loot type, NPC trade item types and boss attack types/warning shapes. Use the existing enum values when configuring assets; add new values only together with their runtime handling.
