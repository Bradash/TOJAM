using System;
using UnityEngine;


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

        CreateItem();
        Display();
    }

    [ContextMenu("TestItemDisplay")]
    private void TestItemDisplay()
    {
        CreateItem();
        Display();
    }
    public void CreateItem()
    {
        itemSystem = Application.isPlaying ? ItemSystem.Instance : FindAnyObjectByType<ItemSystem>();
        string destination = location;
        if (Application.isPlaying && itemSystem == null)
        {
            Debug.LogError("ItemDisplay needs an ItemSystem to prepare its item.", this);
            return;
        }
        if (Application.isPlaying && itemSystem.HasSetup)
        {
            if (!itemSystem.TryGetSpawn(this, out ItemData preparedItem, out destination)) return;
            if (generateRandom) itemData = preparedItem;
        }
        else
        {
            // Inspector previews and display-only scenes (without Setup) do not reserve destinations.
            if (generateRandom && itemSystem != null) itemData = itemSystem.GetRandomItem(storeDisplay);
            if (!storeDisplay) destination = Application.isPlaying ? "NA" : "Test 1";
        }

        if (itemData == null) return;
        if (itemData.itemPrefab == null || !itemData.itemPrefab.TryGetComponent<Item>(out _))
        {
            Debug.LogError($"Item '{itemData.name}' needs a prefab with an Item component.", this);
            return;
        }
        if (itemObject != null)
        {
            if (Application.isPlaying) Destroy(itemObject);
            else DestroyImmediate(itemObject);
        }
        itemObject = Instantiate(itemData.itemPrefab);
        item = itemObject.GetComponent<Item>();
        item.itemData = itemData;
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

    public void UpdatLocationText()
    {
        locationText.text = location;
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
