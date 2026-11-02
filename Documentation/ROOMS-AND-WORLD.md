# Rooms and World Generation

The room system is built from room prefabs and runtime components. Core scripts include `Room`, `RoomGenerator`, `RoomPoint`, `RoomEnemySpawner`, `RoomTrigger`, `SpecialRoomData`, `RestRoom` and `ResurrectionStatue`.

## Room prefab setup

1. Duplicate a working room prefab from `Assets/Prefab/Room/`.
2. Preserve its room bounds, entry/exit points, spawn points and trigger hierarchy.
3. Configure the `Room` component and its required references.
4. Add/configure `RoomPoint` transforms for the connections expected by the generator.
5. Configure `RoomEnemySpawner` with the enemy prefabs/spawn configuration.
6. Add a `RoomTrigger` where the player should activate room behavior.
7. Test the room in the normal gameplay flow and verify that exits/progression respond to enemy completion.

## Room generation

`RoomGenerator` coordinates room creation and progression. Room prefabs must have compatible connection points and dimensions; inconsistent connection transforms can cause gaps, overlaps or disconnected rooms. Use the included test rooms as templates before authoring a new layout.

## Special rooms

`SpecialRoomData` references a room prefab. Create an asset and assign the special room prefab, then connect it to the special-room selection/configuration used by the generator. The project includes a sample special-room asset.

## Rest and resurrection rooms

`RestRoom` and `ResurrectionStatue` implement special interactions. Duplicate the corresponding sample prefab and preserve the serialized UI/interactable references. Test healing/restoration and resurrection through the actual character/team state, not only the visual UI.

## Enemy progression

Room completion is linked to the room's enemy/spawner state. When adding a new enemy type or spawn source, ensure it is registered with the room's tracking logic; otherwise the room may remain locked or unlock too early.
