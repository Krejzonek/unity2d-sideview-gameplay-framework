# Classes and Skills

## Class assets

`Class` is a ScriptableObject containing the class name, color, description, stat bonuses, formation position, two passive-skill references and an active-skill reference. The sample assets are under `Assets/ScriptableObject/Class/`.

To create a class:

1. Create a `Class` asset using the type available in the Create Asset menu.
2. Set its display name, color and description.
3. Configure its health, stamina, attack, defence and speed bonuses.
4. Assign passive skill assets and an active skill asset where appropriate.
5. Add the class to the tier/progression configuration used by the class-selection UI.
6. Verify the class appears in `ClassSelector` and its bonuses/skills are applied in gameplay.

## Tiers and progression

`ClassTier` links a class to its possible next classes. Configure the starting tier and the next-class references so the progression graph is intentional. Avoid circular or unreachable progression links.

## Passive skills

`PassiveSkill` stores a passive-skill type and its parameters (including value, attack count and duration). Choose the type that matches the intended behavior and set only the parameters used by that type. `ClassSkillManager` evaluates the selected class's skill data during gameplay.

## Active skills

`ActiveSkill` stores its type, value, cooldown and knockback multiplier. Assign it to the class asset and test cooldown, stamina/resource behavior, effect duration and interaction with weapon attacks.

## Custom skill types

The existing enums define the behaviors understood by the runtime manager. Adding a new enum value does not implement a new skill by itself. Add the corresponding behavior to `ClassSkillManager` (and any UI/description handling), then create an asset using the new type and test it with every relevant weapon/character state.
