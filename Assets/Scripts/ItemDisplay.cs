using System;
using UnityEngine;
using UnityEngine.Serialization;


public class ItemDisplay : MonoBehaviour
{
    public TMPro.TMP_Text locationText;
    public string location;
    [FormerlySerializedAs("item")] public ItemData itemData;
    public Transform itemPosition;
    public Transform itemParent;
    public bool storeDisplay = true;
    public bool generateRandom = true;

    private Item item;
    private GameObject itemObject;
    //private Transform itemTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if(!itemPosition)
            itemPosition = transform;
        if (!itemParent)
        {
            itemParent = new GameObject("ItemParent").transform;
            itemParent.SetParent(transform);
            itemParent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        if (generateRandom)
        {
            itemData = ItemSystem.Instance.GetRandomItem(storeDisplay);
        }

        if (itemData)
        {
            CreateItem();
        }
        Display();
    }

    public void CreateItem()
    {
        string destination = location;
        if (!storeDisplay)
            destination = ItemSystem.Instance.GetRandomAvailableDisplay();
 
        itemObject = Instantiate(itemData.itemPrefab);
        item = itemObject.GetComponent<Item>();
        item.itemDestination  = destination;
        item.storeItem = storeDisplay;
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
        if (update)
            itemTransform.parent = null;
        switch (itemData.relativeScale)
        {
            // set scale before parent to set global scale
            case true when !update:
                itemTransform.localScale.Scale(itemData.itemScale);
                break;
            case false:
                itemTransform.localScale = itemData.itemScale;
                break;
        }

        //itemTransform.localScale = itemData.itemScale;
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
        if (!swapItem || swapItem.itemDestination != location)
        {
            takeItem = null;
            return false;
        }
        takeItem = item;
        if (takeItem)
        {
            takeItem.transform.parent = null;
            takeItem.gameObject.SetActive(false);
        }
        
        item = swapItem;
        itemData = swapItem.itemData;
        itemObject = item.gameObject;
        itemObject.SetActive(true);
        Display();
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