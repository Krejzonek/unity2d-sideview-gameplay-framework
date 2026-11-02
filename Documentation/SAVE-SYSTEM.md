# Save System

## Components

- `Checkpoint` represents a checkpoint interaction/location.
- `MapSaveSystem` serializes map and runtime progress data.
- `MapAssetRegistry` resolves saved asset identifiers to project assets.

The save data types in `MapSaveSystem.cs` include records for map state, loot, enemies, rooms, characters, items, ingredients and consumables. The system also stores progression/hotbar/checkpoint/team-position information.

## Using the save flow

1. Keep the `MapSaveSystem` and `MapAssetRegistry` objects present in the scene lifecycle expected by the sample project.
2. Register content assets that must be reconstructed after loading through the asset registry workflow.
3. Use the save/load entry points exposed by `MapSaveSystem` rather than writing separate JSON files for individual systems.
4. Test a save after changing room progression, character state, inventory/equipment and checkpoint position.
5. Restart/reload and verify that every saved reference resolves to the intended asset.

## Adding a persistent field

When adding a field to persistent state, update all relevant parts together:

- the serializable data model;
- capture/write logic;
- load/restore logic;
- asset registration/resolution if the field stores an asset reference;
- compatibility/default handling for older save files;
- tests for a new save and a save created before the field existed.

Do not serialize a Unity object reference directly and expect JSON to reconstruct it. Store a stable identifier and resolve it through the registry or an equivalent explicit mapping. Keep identifiers stable when renaming assets if the registry depends on those identifiers.

## Safety

Back up save files before changing serialization code. A change to field names, identifiers or data layout can make existing saves incomplete or incompatible.
