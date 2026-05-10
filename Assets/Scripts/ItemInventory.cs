using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ItemInventory : MonoBehaviour
{
    public InventorySlot[] slots;
    public int selectedItemSlot = 0;
    public TMP_Text destText;
    private Item selectedItem;
    [SerializeField] FPSController fpsController;
    public Color emptyColor;
    private void Start()
    {
        SelectSlot(selectedItemSlot);
        foreach (var slot in slots)
        {
            if (slot == null)
            {
                throw new NullReferenceException("InventorySlot is null");
            }
            slot.itemUI.emptyColor = emptyColor;
        }
    }
    public void SelectSlot(int slot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                throw new NullReferenceException($"InventorySlot {i} is null");
            }
            slots[i].itemUI.SelectedUIObject.SetActive(slot == i);
        }
        (selectedItem,_) = GetSelectedItem();
        if (!destText) return;
        if (selectedItem)
        {
            destText.text = selectedItem.storeItem ? "Car" : selectedItem.itemDestination;
        }
        else
            destText.text = "";
    }

    public bool SetItem(Item item)
    {
        int slot = -1;
        //find empty InventorySlot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                throw new NullReferenceException($"InventorySlot {i} is null");
            }

            if (!slots[i].HasItem)
                slot = i;
        }
        if (slot == -1) return false;
        return SetItemInSlot(slot, item);
    }
    public bool SetItemInSlot(int slot, Item item)
    {
        if (slots[slot] == null)
        {
            throw new NullReferenceException($"InventorySlot {slot} is null");
        }
        if(slots[slot].HasItem)
            return false;
        slots[slot].item = item;
        slots[slot].itemUI.UpdateImage(item.itemData);
        item.gameObject.SetActive(false);
        item.transform.parent = null;
        item.itemDisplay = null;
        InventoryUpdated(slots[slot]);
        return true;
    }

    public void SetWeightCarried() {   
        float weight = 1;
        foreach (var slot in slots)
        {
            if (slot.HasItem)
                weight += 0.5f;
        }
        fpsController.weightCarried = weight;
    }

    public bool SetItemInSelectedSlot(Item item) => SetItemInSlot(selectedItemSlot, item);

    public (Item item, int slot) GetSelectedItem()
    {
        return (slots[selectedItemSlot].item, selectedItemSlot);
    }

    public bool TryRemoveSelectedStoreItem()
    {
        InventorySlot inventorySlot = slots[selectedItemSlot];
        if (inventorySlot.HasItem && inventorySlot.item.storeItem)
        {
            Destroy(inventorySlot.item.gameObject);
            inventorySlot.item = null;
            InventoryUpdated(inventorySlot);
            return true;
        }
        return false;
    }
    public bool TryRemoveItemInSlot(int slot)
    {
        if (slots[slot] == null)
        {
            return false;
        }
        
        if (!slots[slot].HasItem)
            return false;
        slots[slot].item = null;
        slots[slot].itemUI.UpdateImage();
        InventoryUpdated(slots[slot]);
        return true;
    }

    private void InventoryUpdated(InventorySlot slot)
    {
        if (!slot.HasItem)
        {
            slot.itemUI.UpdateImage();
        }
        else
        {
            slot.itemUI.UpdateImage(slot.item.itemData);
        }
        SetWeightCarried();
    }

    public bool TryRemoveItemInSlot(int slot, out Item item)
    {
        if (slots[slot] == null)
        {
            item  = null;
            return false;
        }
        item = slots[slot].item;
        if (item == null)
            return false;
        slots[slot].item = null;
        slots[slot].itemUI.UpdateImage();
        return true;
    }
    public void SelectNextSlot()
    {
        selectedItemSlot++;
        if (selectedItemSlot >= slots.Length)
            selectedItemSlot = 0;
        SelectSlot(selectedItemSlot);
    }

    public void SelectPrevSlot()
    {
        selectedItemSlot--;
        if (selectedItemSlot < 0)
            selectedItemSlot = slots.Length - 1;
        SelectSlot(selectedItemSlot);
    }
}
[Serializable]
public class InventorySlot
{
    public bool HasItem => item != null;
    public Item item;
    public ItemUIData itemUI;

}
[Serializable]
public class ItemUIData
{
    public Color emptyColor;
    public Image itemImage;
    public GameObject SelectedUIObject;

    public void UpdateImage(ItemData data)
    {
        itemImage.sprite = data.itemSprite;
        itemImage.color = data.itemSpriteColor;
    }
    public void UpdateImage()
    {
        itemImage.sprite = null;
        itemImage.color = emptyColor;
    }
}