# Characters and Teams

## Character data

`CharacterData` is the persistent character record. Its serialized data includes the character name, selected `Class`, level, current EXP, free points and the saved health/stamina/speed/attack/luck point values.

Use the character-selection flow and `CharacterManager` rather than creating a separate runtime player manually. `CharacterManager` exposes the character collection, selected character and selected index, and also tracks character switching, respawn/checkpoint and saved switch-position state.

## Character setup

1. Create or configure a `Class` ScriptableObject (see [Classes and Skills](CLASSES-AND-SKILLS.md)).
2. Create/select a character through the character-selection UI.
3. Ensure the gameplay scene contains the player prefab and the manager objects required by the scene flow.
4. Verify the player prefab has its Rigidbody2D, collider, floor-check transform, visual hierarchy, and references required by its `Player`/animation components.
5. Test the character through the normal scene flow so the selected `CharacterData` is available to the runtime player.

The sample character is assembled from 2D child objects. If you replace its art, preserve the expected transforms and weapon-holder hierarchy unless you also update the scripts that reference them.

## Team management

`TeamManager` maintains the team and exposes `CurrentPlayer` and `Team`. `BotController` handles bot behavior and formation; its `BotFormationMode` controls the formation mode. Configure bot/player references on the relevant prefab/scene objects and test:

- switching the controlled character;
- bots following/holding their formation positions;
- a controlled character dying;
- bot death and revival/respawn behavior;
- switching after a character has died.

Do not assume every bot is a fully independent player. Bot behavior is coordinated with the team/player systems.

## Stats and progression

`Stats` contains the runtime combat values, including health, defence, stamina, speed, attack, luck, EXP bonus, critical rate/damage, knockback and stun chance. `StatBonus` represents a typed stat bonus. `Player` also tracks level and current EXP. When adding a stat, update every place that reads, calculates, displays, saves or restores it; adding a field to `Stats` alone is not sufficient.
