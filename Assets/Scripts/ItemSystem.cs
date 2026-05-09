using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-20)]
public class ItemSystem : MonoBehaviour
{
    public static ItemSystem Instance { get; private set; }
    Dictionary<string, ItemDisplay> storeDisplays = new Dictionary<string, ItemDisplay>();
    List<string> availableDisplays = new List<string>();
    public List<ItemData> storeItems = new List<ItemData>();
    public List<ItemData> swappedItems = new List<ItemData>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        var displays = FindObjectsByType<ItemDisplay>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach (var display in displays)
        {
            if (display.storeDisplay)
            {
                if (storeDisplays.TryAdd(display.location, display))
                {
                    availableDisplays.Add(display.location);
                }
            }
        }
    }

    public string GetRandomAvailableDisplay()
    {
        int index = Random.Range(0, availableDisplays.Count);
        string available = availableDisplays[index];
        availableDisplays.RemoveAt(index);
        return available;
    }

    public ItemData GetRandomItem(bool store)
    {
        ItemData item = null;
        if (store)
        {
            if (storeItems.Count == 0)
            {
                Debug.LogError("Store items not found");
                return null;
            } 
            item = storeItems[Random.Range(0, swappedItems.Count)];
        }
        else
        {
            if (swappedItems.Count == 0)
            {
                Debug.LogError("Swapped items not found");
                return null;
            }
            item = swappedItems[Random.Range(0, swappedItems.Count)];
        }
        return item;
    }

    public void AddAsAvailableDisplay(string location)
    { 
        availableDisplays.Add(location);
    }


}
