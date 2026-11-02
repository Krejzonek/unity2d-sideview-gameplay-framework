# Technical Documentation

This folder explains how to configure, use, and extend the gameplay systems in **Unity2D.SideView.GameplayFramework**.

## Contents

- [Getting Started](GETTING-STARTED.md)
- [Project Structure](PROJECT-STRUCTURE.md)
- [Architecture](ARCHITECTURE.md)
- [Characters and Teams](CHARACTERS.md)
- [Combat and Enemies](COMBAT-AND-ENEMIES.md)
- [Classes and Skills](CLASSES-AND-SKILLS.md)
- [Items, Equipment and Crafting](ITEMS-EQUIPMENT-CRAFTING.md)
- [NPC System](NPC-SYSTEM.md)
- [Rooms and World Generation](ROOMS-AND-WORLD.md)
- [Input and UI](INPUT-AND-UI.md)
- [Save System](SAVE-SYSTEM.md)
- [Extension Guide](EXTENDING.md)
- [API Overview](API-OVERVIEW.md)

## Documentation conventions

The instructions describe the scripts and assets present in the supplied `Assets` folder. Inspector fields can differ if a project version is changed. A serialized reference should be assigned to the matching component or asset type; do not assume that a field is optional unless the code checks for `null`.

This is a gameplay framework, not a plug-and-play game template. The sample scenes, prefabs, ScriptableObjects, primitive visuals, and test data are examples to build on. You are expected to provide or replace art, animation, audio, level content, and game-specific balance.
