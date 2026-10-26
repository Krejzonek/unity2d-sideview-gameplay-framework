#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapSaveSystem : MonoBehaviour
{
    public static MapSaveSystem Instance { get; private set; }

    [SerializeField] private LootItem lootItemPrefab;

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
        public List<ItemSaveData> inventoryItems = new();
        public List<IngredientSaveData> ingredients = new();
        public List<ConsumableSaveData> consumables = new();
        public List<Vector3> teamPositions = new();
        public bool hasTeamPositions;
        public Vector3 checkpointPosition;
        public bool hasCheckpoint;
        public List<string> openedChests = new();
        public List<LootItemSaveData> lootItems = new();
    }

    [Serializable]
    public class LootItemSaveData
    {
        public LootType lootType;
        public string itemName;
        public int amount;
        public float luck;
        public Vector3 position;
    }

    [Serializable]
    public class EnemySaveData
    {
        public string prefabName;
        public Vector3 position;
        public Vector3 localScale;
        public float health;
        public float maxHealth;
        public float damage;
        public float moveSpeed;
        public float detectionRange;
        public float attackRange;
        public float attackCooldown;
        public float eliteKnockbackResistance;
        public float assassinSpeedMultiplier;
        public float supportHealAmount;
        public float supportHealCooldown;
        public float normalDetectionRange;
        public float normalMoveSpeed;
        public bool isElite;
        public EnemyAffix eliteAffix;
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
        public List<EnemySaveData> enemies = new();
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
        public List<ItemSaveData> equippedItemData = new();
        public Vector3 position;
        public bool hasPosition;
    }

    [Serializable]
    public class ItemSaveData
    {
        public string itemName;
        public ItemRarity rarity;
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
        public ConsumableRarity rarity;
    }

    private string SavePath => Path.Combine(Application.persistentDataPath, "mapSave.json");
    private readonly HashSet<string> openedChests = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadOpenedChests();
    }

    private void LoadOpenedChests()
    {
        openedChests.Clear();

        if (!File.Exists(SavePath))
            return;

        try
        {
            MapData data = JsonUtility.FromJson<MapData>(File.ReadAllText(SavePath));

            if (data?.openedChests == null)
                return;

            foreach (string chestId in data.openedChests)
            {
                if (!string.IsNullOrEmpty(chestId))
                    openedChests.Add(chestId);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to load opened chests: " + exception.Message);
        }
    }

    public bool IsChestOpened(string chestId)
    {
        return !string.IsNullOrEmpty(chestId) && openedChests.Contains(chestId);
    }

    public void RegisterOpenedChest(string chestId)
    {
        if (string.IsNullOrEmpty(chestId) || !openedChests.Add(chestId))
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

        InitializeLists(data);
        data.openedChests = new List<string>(openedChests);

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to save opened chest: " + exception.Message);
        }
    }

    public void RemoveOpenedChestsAfterRoom(int checkpointRoomNumber)
    {
        openedChests.RemoveWhere(chestId =>
        {
            int separator = chestId.IndexOf('_');

            if (separator <= 0)
                return false;

            return int.TryParse(chestId.Substring(0, separator), out int roomNumber) && roomNumber > checkpointRoomNumber;
        });
    }

    public void SaveMap(RoomGenerator generator)
    {
        if (generator == null)
            return;

        MapData data = generator.GetSaveData();

        if (data == null)
            return;

        InitializeLists(data);
        SaveCharacters(data);
        SaveInventories(data);
        SaveTeamPositions(data);
        SaveLootItems(data);
        data.openedChests = new List<string>(openedChests);

        if (CharacterManager.Instance != null)
        {
            data.checkpointPosition = CharacterManager.Instance.CheckpointPosition;
            data.hasCheckpoint = CharacterManager.Instance.HasCheckpoint;
        }

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            Debug.Log("Game saved: " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to save game: " + exception.Message);
        }
    }

    private void InitializeLists(MapData data)
    {
        data.rooms ??= new List<RoomData>();
        data.characters ??= new List<CharacterSaveData>();
        data.deadCharacters ??= new List<bool>();
        data.inventory ??= new List<string>();
        data.inventoryItems ??= new List<ItemSaveData>();
        data.ingredients ??= new List<IngredientSaveData>();
        data.consumables ??= new List<ConsumableSaveData>();
        data.teamPositions ??= new List<Vector3>();
        data.openedChests ??= new List<string>();
        data.lootItems ??= new List<LootItemSaveData>();

        foreach (CharacterSaveData character in data.characters)
        {
            if (character != null)
            {
                character.equippedItems ??= new List<string>();
                character.equippedItemData ??= new List<ItemSaveData>();
            }
        }

        foreach (RoomData room in data.rooms)
        {
            if (room != null)
                room.enemies ??= new List<EnemySaveData>();
        }
    }

    private void SaveLootItems(MapData data)
    {
        data.lootItems.Clear();

        LootItem[] lootItems = FindObjectsByType<LootItem>(FindObjectsSortMode.None);

        foreach (LootItem loot in lootItems)
        {
            if (loot == null)
                continue;

            LootDrop drop = loot.GetLootDrop();

            if (drop == null)
                continue;

            string itemName = "";

            switch (drop.lootType)
            {
                case LootType.Ingredient:
                    if (drop.ingredient != null)
                        itemName = drop.ingredient.itemName;
                    break;

                case LootType.Consumable:
                    if (drop.consumable != null)
                        itemName = drop.consumable.itemName;
                    break;

                case LootType.Equipment:
                    if (drop.equipment != null)
                        itemName = drop.equipment.itemName;
                    break;
            }

            if (string.IsNullOrEmpty(itemName))
                continue;

            data.lootItems.Add(new LootItemSaveData
            {
                lootType = drop.lootType,
                itemName = itemName,
                amount = loot.GetAmount(),
                luck = loot.GetLuck(),
                position = loot.transform.position
            });
        }

        Debug.Log("SAVE: Loot items saved = " + data.lootItems.Count);
    }

    private void RestoreLootItems(MapData data, MapAssetRegistry registry)
    {
        if (lootItemPrefab == null)
        {
            Debug.LogError("LootItem prefab is not assigned in MapSaveSystem!");
            return;
        }

        LootItem[] existingLoot = FindObjectsByType<LootItem>(FindObjectsSortMode.None);

        foreach (LootItem loot in existingLoot)
        {
            if (loot != null)
                Destroy(loot.gameObject);
        }

        if (data.lootItems == null)
            return;

        foreach (LootItemSaveData saved in data.lootItems)
        {
            if (saved == null || string.IsNullOrEmpty(saved.itemName))
                continue;

            LootDrop drop = new LootDrop
            {
                lootType = saved.lootType,
                dropChance = 100f,
                minAmount = saved.amount,
                maxAmount = saved.amount
            };

            switch (saved.lootType)
            {
                case LootType.Ingredient:
                    drop.ingredient = registry.GetIngredient(saved.itemName);
                    if (drop.ingredient == null)
                    {
                        Debug.LogWarning("Missing ground loot ingredient: " + saved.itemName);
                        continue;
                    }
                    break;

                case LootType.Consumable:
                    drop.consumable = registry.GetConsumable(saved.itemName);
                    if (drop.consumable == null)
                    {
                        Debug.LogWarning("Missing ground loot consumable: " + saved.itemName);
                        continue;
                    }
                    break;

                case LootType.Equipment:
                    drop.equipment = registry.GetItem(saved.itemName);
                    if (drop.equipment == null)
                    {
                        Debug.LogWarning("Missing ground loot equipment: " + saved.itemName);
                        continue;
                    }
                    break;

                default:
                    continue;
            }

            LootItem loot = Instantiate(lootItemPrefab, saved.position, Quaternion.identity);
            loot.SetupSaved(drop, saved.amount, saved.luck);
        }

        Debug.Log("RESTORE: Loot items restored = " + data.lootItems.Count);
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

            Player matchedPlayer = null;

            foreach (Player player in players)
            {
                if (player != null && player.GetCharacterData() == character)
                {
                    matchedPlayer = player;
                    break;
                }
            }

            if (matchedPlayer != null)
            {
                Stats stats = matchedPlayer.GetComponent<Stats>();

                if (stats != null)
                    manager.UpdateCharacterFromPlayer(character, matchedPlayer, stats);
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
                luckPoint = character.luckPoint,
                position = matchedPlayer != null ? matchedPlayer.transform.position : Vector3.zero,
                hasPosition = matchedPlayer != null
            };

            if (character.equippedItems != null)
            {
                foreach (ItemData item in character.equippedItems)
                {
                    if (item != null)
                    {
                        saved.equippedItems.Add(item.itemName);
                        saved.equippedItemData.Add(new ItemSaveData
                        {
                            itemName = item.itemName,
                            rarity = item.GetRarity()
                        });
                    }
                }
            }

            data.characters.Add(saved);
            data.deadCharacters.Add(manager.IsCharacterDead(i));
        }

        Debug.Log("SAVE: Characters actually serialized = " + data.characters.Count);
    }

    private void SaveTeamPositions(MapData data)
    {
        TeamManager teamManager = TeamManager.Instance;

        if (teamManager == null || teamManager.Team == null || teamManager.Team.Count == 0)
            return;

        data.teamPositions = new List<Vector3>();

        foreach (Player player in teamManager.Team)
            data.teamPositions.Add(player != null ? player.transform.position : Vector3.zero);

        data.hasTeamPositions = data.teamPositions.Count > 0;
    }

    public bool TryGetSavedTeamPositions(out List<Vector3> positions)
    {
        positions = null;

        if (!File.Exists(SavePath))
            return false;

        try
        {
            MapData data = JsonUtility.FromJson<MapData>(File.ReadAllText(SavePath));

            if (data == null || !data.hasTeamPositions || data.teamPositions == null || data.teamPositions.Count == 0)
                return false;

            positions = data.teamPositions;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to load team positions: " + exception.Message);
            return false;
        }
    }

    private void SaveInventories(MapData data)
    {
        EquipmentManager equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment != null)
        {
            data.inventory.Clear();
            data.inventoryItems.Clear();

            foreach (ItemData item in equipment.GetInventory())
            {
                if (item != null)
                {
                    data.inventory.Add(item.itemName);
                    data.inventoryItems.Add(new ItemSaveData
                    {
                        itemName = item.itemName,
                        rarity = item.GetRarity()
                    });
                }
            }
        }

        IngredientInventory ingredientInventory = FindFirstObjectByType<IngredientInventory>();

        if (ingredientInventory != null)
        {
            data.ingredients.Clear();

            foreach (IngredientInventory.IngredientStack stack in ingredientInventory.GetIngredients())
            {
                if (stack != null && stack.ingredient != null)
                    data.ingredients.Add(new IngredientSaveData { itemName = stack.ingredient.itemName, amount = stack.amount });
            }
        }

        ConsumableInventory consumableInventory = FindFirstObjectByType<ConsumableInventory>();

        if (consumableInventory != null)
        {
            data.consumables.Clear();

            foreach (ConsumableInventory.ConsumableStack stack in consumableInventory.GetConsumables())
            {
                if (stack != null && stack.item != null)
                    data.consumables.Add(new ConsumableSaveData
                    {
                        itemName = stack.item.itemName,
                        amount = stack.amount,
                        rarity = stack.rarity
                    });
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

            InitializeLists(data);

            openedChests.Clear();

            foreach (string chestId in data.openedChests)
            {
                if (!string.IsNullOrEmpty(chestId))
                    openedChests.Add(chestId);
            }

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
                manager.RestoreCharacters(data.characters, data.deadCharacters, registry);

            if (data.hasCheckpoint)
                manager.SetCheckpoint(data.checkpointPosition);

            for (int i = 0; i < manager.Characters.Count && i < data.characters.Count; i++)
            {
                CharacterData character = manager.Characters[i];
                CharacterSaveData saved = data.characters[i];

                if (character == null || saved == null || character.equippedItems == null || saved.equippedItemData == null)
                    continue;

                List<ItemSaveData> remaining = new(saved.equippedItemData);

                for (int j = 0; j < character.equippedItems.Count; j++)
                {
                    ItemData item = character.equippedItems[j];

                    if (item == null)
                        continue;

                    int index = remaining.FindIndex(x => x != null && x.itemName == item.itemName);

                    if (index < 0)
                        continue;

                    ItemData restoredItem = Instantiate(item);
                    restoredItem.SetRarity(remaining[index].rarity);
                    character.equippedItems[j] = restoredItem;
                    remaining.RemoveAt(index);
                }
            }
        }

        EquipmentManager equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment != null)
        {
            List<ItemData> inventory = new();

            if (data.inventoryItems != null && data.inventoryItems.Count > 0)
            {
                foreach (ItemSaveData saved in data.inventoryItems)
                {
                    if (saved == null)
                        continue;

                    ItemData item = registry.GetItem(saved.itemName);

                    if (item != null)
                    {
                        item = Instantiate(item);
                        item.SetRarity(saved.rarity);
                        inventory.Add(item);
                    }
                    else
                        Debug.LogWarning("Missing inventory item: " + saved.itemName);
                }
            }
            else
            {
                foreach (string itemName in data.inventory)
                {
                    ItemData item = registry.GetItem(itemName);

                    if (item != null)
                        inventory.Add(Instantiate(item));
                    else
                        Debug.LogWarning("Missing inventory item: " + itemName);
                }
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
                {
                    item = Instantiate(item);
                    item.rarity = saved.rarity;
                    consumableInventory.AddConsumable(item, saved.amount);
                }
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
        {
            equipment.RefreshAllWeaponVisuals();
            equipment.RefreshEquipmentUI();
        }

        RestoreLootItems(data, registry);

        Debug.Log("Character, inventory and ground loot data restored.");
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

        InitializeLists(data);
        SaveCharacters(data);
        SaveInventories(data);
        SaveTeamPositions(data);
        SaveLootItems(data);
        data.openedChests = new List<string>(openedChests);

        if (manager.HasCheckpoint)
        {
            data.checkpointPosition = manager.CheckpointPosition;
            data.hasCheckpoint = true;
        }

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            Debug.Log("CHARACTERS, INVENTORIES AND GROUND LOOT SAVED: " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to save characters: " + exception.Message);
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save deleted: " + SavePath);
        }

        openedChests.Clear();

        CharacterManager manager = CharacterManager.Instance;

        if (manager != null)
        {
            manager.ClearAllCharacters();
        }

        EquipmentManager equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment != null)
            equipment.SetInventory(new List<ItemData>());

        IngredientInventory ingredientInventory = FindFirstObjectByType<IngredientInventory>();

        if (ingredientInventory != null)
            ingredientInventory.ClearIngredients();

        ConsumableInventory consumableInventory = FindFirstObjectByType<ConsumableInventory>();

        if (consumableInventory != null)
            consumableInventory.ClearConsumables();

        CharacterSelectUI characterSelect = FindFirstObjectByType<CharacterSelectUI>();

        if (characterSelect != null)
            characterSelect.RefreshAfterDelete();

        Debug.Log("All saved game data cleared.");
    }

    private void OnApplicationQuit()
    {
        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            SaveMap(generator);
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void OnEditorPlayModeStateChanged()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode || Instance == null)
            return;

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            Instance.SaveMap(generator);
    }
#endif
}