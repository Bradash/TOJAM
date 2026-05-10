using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventory : MonoBehaviour
{
    public InventorySlot[] slots;
    public int selectedItemSlot = 0;
    public TMP_Text destText;
    private Item selectedItem;

    private void Start()
    {
        SelectSlot(selectedItemSlot);
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
        selectedItem = GetSelectedItem();
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
        return true;
    }

    public bool SetItemInSelectedSlot(Item item) => SetItemInSlot(selectedItemSlot, item);

    public Item GetSelectedItem()
    {
        return slots[selectedItemSlot].item;
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
        itemImage.color = Color.white;
    }
}