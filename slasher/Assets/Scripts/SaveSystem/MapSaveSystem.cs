using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapSaveSystem : MonoBehaviour
{
    public static MapSaveSystem Instance { get; private set; }

    [Serializable]
    public class MapData
    {
        public List<RoomData> rooms = new();
        public int nextRoomNumber;
        public int lastRoomIndex;
        public int currentRoomNumber;
        public List<CharacterSaveData> characters = new();
        public List<bool> deadCharacters = new();
        public List<string> inventory = new();
        public List<IngredientSaveData> ingredients = new();
        public List<ConsumableSaveData> consumables = new();
        public Vector3 checkpointPosition;
        public bool hasCheckpoint;
    }

    [Serializable]
    public class RoomData
    {
        public int roomNumber;
        public string prefabName;
        public Vector3 position;
        public bool isCleared;
        public int currentPhase;
        public int totalPhases;
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string characterName;
        public string className;
        public int level;
        public int currentEXP;
        public int freePoint;
        public float healthPoint;
        public float staminaPoint;
        public float speedPoint;
        public float attackPoint;
        public float luckPoint;
        public List<string> equippedItems = new();
    }

    [Serializable]
    public class IngredientSaveData
    {
        public string itemName;
        public int amount;
    }

    [Serializable]
    public class ConsumableSaveData
    {
        public string itemName;
        public int amount;
    }

    private string SavePath => Path.Combine(Application.persistentDataPath, "mapSave.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SaveMap(RoomGenerator generator)
    {
        if (generator == null)
            return;

        MapData data = generator.GetSaveData();

        if (data == null)
            return;

        data.characters ??= new List<CharacterSaveData>();
        data.deadCharacters ??= new List<bool>();
        data.inventory ??= new List<string>();
        data.ingredients ??= new List<IngredientSaveData>();
        data.consumables ??= new List<ConsumableSaveData>();

        SaveCharacters(data);
        SaveInventories(data);

        if (CharacterManager.Instance != null)
        {
            data.checkpointPosition = CharacterManager.Instance.CheckpointPosition;
            data.hasCheckpoint = CharacterManager.Instance.HasCheckpoint;
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log("Game saved: " + SavePath);
    }

    private void SaveCharacters(MapData data)
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null)
        {
            Debug.LogError("SAVE: CharacterManager is NULL!");
            return;
        }

        data.characters.Clear();
        data.deadCharacters.Clear();

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        Debug.Log("SAVE: Characters in manager = " + manager.Characters.Count);

        for (int i = 0; i < manager.Characters.Count; i++)
        {
            CharacterData character = manager.Characters[i];

            if (character == null)
            {
                Debug.LogError("SAVE: Character at index " + i + " is NULL!");
                continue;
            }

            Debug.Log("SAVE: Processing index " + i + ", name = " + character.characterName);

            foreach (Player player in players)
            {
                if (player == null || player.GetCharacterData() != character)
                    continue;

                Stats stats = player.GetComponent<Stats>();

                if (stats != null)
                    manager.UpdateCharacterFromPlayer(character, player, stats);

                break;
            }

            CharacterSaveData saved = new CharacterSaveData
            {
                characterName = character.characterName,
                className = character.characterClass != null ? character.characterClass.className : "",
                level = character.level,
                currentEXP = character.currentEXP,
                freePoint = character.freePoint,
                healthPoint = character.healthPoint,
                staminaPoint = character.staminaPoint,
                speedPoint = character.speedPoint,
                attackPoint = character.attackPoint,
                luckPoint = character.luckPoint
            };

            if (character.equippedItems != null)
            {
                foreach (ItemData item in character.equippedItems)
                {
                    if (item != null)
                        saved.equippedItems.Add(item.itemName);
                }
            }

            data.characters.Add(saved);
            data.deadCharacters.Add(manager.IsCharacterDead(i));
        }

        Debug.Log("SAVE: Characters actually serialized = " + data.characters.Count);
    }

    private void SaveInventories(MapData data)
    {
        data.inventory.Clear();
        data.ingredients.Clear();
        data.consumables.Clear();

        EquipmentManager equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment != null)
        {
            foreach (ItemData item in equipment.GetInventory())
            {
                if (item != null)
                    data.inventory.Add(item.itemName);
            }
        }

        IngredientInventory ingredientInventory = FindFirstObjectByType<IngredientInventory>();

        if (ingredientInventory != null)
        {
            foreach (IngredientInventory.IngredientStack stack in ingredientInventory.GetIngredients())
            {
                if (stack != null && stack.ingredient != null)
                    data.ingredients.Add(new IngredientSaveData { itemName = stack.ingredient.itemName, amount = stack.amount });
            }
        }

        ConsumableInventory consumableInventory = FindFirstObjectByType<ConsumableInventory>();

        if (consumableInventory != null)
        {
            foreach (ConsumableInventory.ConsumableStack stack in consumableInventory.GetConsumables())
            {
                if (stack != null && stack.item != null)
                    data.consumables.Add(new ConsumableSaveData { itemName = stack.item.itemName, amount = stack.amount });
            }
        }
    }

    public bool TryLoadMap(out MapData data)
    {
        data = null;

        if (!File.Exists(SavePath))
            return false;

        try
        {
            data = JsonUtility.FromJson<MapData>(File.ReadAllText(SavePath));

            if (data == null || data.rooms == null || data.rooms.Count == 0)
                return false;

            data.characters ??= new List<CharacterSaveData>();
            data.deadCharacters ??= new List<bool>();
            data.inventory ??= new List<string>();
            data.ingredients ??= new List<IngredientSaveData>();
            data.consumables ??= new List<ConsumableSaveData>();

            StartCoroutine(RestoreGameDataWhenReady(data));

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to load save: " + exception.Message);
            return false;
        }
    }

    private IEnumerator RestoreGameDataWhenReady(MapData data)
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (CharacterManager.Instance != null && CharacterManager.Instance.Characters.Count > data.characters.Count)
        {
            Debug.LogWarning("RESTORE CANCELLED: CharacterManager has more characters than the save file. Keeping current characters.");
            yield break;
        }

        RestoreGameData(data);
    }

    private void RestoreGameData(MapData data)
    {
        MapAssetRegistry registry = MapAssetRegistry.Instance;

        if (registry == null)
        {
            Debug.LogError("MapAssetRegistry is missing!");
            return;
        }

        CharacterManager manager = CharacterManager.Instance;

        if (manager != null)
        {
            if (data.characters != null && data.characters.Count > 0)
            {
                foreach (CharacterSaveData saved in data.characters)
                    Debug.Log("LOADING CHARACTER: " + saved.characterName + ", saved class: " + (string.IsNullOrEmpty(saved.className) ? "NULL" : saved.className));

                manager.RestoreCharacters(data.characters, data.deadCharacters, registry);
            }

            if (data.hasCheckpoint)
                manager.SetCheckpoint(data.checkpointPosition);
        }

        EquipmentManager equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment != null)
        {
            List<ItemData> inventory = new();

            foreach (string itemName in data.inventory)
            {
                ItemData item = registry.GetItem(itemName);

                if (item != null)
                    inventory.Add(item);
                else
                    Debug.LogWarning("Missing inventory item: " + itemName);
            }

            equipment.SetInventory(inventory);
        }

        IngredientInventory ingredientInventory = FindFirstObjectByType<IngredientInventory>();

        if (ingredientInventory != null)
        {
            ingredientInventory.ClearIngredients();

            foreach (IngredientSaveData saved in data.ingredients)
            {
                IngredientData item = registry.GetIngredient(saved.itemName);

                if (item != null)
                    ingredientInventory.AddIngredient(item, saved.amount);
                else
                    Debug.LogWarning("Missing ingredient: " + saved.itemName);
            }
        }

        ConsumableInventory consumableInventory = FindFirstObjectByType<ConsumableInventory>();

        if (consumableInventory != null)
        {
            consumableInventory.ClearConsumables();

            foreach (ConsumableSaveData saved in data.consumables)
            {
                ConsumableData item = registry.GetConsumable(saved.itemName);

                if (item != null)
                    consumableInventory.AddConsumable(item, saved.amount);
                else
                    Debug.LogWarning("Missing consumable: " + saved.itemName);
            }
        }

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players)
        {
            if (player == null || player.GetCharacterData() == null || manager == null)
                continue;

            Stats stats = player.GetComponent<Stats>();

            if (stats != null)
            {
                manager.ApplyCharacterToPlayer(player.GetCharacterData(), player, stats);
                player.RefreshStats();
            }
        }

        if (equipment != null)
            equipment.RefreshEquipmentUI();

        Debug.Log("Character and inventory data restored.");
    }

    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public void SaveCharactersNow()
    {
        CharacterManager manager = CharacterManager.Instance;

        if (manager == null)
            return;

        MapData data = null;

        if (File.Exists(SavePath))
        {
            try
            {
                data = JsonUtility.FromJson<MapData>(File.ReadAllText(SavePath));
            }
            catch (Exception exception)
            {
                Debug.LogError("Failed to read save: " + exception.Message);
            }
        }

        if (data == null)
            data = new MapData();

        data.characters ??= new List<CharacterSaveData>();
        data.deadCharacters ??= new List<bool>();
        data.inventory ??= new List<string>();

        data.characters.Clear();
        data.deadCharacters.Clear();

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        for (int i = 0; i < manager.Characters.Count; i++)
        {
            CharacterData character = manager.Characters[i];

            if (character == null)
                continue;

            foreach (Player player in players)
            {
                if (player == null || player.GetCharacterData() != character)
                    continue;

                Stats stats = player.GetComponent<Stats>();

                if (stats != null)
                    manager.UpdateCharacterFromPlayer(character, player, stats);

                break;
            }

            CharacterSaveData saved = new CharacterSaveData
            {
                characterName = character.characterName,
                className = character.characterClass != null ? character.characterClass.className : "",
                level = character.level,
                currentEXP = character.currentEXP,
                freePoint = character.freePoint,
                healthPoint = character.healthPoint,
                staminaPoint = character.staminaPoint,
                speedPoint = character.speedPoint,
                attackPoint = character.attackPoint,
                luckPoint = character.luckPoint
            };

            if (character.equippedItems != null)
            {
                foreach (ItemData item in character.equippedItems)
                {
                    if (item != null)
                        saved.equippedItems.Add(item.itemName);
                }
            }

            data.characters.Add(saved);
            data.deadCharacters.Add(manager.IsCharacterDead(i));
        }

        EquipmentManager equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment != null)
        {
            data.inventory.Clear();

            foreach (ItemData item in equipment.GetInventory())
            {
                if (item != null)
                    data.inventory.Add(item.itemName);
            }
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log("CHARACTERS AND INVENTORY SAVED: " + SavePath);
    }
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save deleted: " + SavePath);
        }

        CharacterManager manager = CharacterManager.Instance;
        if (manager != null)
            manager.LoadCharactersFromSave();

        CharacterSelectUI characterSelect = FindFirstObjectByType<CharacterSelectUI>();
        if (characterSelect != null)
            characterSelect.RefreshAfterDelete();
    }
}