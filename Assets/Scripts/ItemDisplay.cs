using System;
using UnityEngine;
using UnityEngine.Serialization;


public class ItemDisplay : MonoBehaviour
{
    /// <summary>
    /// Fires when a swap-item is successfully placed into a store display
    /// at its correct destination. Used by GameManager to count win progress.
    /// </summary>
    public static event Action<ItemDisplay> OnStoreSwapCompleted;

    public TMPro.TMP_Text locationText;
    public string location;
    public ItemData itemData;
    public Transform itemPosition;
    public Transform itemParent;
    public bool storeDisplay = true;
    public bool generateRandom = true;

    private Item item;
    private GameObject itemObject;
    private ItemSystem itemSystem;
    //private Transform itemTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        itemSystem = ItemSystem.Instance;
        if (!itemParent)
        {
            itemParent = new GameObject("ItemParent").transform;
            itemParent.SetParent(transform);
            itemParent.localScale = Vector3.one;
            itemParent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        if(!itemPosition)
            itemPosition = itemParent;

        if (generateRandom)
        {
            itemData = itemSystem.GetRandomItem(storeDisplay);
        }

        if (itemData)
        {
            CreateItem();
        }
        Display();
    }

    [ContextMenu("TestItemDisplay")]
    private void TestItemDisplay()
    {
        itemSystem = FindAnyObjectByType<ItemSystem>();
        if (generateRandom)
        {
            itemData = itemSystem.GetRandomItem(storeDisplay);
        }

        if (itemData)
        {
            CreateItem();
        }
        else
        {
            Debug.LogWarning("TestItemDisplay: No item found");
        }
        Display();
    }
    public void CreateItem()
    {
        string destination = location;
        if (!storeDisplay)
            destination = itemSystem.GetRandomAvailableDisplay();
 
        itemObject = Instantiate(itemData.itemPrefab);
        item = itemObject.GetComponent<Item>();
        item.itemDestination  = destination;
        item.storeItem = storeDisplay;
        item.itemDisplay = this;
        UpdateTransform();
        itemObject.name = itemData.itemName;

    }
    private void Display()
    {
        if(locationText)
            locationText.text = location;
        
        if (!itemParent)
        {
            itemParent = new GameObject("ItemParent").transform;
            itemParent.SetParent(transform);
            itemParent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        if (!itemObject) return;
        UpdateTransform(true);
    }
    
    private void UpdateTransform(bool update = false)
    {
        if(!itemPosition)
            itemPosition = transform;
        Transform itemTransform = itemObject.transform;
        //if (update)
            itemTransform.parent = null;

        if(itemData.editScale)
            itemTransform.localScale = itemData.itemScale;
        itemTransform.SetParent(itemParent,true);
        itemTransform.localRotation = Quaternion.identity;
        itemTransform.position = itemPosition.position + itemData.itemOffset;
    }
    private void RemoveItem()
    {
        itemObject = null;
        itemData = null;
    }

    public bool TryTakeItem(out Item item)
    {
        item = this.item;
        if (this.item == null) return false;
        RemoveItem();
        Display();
        return true;
    }

    public bool TryReplaceItem(Item swapItem, out Item takeItem )
    {
        if (storeDisplay && (!swapItem || swapItem.itemDestination != location))
        {
            takeItem = null;
            return false;
        }
        takeItem = item;
        if (takeItem)
        {
            takeItem.transform.parent = null;
            takeItem.gameObject.SetActive(false);
            takeItem.itemDisplay = null;
        }
        
        item = swapItem;
        if (!item) return true;
        item.itemDisplay = this;
        itemData = swapItem.itemData;
        itemObject = item.gameObject;
        itemObject.SetActive(true);
        Display();

        if (storeDisplay)
            OnStoreSwapCompleted?.Invoke(this);

        return true;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; 
            Display();
        };
#endif
    }
}