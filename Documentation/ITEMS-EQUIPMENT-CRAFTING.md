# Items, Equipment and Crafting

## Equipment data

`EquipmentData` is a ScriptableObject describing an item. Its fields include item name/icon, equipment type, weapon type, element, optional `WeaponVisualProfile`, stat bonuses (health, defence, stamina, speed, attack and EXP), and miscellaneous item type. Sample assets are in `Assets/ScriptableObject/Equipment/`.

### Create equipment

1. Create an `EquipmentData` asset.
2. Set the name, icon and equipment slot/type.
3. For weapons, select the weapon type and element.
4. Configure stat bonuses.
5. For a visible weapon, assign a `WeaponVisualProfile`.
6. Add the item to the content source used by your game (loot table, NPC trade, starting inventory, or other item provider).
7. Test equipping, unequipping, stat recalculation, visual changes and save/load.

## Weapon visuals

`WeaponVisualProfile` stores a weapon prefab, weapon type, hand, grip position/rotation/scale and a list of `WeaponPartSettings`. A part setting identifies a part by its string name and can enable it, assign a texture and configure local position/rotation. Keep part names consistent with the child objects expected by the visual controller. The custom `WeaponVisualProfileEditor` provides editor tooling for this data.

The visual profile is separate from combat data: a correct model does not guarantee a working hitbox, and a working weapon attack does not guarantee a correctly aligned model.

## Equipment runtime

`EquipmentManager` coordinates equipment state; `EquipmentSlot`/`EquippmentSlot` components and equipment UI scripts represent the UI slots/panels. When building a new equipment UI, preserve the character/slot association and connect the UI controls to the same manager rather than maintaining a second independent equipment state.

## Consumables and ingredients

- `ConsumableData` defines consumable type/rarity, stat effect, healing, buff and regeneration values, icon and description.
- `ConsumableInventory` stores stacks of consumables.
- `ConsumableHotbar` exposes quick-use slots.
- `ConsumableManager` handles runtime use.
- `IngredientData` defines an ingredient; `IngredientInventory` stores ingredient stacks.

Create data assets first, then make them obtainable through loot, trades or other content sources. Test stack counts, use effects, hotbar updates and persistence.

## Crafting

`CraftingRecipe` defines a recipe name/icon, ingredient requirements, result consumable and result amount. Configure the recipe asset, ensure the required ingredients exist as assets, then make the recipe available to the crafting UI/cauldron flow. Test insufficient ingredients, successful crafting, inventory changes and repeated crafting.

## Loot and rarity

`LootTable`, `LootDrop`, `ChestLoot`, `LootManager`, `LootItem`, `LuckSystem`, `RarityManager` and `RarityColor` form the drop/rarity path. `LootDrop` can identify an ingredient, consumable or equipment reward and contains chance and min/max amount fields. Configure the loot table and assign it to the relevant chest/enemy/drop source. Test both successful and failed rolls and verify the correct inventory receives the reward.
