using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-20)]
public class ItemSystem : MonoBehaviour
{
    public static ItemSystem Instance { get; private set; }
    private readonly Dictionary<string, ItemDisplay> storeDisplays = new Dictionary<string, ItemDisplay>();
    private readonly Dictionary<ItemDisplay, SpawnAssignment> spawnAssignments = new Dictionary<ItemDisplay, SpawnAssignment>();
    private bool setupComplete;
    internal bool HasSetup => setupComplete;
    public ItemSet storeItems;
    public ItemSet swappedItems;

    private readonly struct SpawnAssignment
    {
        internal readonly ItemData data;
        internal readonly string destination;

        internal SpawnAssignment(ItemData data, string destination)
        {
            this.data = data;
            this.destination = destination;
        }
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    internal void Setup()
    {
        if (setupComplete) return;
        setupComplete = true;
        List<ItemDisplay> stores = new List<ItemDisplay>();
        List<ItemDisplay> replacements = new List<ItemDisplay>();
        foreach (ItemDisplay display in FindObjectsByType<ItemDisplay>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID))
        {
            if (!display.isActiveAndEnabled || display.gameObject.scene != gameObject.scene) continue;
            if (!display.generateRandom && display.itemData != null && !IsSpawnable(display.itemData))
            {
                Debug.LogError($"Item setup failed: fixed display '{display.name}' needs an item prefab with an Item component.", display);
                return;
            }
            if (display.storeDisplay)
            {
                if (string.IsNullOrWhiteSpace(display.location) || !storeDisplays.TryAdd(display.location, display))
                {
                    Debug.LogError($"Item setup failed: store display '{display.name}' needs a unique, non-empty location.", display);
                    return;
                }
                stores.Add(display);
            }
            else if (display.generateRandom || display.itemData != null)
            {
                replacements.Add(display);
            }
        }

        ItemPairingPlanner<ItemData>.Slot[] storeSlots = CreateSlots(stores);
        ItemPairingPlanner<ItemData>.Slot[] swapSlots = CreateSlots(replacements);
        bool success = ItemPairingPlanner<ItemData>.TryCreate(storeSlots, swapSlots,
            GetSpawnableItems(storeItems), GetSpawnableItems(swappedItems),
            (store, replacement) => store.swappableItems != null && store.swappableItems.Contains(replacement),
            new System.Random(Random.Range(0, int.MaxValue)), out var plan, out string error);
        if (!success)
        {
            Debug.LogError($"Item setup failed: {error} Fixed display items were not changed.", this);
            return;
        }

        for (int i = 0; i < stores.Count; i++)
            spawnAssignments.Add(stores[i], new SpawnAssignment(plan.storeItems[i], stores[i].location));
        for (int i = 0; i < replacements.Count; i++)
            spawnAssignments.Add(replacements[i], new SpawnAssignment(plan.replacements[i], stores[plan.destinations[i]].location));
    }

    private static ItemPairingPlanner<ItemData>.Slot[] CreateSlots(List<ItemDisplay> displays)
    {
        var slots = new ItemPairingPlanner<ItemData>.Slot[displays.Count];
        for (int i = 0; i < displays.Count; i++)
        {
            ItemDisplay display = displays[i];
            slots[i] = new ItemPairingPlanner<ItemData>.Slot(display.generateRandom, display.itemData,
                $"{display.name} ({display.location})");
        }
        return slots;
    }

    private static List<ItemData> GetSpawnableItems(ItemSet set)
    {
        List<ItemData> items = new List<ItemData>();
        if (set == null || set.items == null) return items;
        foreach (ItemData data in set.items)
        {
            if (IsSpawnable(data)) items.Add(data);
        }
        return items;
    }

    private static bool IsSpawnable(ItemData data)
    {
        return data != null && data.itemPrefab != null && data.itemPrefab.TryGetComponent<Item>(out _);
    }

    internal bool TryGetSpawn(ItemDisplay display, out ItemData data, out string destination)
    {
        if (spawnAssignments.TryGetValue(display, out SpawnAssignment assignment))
        {
            data = assignment.data;
            destination = assignment.destination;
            return true;
        }
        data = null;
        destination = null;
        return false;
    }

    public ItemData GetRandomItem(bool store)
    {
        ItemSet itemSet = store ? storeItems : swappedItems;
        if (itemSet == null || itemSet.items == null || itemSet.items.Count == 0)
        {
            Debug.LogError(store ? "Store items not found" : "Swapped items not found");
            return null;
        }

        return itemSet.items[Random.Range(0, itemSet.items.Count)];
    }

    public ItemDisplay GetDisplay(string location)
    {
        return storeDisplays.GetValueOrDefault(location);
    }
}
