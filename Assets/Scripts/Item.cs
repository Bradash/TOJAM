using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData itemData;
    public string itemDestination;
    public bool storeItem;
    public ItemDisplay itemDisplay;

    public bool TryGetItemDisplay(out ItemDisplay display)
    {
        display = itemDisplay;
        return itemDisplay;
    }
}