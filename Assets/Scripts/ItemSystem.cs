using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-20)]
public class ItemSystem : MonoBehaviour
{
    public static ItemSystem Instance { get; private set; }
    private readonly Dictionary<string, ItemDisplay> storeDisplays = new Dictionary<string, ItemDisplay>();
    private readonly List<string> availableDisplays = new List<string>();
    public ItemSet storeItems;
    public ItemSet swappedItems;
    
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
        ItemDisplay[] displays = FindObjectsByType<ItemDisplay>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach (ItemDisplay display in displays)
        {
            if (!display.storeDisplay) continue;
            if (storeDisplays.TryAdd(display.location, display))
            {
                availableDisplays.Add(display.location);
            }
        }
    }

    public string GetRandomAvailableDisplay()
    {
        if (!Application.isPlaying)
        {
            return "Test 1";
        }
        
        if (availableDisplays.Count == 0)
        {
            //None Available
            return "NA";
        }

     
        int index = Random.Range(0, availableDisplays.Count);
        string available = availableDisplays[index];
        availableDisplays.RemoveAt(index);
        return available;
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
    public void AddAsAvailableDisplay(string location)
    { 
        availableDisplays.Add(location);
    }


}
