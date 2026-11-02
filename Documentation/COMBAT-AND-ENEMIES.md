# Combat and Enemies

## Player combat

The `Player` component is the main player-side combat controller. It coordinates movement/combat state, stamina, stats, equipment, skill effects and damage handling. The project supports melee hitboxes and projectile-based attacks. `Projectile` carries the attack data to the target; `MeleeWeaponHitbox` is used for melee weapon collision, while `WeaponVisualController` handles weapon presentation rather than damage rules.

### Adding a melee weapon

1. Create/configure an `EquipmentData` asset and set its equipment/weapon type, element and stat bonuses.
2. Create or reuse a weapon prefab under `Assets/Prefab/`.
3. Configure its collider/hitbox setup and ensure the hitbox component is on the object expected by the weapon controller.
4. Create/configure a `WeaponVisualProfile` and assign the weapon prefab, hand/grip settings and named part settings.
5. Assign the profile to the equipment asset.
6. Equip the item through the equipment system and test hit registration, facing, damage, knockback and status effects.

### Adding a ranged weapon

1. Create an `EquipmentData` asset with the correct ranged weapon type and element.
2. Prepare a projectile prefab with the `Projectile` component and required visual/collider setup.
3. Ensure the player/weapon setup has the correct attack point/weapon-specific attack point reference.
4. Configure the visual profile and equip the weapon.
5. Test aim direction, projectile travel, collision, damage and status effects.

The actual projectile behavior and attack parameters are controlled by the scripts and serialized fields; do not rely on the sample values as universal balance recommendations.

## Damage and effects

The project includes `ElementSystem`, `StatusEffect`, and `StatusEffectSystem`. Equipment can define an element and status-effect properties. Enemy damage handling receives combat information such as damage, knockback, stun chance, critical-hit state, status effect and attacker reference. When adding a new element or status effect, update the enum/data, application logic, ticking/removal behavior, UI if applicable, and save behavior if the effect can persist.

## Enemy AI

`EnemyAI` supports enemy classification/AI configuration through its enums and serialized fields. Enemy behavior is configured on the enemy prefab and through the room/spawner setup. To add an enemy:

1. Duplicate a suitable enemy prefab.
2. Configure its collider, Rigidbody2D, visuals/Animator and `EnemyAI` references.
3. Set the enemy class/AI type and combat parameters in the Inspector.
4. Add it to the appropriate room/spawn configuration.
5. Test target acquisition, movement, attack range, damage reception, death and room completion.

## Bosses

`BossAI` uses boss-specific configuration, including `BossClassData` and `BossAttackData`. Attack data includes attack type, cooldown, windup/recovery, distance limits, damage multiplier, knockback and warning geometry/charge settings. `BossAttackWarning` displays the telegraph. Create a boss by duplicating a working boss prefab, assigning the boss data and attack assets, and testing the warning area against the actual attack hit area.

## Combat test checklist

- Damage is applied once per intended hit.
- Critical hits and elemental multipliers are correct.
- Knockback/stun/status effects are applied and expire correctly.
- Invulnerability/defense behavior does not allow repeated unintended damage.
- Dead actors stop acting and are handled by team/room progression.
