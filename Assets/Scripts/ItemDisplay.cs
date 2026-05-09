using System;
using UnityEngine;


public class ItemDisplay : MonoBehaviour
{
    public TMPro.TMP_Text locationText;
    public string location;
    public ItemData item;
    public Transform itemPosition;
    public Transform itemParent;
    
    private GameObject itemObject;
    private Transform itemTransform;
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
            
        Display();
    }

    private void Display()
    {
        if(locationText)
            locationText.text = location;
        if (item == null)
        {
            RemoveItem();
            return;
        }
        if (!itemParent)
        {
            itemParent = new GameObject("ItemParent").transform;
            itemParent.SetParent(transform);
            itemParent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        if (itemObject == null)
        {
            if (itemParent.childCount > 0)
            {
                itemObject = itemParent.GetChild(0).gameObject;
            }
        }
        if (itemObject != null)
        {
            if (item.itemName == itemObject.name)
            {
                UpdateTransform(true);
                return;
            }
            RemoveItem();
        }
        if(item.itemPrefab == null)
            return;
        itemObject = Instantiate(item.itemPrefab);
        UpdateTransform();
        itemObject.name = item.itemName;

    }

    private void UpdateTransform(bool update = false)
    {
        if(!itemPosition)
            itemPosition = transform;
        itemTransform = itemObject.transform;
        if (update)
            itemTransform.parent = null;
        switch (item.relativeScale)
        {
            // set scale before parent to set global scale
            case true when !update:
                itemTransform.localScale.Scale(item.itemScale);
                break;
            case false:
                itemTransform.localScale = item.itemScale;
                break;
        }

        itemTransform.localScale = item.itemScale;
        itemTransform.SetParent(itemParent,true);
        itemTransform.localRotation = Quaternion.identity;
        itemTransform.position = itemPosition.position + item.itemOffset;
    }
    private void RemoveItem()
    {
        if (itemObject == null) return;
        if (Application.isPlaying)
            Destroy(itemObject);
        else
            DestroyImmediate(itemObject);
        itemObject = null;
        item = null;
    }

    public ItemData TakeItem()
    {
        if (item == null) return null;
        ItemData getItem = item;
        RemoveItem();
        Display();
        return getItem;
    }

    public ItemData ReplaceItem(ItemData newItem)
    {
        ItemData oldItem = item;
        item = newItem;
        RemoveItem();
        Display();
        return oldItem;
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
